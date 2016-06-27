using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Foundations.Bootstrap
{
    public static class AutofacExtensions
    {
        /// <summary>
        /// Register all open and closed types implementing a particular interface
        /// </summary>
        /// <param name="instance">Autofac container instance</param>
        /// <param name="assemblies">Assemblies to search for interface implementors</param>
        /// <param name="openGenericInterface">Interface type to search for</param>
        /// <returns>Autofac container builder with the types registered</returns>
        public static ContainerBuilder RegisterAssemblyGenericInterfaceImplementors(
            this ContainerBuilder instance,
            IEnumerable<Assembly> assemblies,
            Type openGenericInterface)
        {
            var types = assemblies
                .Select(a => a.ExportedTypes.AsEnumerable())
                .SelectMany(a => a);

            var handlerTypes = types.Where(
                t => t.GetTypeInfo()
                      .ImplementedInterfaces.Any(
                            i => i.GetTypeInfo().IsGenericType &&
                                 i.GetGenericTypeDefinition() == openGenericInterface));

            foreach (var type in handlerTypes)
            {
                if (type.GetTypeInfo().IsGenericType)
                {
                    instance.RegisterGeneric(type).As(openGenericInterface);
                }
                else
                {
                    instance.RegisterType(type).AsImplementedInterfaces();
                }
            }

            return instance;
        }

        /// <summary>
        /// Find all types a context resolves given a type
        /// </summary>
        /// <typeparam name="T">The type for the context to resolve</typeparam>
        /// <param name="instance">The container context with registered resolution types</param>
        /// <returns>A list of types the container would create if "Resolve" were called</returns>
        public static IEnumerable<Type> ResolutionTypes<T>(this IComponentContext instance)
        {
            var registrations = instance.ComponentRegistry
                .RegistrationsFor(new TypedService(typeof(T)));

            return registrations
                .Select(registration => registration.Activator)
                .OfType<ReflectionActivator>()
                .Select(activator => activator.LimitType)
                .ToList();
        }
    }
}
