using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Foundations.Serialization
{
    public static class SerializationExtensions
    {
        public static IDictionary<string, object> AsDictionary(
            this object source,
            bool withType = true)
        {
            IDictionary<string, JToken> jsonDictionary = source.AsJObject();
            return jsonDictionary.ToDictionary(a => a.Key, a => (object)a.Value);
        }

        public static string AsJson(
            this object source,
            bool withType = true)
        {
            return Serialize(source, withType);
        }

        public static JObject AsJObject(
            this object source,
            bool withType = true)
        {
            return (JObject)JToken.FromObject(
                source,
                JsonSerializer.CreateDefault(GetSerializerSettings(withType)));
        }

        private static string Serialize(
            object entity,
            bool withType)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }

            return JsonConvert.SerializeObject(
                entity,
                GetSerializerSettings(withType));
        }

        public static TEntity AsEntity<TEntity>(
            this string source,
            bool withType = true)
        {
            return Deserialize<TEntity>(source, withType);
        }

        public static TEntity AsEntity<TEntity>(
            this IDictionary<string, object> source,
            bool withType = false)
        {
            string json = Serialize(source, false);
            return Deserialize<TEntity>(json, true);
        }

        private static TEntity Deserialize<TEntity>(
            string json,
            bool withType)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(TEntity);
            }

            var result = (TEntity)JsonConvert.DeserializeObject(
                json,
                typeof(TEntity),
                GetSerializerSettings(withType));

            return result;
        }

        private static JsonSerializerSettings GetSerializerSettings(bool withType)
        {
            var serializerSettings = JsonConvert.DefaultSettings();

            serializerSettings.TypeNameHandling = withType ?
                TypeNameHandling.Auto :
                TypeNameHandling.None;

            return serializerSettings;
        }
    }
}
