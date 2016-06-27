using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Foundations.Extensions
{
    public static class AssemblyExtensions
    {
        private static readonly List<string> _frameworkAssemblyCompanyNames = new List<string>
        {
            "Microsoft Corporation",
            "Microsoft",
            "Mono development team",
            "Mono",
            "Xamarin, Inc.",
            "Xamarin Inc."
        };

        /// <summary>
        /// Determines if the assembly is a Microsoft assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool IsFrameworkAssembly(this Assembly assembly)
        {
            var attributes = assembly.GetCustomAttributes(
                typeof(AssemblyCompanyAttribute));

            return attributes.OfType<AssemblyCompanyAttribute>()
                        .Any(a => _frameworkAssemblyCompanyNames.Contains(a.Company) );
        }

        /// <summary>
        /// Determines if an assembly has an attribute
        /// </summary>
        /// <param name="instance">The assembly to check</param>
        /// <param name="attributeType">The attribute type to check for</param>
        /// <returns>True if the assembly has the given attribute type, false otherwise</returns>
        public static bool HasCustomAttribute(
            this Assembly instance,
            Type attributeType)
        {
            if (instance == null)
            {
                throw new NullReferenceException();
            }

            return instance.GetCustomAttributes(attributeType).Any();
        }

        /// <summary>
        /// Gets all loaded assemblies matching a set of names
        /// </summary>
        /// <param name="instance">The list of names to match</param>
        /// <returns>List of distinct loaded assemblies with names matching the passed in names</returns>
        public static IEnumerable<Assembly> GetAssembliesFromNames(this IEnumerable<string> instance)
        {
            if (instance == null)
            {
                throw new NullReferenceException();
            }

            var loadedAssemblies = CurrentAppDomain.GetAssemblies();

            if (loadedAssemblies != null)
            {
                return instance
                    .Select(name => loadedAssemblies.FirstOrDefault(a => a.GetName().Name == name))
                    .Where(assembly => assembly != null)
                    .Distinct()
                    .ToList();
            }
            else
            {
                return new List<Assembly>();
            }
        }
    }
}
