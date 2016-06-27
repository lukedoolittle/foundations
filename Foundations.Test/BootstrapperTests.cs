using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;
using Foundations.Bootstrap;
using Foundations.Bootstrap.Attributes;
using Microsoft.Practices.ServiceLocation;
using Foundations.Test.Mocks;
using Xunit;

namespace Foundations.Test
{
    public class Bootstrapper : AutofacBootstrapperBase
    {
        protected override IServiceLocator BuildContainer(
            ContainerBuilder builder, 
            IEnumerable<Assembly> assemblies)
        {
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            builder.RegisterInstance<string>("somestringhere");
            return base.BuildContainer(builder, assemblies);
        }
    }

    public class BootstrapperTests
    {
        private static object syncLock = new object();

        [Fact]
        public void RegisterModulesAndStartupsWithDefaultAssemblies()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddAllLoadedAssemblies()
                    .AddAllModules()
                    .AddAllStartupTasks()
                    .Run();

                Assert.Equal(1, TestModule.CallCount);
                Assert.Equal(1, TestStartupTask.CallCount);

                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterModulesAndStartupsWithDefaultAssembliesAllowingFrameworks()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddAllModules()
                    .AddAllStartupTasks()
                    .AddAllLoadedAssemblies(addFrameworkAssemblies: true)
                    .Run();

                Assert.Equal(1, TestModule.CallCount);
                Assert.Equal(1, TestStartupTask.CallCount);

                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterModulesAndStartupsWithDefaultAssembliesExcludingThisAssembly()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddAllLoadedAssemblies(assembliesToExclude: GetType().Assembly)
                    .AddAllModules()
                    .AddAllStartupTasks()
                    .Run();

                Assert.Equal(0, TestModule.CallCount);
                Assert.Equal(0, TestStartupTask.CallCount);

                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterSpecificModulesAndStartupTasks()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddModule<TestModule>()
                    .AddStartupTask<TestStartupTask>()
                    .Run();

                Assert.Equal(1, TestModule.CallCount);
                Assert.Equal(1, TestStartupTask.CallCount);

                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterModulesAndStartupsWithDecoratedAssemblies()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddAllDecoratedAssemblies()
                    .AddAllModules()
                    .AddAllStartupTasks()
                    .Run();

                Assert.Equal(1, TestModule.CallCount);
                Assert.Equal(1, TestStartupTask.CallCount);

                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterModulesAndStartupsWithDecoratedAssembliesGivenType()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddAllModules()
                    .AddAllStartupTasks()
                    .AddAllDecoratedAssemblies(typeof(BootstrapperAssemblyAttribute))
                    .Run();

                Assert.Equal(1, TestModule.CallCount);
                Assert.Equal(1, TestStartupTask.CallCount);

                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterModulesAndStartupsGivenDirectAssemblies()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddAllModules(GetType().Assembly)
                    .AddAllStartupTasks(GetType().Assembly)
                    .Run();

                Assert.Equal(1, TestModule.CallCount);
                Assert.Equal(1, TestStartupTask.CallCount);

                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterInstanceModuleAndStartup()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddModule(new TestModule())
                    .Run();

                Assert.Equal(1, TestModule.CallCount);

                TestModule.CallCount = 0;
            }
        }

        [Fact]
        public void RegisterModulesAndStartupsWithDefaultsAndAdditionalAssemblyRegistration()
        {
            lock (syncLock)
            {
                var bootstrapper = new Bootstrapper();

                bootstrapper
                    .AddAllLoadedAssemblies()
                    .AddAllDecoratedAssemblies()
                    .AddAssemblies(GetType().Assembly)
                    .AddAllModules()
                    .AddAllStartupTasks()
                    .Run();

                Assert.Equal(1, TestStartupTask.CallCount);
                Assert.Equal(1, TestModule.CallCount);
                
                TestModule.CallCount = 0;
                TestStartupTask.CallCount = 0;
            }
        }
    }
}
