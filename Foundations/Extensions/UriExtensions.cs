using System;

namespace Foundations.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        /// Gets a fragment of the uri containing the scheme and the host
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string NonPath(this Uri instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            return $"{instance.Scheme}://{instance.Authority}/";
        }
    }
}
