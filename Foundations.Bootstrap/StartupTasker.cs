using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Foundations.Bootstrap.Attributes;
using Foundations.Bootstrap.Exceptions;
using Foundations.Extensions;
using Microsoft.Practices.ServiceLocation;

namespace Foundations.Bootstrap
{
    public class StartupTasker
    {
        private readonly int _circularDependencyThreshold;
        private readonly Queue<Type> _taskQueue = new Queue<Type>(); 
        private readonly List<Type> _executedStartupTaskTypes = new List<Type>();

        public StartupTasker(int circularDependencyThreshold = 500)
        {
            _circularDependencyThreshold = circularDependencyThreshold;
        }

        public void AddStartupTask<TTask>()
            where TTask : IStartupTask
        {
            _taskQueue.Enqueue(typeof(TTask));
        }

        public void AddAssemblyStartupTasks(IEnumerable<Assembly> assemblies)
        {
            var startupTaskTypes = assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.IsInstantiableConcreteImplementation<IStartupTask>())
                .ToList();

            foreach (var startupTaskType in startupTaskTypes)
            {
                _taskQueue.Enqueue(startupTaskType);
            }
        }

        public void RunTasks(IServiceLocator resolver)
        {
            var numberOfQueueIterations = 0;

            while (_taskQueue.Count != 0)
            {
                if (numberOfQueueIterations > _circularDependencyThreshold)
                {
                    throw new CircularDependencyException();
                }

                var taskType = _taskQueue.Dequeue();
                var dependencies = taskType
                    .GetCustomAttributes<DependencyAttribute>()
                    .Select(d => d.DependsOn)
                    .ToList();

                //If there are no dependencies OR all the have been executed, then execute
                if (dependencies.Count == 0 ||
                    dependencies.IsSubsetOf(_executedStartupTaskTypes))
                {
                    ((IStartupTask)resolver.GetService(taskType)).Execute();
                    _executedStartupTaskTypes.Add(taskType);
                }
                else
                {
                    //Otherwise check each dependency that has not been executed
                    var nonExecutedDependencies = dependencies
                        .Where(d => !_executedStartupTaskTypes.Contains(d));

                    foreach (var dependency in nonExecutedDependencies)
                    {
                        //If that dependency isn't in the queue throw and exception
                        if (!_taskQueue.Contains(dependency))
                        {
                            throw new MissingDependencyException(
                                taskType,
                                dependency);
                        }
                    }

                    //Otherwise throw this at the back of the queue
                    _taskQueue.Enqueue(taskType);

                    //NOTE: if you have a circular dependency this will loop forever
                }

                numberOfQueueIterations++;
            }
        }
    }
}
