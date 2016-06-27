using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Foundations.Bootstrap;
using Foundations.Bootstrap.Attributes;
using Foundations.Bootstrap.Exceptions;
using Microsoft.Practices.ServiceLocation;
using Foundations.Test.Mocks;
using Xunit;

namespace Foundations.Test
{
    public class TaskerTests
    {
        [Fact]
        public void ExecutingTaskWithNoDependenciesExecutes()
        {
            var bag = new ConcurrentQueue<string>();
            var resolver = Initialize(bag);
            var tasker = new StartupTasker();

            tasker.AddStartupTask<DummyStartupTask>();
            tasker.RunTasks(resolver);

            var result = "";
            var hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTask", result);
        }

        [Fact]
        public void ExecutingTaskWithMixedDependenciesExecutesInCorrectOrder()
        {
            var bag = new ConcurrentQueue<string>();
            var resolver = Initialize(bag);
            var tasker = new StartupTasker();

            tasker.AddStartupTask<DummyStartupTaskWithAnotherDependency>();
            tasker.AddStartupTask<DummyStartupTaskWithDependency>();
            tasker.AddStartupTask<DummyStartupTask>();
            tasker.RunTasks(resolver);

            var result = "";

            var hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTask", result);

            hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTaskWithDependency", result);

            hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTaskWithAnotherDependency", result);
        }

        [Fact]
        public void ExecutingTaskWithMixedDependenciesCanExecute()
        {
            var bag = new ConcurrentQueue<string>();
            var resolver = Initialize(bag);
            var tasker = new StartupTasker();

            tasker.AddStartupTask<DummyStartupTaskWithDependency>();
            tasker.AddStartupTask<DummyStartupTask>();
            tasker.RunTasks(resolver);

            var result = "";

            var hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTask", result);

            hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTaskWithDependency", result);
        }

        [Fact]
        public void ExecutingTaskWithManyDependenciesExecutesInCorrectOrder()
        {
            var bag = new ConcurrentQueue<string>();
            var resolver = Initialize(bag);
            var tasker = new StartupTasker();

            tasker.AddAssemblyStartupTasks(new List<Assembly> {this.GetType().Assembly});
            tasker.RunTasks(resolver);

            var result = "";

            var hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTask", result);

            hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTaskWithDependency", result);

            hasItem = bag.TryDequeue(out result);
            Assert.True(hasItem);
            Assert.Equal("DummyStartupTaskWithAnotherDependency", result);
        }

        [Fact]
        public void ExecutingTaskWithMissingDependenciesThrowsException()
        {
            var bag = new ConcurrentQueue<string>();
            var resolver = Initialize(bag);
            var tasker = new StartupTasker();

            tasker.AddStartupTask<DummyStartupTaskWithDependency>();
            Assert.Throws<MissingDependencyException>(()=>tasker.RunTasks(resolver));
        }

        private IServiceLocator Initialize(ConcurrentQueue<string> executionList)
        {
            var builder = new ContainerBuilder();
            builder.Register(a => executionList);
            builder.RegisterType<TestStartupTask>();
            builder.RegisterType<DummyStartupTask>();
            builder.RegisterType<DummyStartupTaskWithDependency>();
            builder.RegisterType<DummyStartupTaskWithAnotherDependency>();
            var container = builder.Build();
            return new AutofacServiceLocator(container);
        }
    }

    public class DummyStartupTask : IStartupTask
    {
        private readonly ConcurrentQueue<string> _names;

        public DummyStartupTask(ConcurrentQueue<string> names)
        {
            _names = names;
        }

        public void Execute()
        {
            _names.Enqueue(this.GetType().Name);
        }
    }

    [Dependency(typeof(DummyStartupTask))]
    public class DummyStartupTaskWithDependency : IStartupTask
    {
        private readonly ConcurrentQueue<string> _names;

        public DummyStartupTaskWithDependency(ConcurrentQueue<string> names)
        {
            _names = names;
        }

        public void Execute()
        {
            _names.Enqueue(this.GetType().Name);
        }
    }

    [Dependency(typeof(DummyStartupTaskWithDependency))]
    [Dependency(typeof(DummyStartupTask))]
    public class DummyStartupTaskWithAnotherDependency : IStartupTask
    {
        private readonly ConcurrentQueue<string> _names;

        public DummyStartupTaskWithAnotherDependency(ConcurrentQueue<string> names)
        {
            _names = names;
        }

        public void Execute()
        {
            _names.Enqueue(this.GetType().Name);
        }
    }
}
