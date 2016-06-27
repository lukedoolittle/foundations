using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Foundations.Serialization
{
    public class PrivateMembersContractResolver :DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(
            Type type,
            MemberSerialization memberSerialization)
        {
            var fields =
                type.GetRuntimeFields()
                    .Where(f => !f.IsLiteral && !f.IsStatic)
                    .Select(a => base.CreateProperty(a, memberSerialization));
            var properties =
                type.GetRuntimeProperties()
                    .Where(
                        p =>
                            p.GetMethod != null &&
                            !p.GetMethod.IsStatic &&
                            p.SetMethod != null &&
                            !p.SetMethod.IsStatic)
                    .Select(a => base.CreateProperty(a, memberSerialization));

            var props = properties.Union(fields).ToList();

            foreach (var prop in props)
            {
                prop.Writable = true;
                prop.Readable = true;
            }

            var setterlessProperties =
                type.GetRuntimeProperties()
                    .Where(p => p.GetMethod != null && p.SetMethod == null)
                    .Select(a => base.CreateProperty(a, memberSerialization));

            foreach (var prop in setterlessProperties)
            {
                prop.Writable = false;
                prop.Readable = true;
            }

            //This will remove any indexer attributes
            props.RemoveAll(a => a.PropertyName == "Item");

            //This will remove any backingProperties
            props.RemoveAll(a => a.PropertyName.Contains("k__BackingField"));

            props = props.Union(setterlessProperties).ToList();

            return props;
        }
    }
}
