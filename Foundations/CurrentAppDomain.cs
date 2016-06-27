using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Foundations
{
    public static class CurrentAppDomain
    {
        /// <summary>
        /// Get all of the loaded assemblies in the current app domain
        /// </summary>
        /// <returns>List of loaded assemblies OR null if System.AppDomain.CurrentDomain.GetAssemblies is not available</returns>
        public static List<Assembly> GetAssemblies()
        {
            try
            {
                var currentdomain = typeof(string)
                    .GetTypeInfo()
                    .Assembly
                    .GetType("System.AppDomain")
                    .GetRuntimeProperty("CurrentDomain")
                    .GetMethod
                    .Invoke(null, new object[] { });

                var getassemblies = currentdomain
                    .GetType()
                    .GetRuntimeMethod("GetAssemblies", new Type[] { });

                var assemblies = getassemblies
                    .Invoke(currentdomain, new object[] { }) as Assembly[];

                return assemblies.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
