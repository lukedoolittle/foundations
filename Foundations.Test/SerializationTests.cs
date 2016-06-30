using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundations.Serialization;
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

        public class OAuthMock
        {
            public string OauthToken { get; set; }
            public string OauthSecret { get; set; }
        }
    }
}
