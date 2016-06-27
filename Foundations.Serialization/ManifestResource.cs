using System.IO;
using System.Reflection;

namespace Foundations.Serialization
{
    public static class ManifestResource
    {
        public static T GetResourceAsObject<T>(
            string resourceName,
            Assembly assembly)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    var document = reader.ReadToEnd();
                    return document.AsEntity<T>();
                }
            }
        }
    }
}
