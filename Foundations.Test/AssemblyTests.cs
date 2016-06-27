using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Foundations.Extensions;
using Xunit;

namespace Foundations.Test
{
    public class AssemblyTests
    {
        [Fact]
        public void GetMatchingAssembliesReturnsValidMatches()
        {
            var expected = new List<Assembly>
            {
                typeof(FactAttribute).Assembly,
                typeof(ContainerBuilder).Assembly
            };

            var names = new List<string>
            {
                "Autofac",
                "xunit.core"
            };

            var actual = names.GetAssembliesFromNames();

            Assert.Equal(expected.Count, actual.Count());

            foreach (var assembly in expected)
            {
                Assert.True(actual.Contains(assembly));
            }
        }

        [Fact]
        public void GetMatchingAssembliesWithSomeInvalidNamesStillWorks()
        {
            var expected = new List<Assembly>
            {
                typeof(FactAttribute).Assembly,
                typeof(ContainerBuilder).Assembly
            };

            var names = new List<string>
            {
                "Autofac",
                "xunit.core",
                "blah blah blah"
            };

            var actual = names.GetAssembliesFromNames();

            foreach (var assembly in expected)
            {
                Assert.True(actual.Contains(assembly));
            }
        }
    }
}
