using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundations.Serialization;
using Newtonsoft.Json;
using Xunit;

namespace Foundations.Test
{
    public class SerializationTests
    {
        public class Something
        {
            public string SomeProperty { get; set; }
        }

        [Fact]
        public void SerializingBeforeDefaultSerializerIsSetWorks()
        {
            var myObject = new Something {SomeProperty = "blah blah"};

            var actual = myObject.AsJson();

            Assert.False(string.IsNullOrEmpty(actual));
        }

        [Fact]
        public void SerializingAQuerystringWorks()
        {
            var target = "OauthToken=jlkasdjflasjdf&OauthSecret=lajsdflkjasdlkf";

            var querystring = HttpUtility.ParseQueryString(target);
            var actual = querystring.AsEntity<OAuthMock>();

            Assert.NotNull(actual.OauthToken);
            Assert.NotEmpty(actual.OauthToken);
            Assert.NotNull(actual.OauthSecret);
            Assert.NotEmpty(actual.OauthSecret);
        }

        [Fact]
        public void SerializingAndThenDeserializingAnObjectWithDatetimeOffsetPreservesTime()
        {
            var target = new OAuthMock();
            target.SetDateCreated(DateTimeOffset.Now);

            var intermediate = target.AsJson();
            var actual = intermediate.AsEntity<OAuthMock>();

            Assert.Equal(target.DateCreated, actual.DateCreated);
        }

        public class OAuthMock : OAuthMockBase
        {
            public string OauthToken { get; set; }
            public string OauthSecret { get; set; }
        }

        public class OAuthMockBase
        {
            [JsonProperty("dateCreated")]
            public DateTimeOffset DateCreated { get; protected set; }

            public void SetDateCreated(DateTimeOffset date)
            {
                DateCreated = date;
            }
        }
    }
}
