using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Foundations.Bootstrap.Attributes;
using Foundations.Extensions;
using Microsoft.Practices.ServiceLocation;
using Module = Autofac.Module;

namespace Foundations.Bootstrap
{
    /// <summary>
    /// Base class for any Autofac bootstrapper
    /// </summary>
    public abstract class AutofacBootstrapperBase
    {
        protected readonly ContainerBuilder _builder = new ContainerBuilder();
        protected readonly StartupTasker _tasker = new StartupTasker();
        protected readonly List<Assembly> _registeredAssemblies = new List<Assembly>();

        protected bool _addAllStartupTasksInRegisteredAssemblies = false;
        protected bool _addAllModulesInRegisteredAssemblies = false;

        /// <summary>
        /// Executes all of the registered bootstrapper actions
        /// </summary>
        /// <returns>A service locator populated with the dependency injection container</returns>
        public virtual IServiceLocator Run()
        {
            var resolver = BuildContainer(
                _builder, 
                _registeredAssemblies);
            ServiceLocator.SetLocatorProvider(() => resolver);

            RunStartupTasks(
                _tasker, 
                _registeredAssemblies, 
                resolver);

            return resolver;
        }

        /// <summary>
        /// Constructs the dependency injection container
        /// </summary>
        /// <returns>The DI container wrapped in a service locator</returns>
        protected virtual IServiceLocator BuildContainer(
            ContainerBuilder builder, 
            IEnumerable<Assembly> assemblies)
        {
            if (_addAllModulesInRegisteredAssemblies)
            {
                builder.RegisterAssemblyModules(assemblies.ToArray());
            }
            var container = builder.Build();
            return new AutofacServiceLocator(container);
        }

        /// <summary>
        /// Runs the registered startup tasks
        /// </summary>
        /// <param name="tasker">Task runner used to run startup tasks</param>
        /// <param name="assemblies">Assemblies to pull tasks from</param>
        /// <param name="resolver">Used to instantiate tasks to run</param>
        protected virtual void RunStartupTasks(
            StartupTasker tasker,
            IEnumerable<Assembly> assemblies,
            IServiceLocator resolver)
        {
            if (_addAllStartupTasksInRegisteredAssemblies)
            {
                tasker.AddAssemblyStartupTasks(assemblies);
            }
            tasker.RunTasks(resolver);
        }

        #region Assemblies

        /// <summary>
        /// Add all assemblies decorated with the given attribute type
        /// </summary>
        /// <param name="attributeType">The type of the attribute on the assembly or MarketAssemblyAttribute if not provided</param>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAllDecoratedAssemblies(
            Type attributeType = null)
        {
            if (attributeType == null)
            {
                attributeType = typeof(BootstrapperAssemblyAttribute);
            }

            var markedAssemblies = CurrentAppDomain
                .GetAssemblies()
                .Where(a => a.HasCustomAttribute(attributeType))
                .ToList();

            _registeredAssemblies.AddUnique(markedAssemblies);

            return this;
        }

        /// <summary>
        /// Adds all loaded assemblies to the registered assembly list
        /// </summary>
        /// <param name="addDynamicAssemblies">Indicates if dynamic assemblies should be included</param>
        /// <param name="addFrameworkAssemblies">Indicates if .NET assemblies should be included</param>
        /// <param name="assemblyNamesToExclude">List of assembly names to exclude</param>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAllLoadedAssemblies(
            string[] assemblyNamesToExclude,
            bool addDynamicAssemblies = false,
            bool addFrameworkAssemblies = false)
        {
            var assembliesToExclude = assemblyNamesToExclude.GetAssembliesFromNames();
            return AddAllLoadedAssemblies(
                addDynamicAssemblies, 
                addFrameworkAssemblies, 
                assembliesToExclude.ToArray());
        }

        /// <summary>
        /// Adds all loaded assemblies to the registered assembly list
        /// </summary>
        /// <param name="addDynamicAssemblies">Indicates if dynamic assemblies should be included</param>
        /// <param name="addFrameworkAssemblies">Indicates if .NET assemblies should be included</param>
        /// <param name="assembliesToExclude">List of assemblies to exclude</param>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAllLoadedAssemblies(
            bool addDynamicAssemblies = false,
            bool addFrameworkAssemblies = false,
            params Assembly[] assembliesToExclude)
        {
            var loadedAssemblies = CurrentAppDomain.GetAssemblies();

            if (!addDynamicAssemblies)
            {
                loadedAssemblies.RemoveAll(a => a.IsDynamic);
            }

            if (!addFrameworkAssemblies)
            {
                loadedAssemblies.RemoveAll(a => a.IsFrameworkAssembly());
            }

            if (assembliesToExclude != null && assembliesToExclude.Length != 0)
            {
                foreach (var assemblyToExclude in assembliesToExclude)
                {
                    loadedAssemblies.Remove(assemblyToExclude);
                }
            }

            _registeredAssemblies.AddUnique(loadedAssemblies);

            return this;
        }

        /// <summary>
        /// Adds the given assemblies to the registered assembly list
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAssemblies(
            params Assembly[] assemblies)
        {
            _registeredAssemblies.AddUnique(assemblies.ToList());

            return this;
        }
        #endregion Assemblies

        #region Modules
        /// <summary>
        /// Adds a specific autofac module
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddModule<TModule>()
            where TModule : Module, new()
        {
            _builder.RegisterModule<TModule>();

            return this;
        }

        /// <summary>
        /// Adds a specific autofac module
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddModule<TModule>(TModule module)
            where TModule : Module, new()
        {
            _builder.RegisterModule(module);

            return this;
        }

        /// <summary>
        /// Adds all autofac modules in the bootstrappers assembly list
        /// </summary>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAllModules()
        {
            _addAllModulesInRegisteredAssemblies = true;
            
            return this;
        }

        /// <summary>
        /// Adds all autofac modules in the given assembly list
        /// </summary>
        /// <param name="assemblies">Assemblies to pull modules from</param>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAllModules(
            params Assembly[] assemblies)
        {
            _builder.RegisterAssemblyModules(assemblies);

            return this;
        }
        #endregion Modules

        #region Startup Tasks
        /// <summary>
        /// Add a specific startup task to the startup task list
        /// </summary>
        /// <typeparam name="TTask">The type of the task to add</typeparam>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddStartupTask<TTask>()
            where TTask : IStartupTask
        {
            _tasker.AddStartupTask<TTask>();

            return this;
        }

        /// <summary>
        /// Add all startup tasks from assemblies in the bootstrapper assembly list
        /// </summary>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAllStartupTasks()
        {
            _addAllStartupTasksInRegisteredAssemblies = true;

            return this;
        }

        /// <summary>
        /// Add all startup tasks from assemblies in the bootstrapper assembly list
        /// </summary>
        /// <param name="assemblies">The assemblies to pull startup tasks from</param>
        /// <returns>Current bootstrapper instance</returns>
        public virtual AutofacBootstrapperBase AddAllStartupTasks(
            params Assembly[] assemblies)
        {
            _tasker.AddAssemblyStartupTasks(assemblies);

            return this;
        }

        #endregion Startup Tasks

    }
}
