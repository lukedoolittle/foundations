using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Foundations.Serialization
{
    public static class JObjectExtensions
    {
        public static IEnumerable<JToken> AllMatchingProperties(
            this JContainer instance,
            string key)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return instance.Descendants()
                            .Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == key)
                            .Select(p => ((JProperty)p).Value);
        }
    }
}
