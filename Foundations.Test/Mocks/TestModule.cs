using Autofac;

namespace Foundations.Test.Mocks
{
    public class TestModule : Module
    {
        public static int CallCount { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            CallCount++;
        }
    }
}
