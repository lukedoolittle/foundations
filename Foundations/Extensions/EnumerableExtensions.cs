using System.Collections.Generic;
using System.Linq;

namespace Foundations.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determine if the intersection of this and another list is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="set"></param>
        /// <returns>True if the intersect is empty, false otherwise</returns>
        public static bool IntersectIsEmptySet<T>(
            this IEnumerable<T> instance, 
            IEnumerable<T> set)
        {
            return !instance.All(set.Contains);
        }

        /// <summary>
        /// Determines if an object is a subset of another given collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static bool IsSubsetOf<T>(
            this IEnumerable<T> instance,
            IEnumerable<T> set)
        {
            return instance.All(set.Contains);
        }
    }
}
