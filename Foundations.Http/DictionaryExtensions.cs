using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;

namespace Foundations.Http
{
    public static class DictionaryExtensions
    {
        public static ILookup<string, string> ToLookup(
            this NameValueCollection instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }
            return instance.AllKeys.ToLookup(a => a, a => instance[a]);
        }

        public static List<Cookie> ToList(
            this CookieCollection instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            var cookies = new List<Cookie>();

            for (var i = 0; i < instance.Count; i++)
            {
                cookies.Add(instance[i]);
            }

            return cookies;
        }
    }
}
