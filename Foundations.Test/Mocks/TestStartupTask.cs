using Foundations.Bootstrap;

namespace Foundations.Test.Mocks
{
    public class TestStartupTask : IStartupTask
    {
        public static int CallCount { get; set; }
        public void Execute()
        {
            CallCount++;
        }
    }
}
