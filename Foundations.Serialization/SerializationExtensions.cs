using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Foundations.Serialization
{
    public static class SerializationExtensions
    {
        static SerializationExtensions()
        {
            try
            {
                JsonConvert.DefaultSettings();
            }
            catch (NullReferenceException)
            {
                var contractResolver = new PrivateMembersContractResolver();
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    ContractResolver = contractResolver,
                    TypeNameHandling = TypeNameHandling.All,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateParseHandling = DateParseHandling.None,
                    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                };
            }
        }

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

        public static TEntity AsEntity<TEntity>(
            this HttpValueCollection source,
            bool withType = true)
        {
            var dictionary = source
                .ToDictionary<HttpValue, string, object>(
                    value => value.Key, 
                    value => value.Value);

            if (withType)
            {
                var rootType = typeof(TEntity);
                var typedDictionary = new Dictionary<string, object>
                {
                    {"$type", $"{rootType.FullName}, {rootType.GetTypeInfo().Assembly.GetName().Name}"}
                };
                foreach (var item in dictionary)
                {
                    typedDictionary.Add(item.Key, item.Value);
                }
                dictionary = typedDictionary;
            }

            return dictionary.AsEntity<TEntity>(true);
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
