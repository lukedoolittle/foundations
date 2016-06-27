using System;

namespace Foundations.Bootstrap.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DependencyAttribute : Attribute
    {
        public Type DependsOn { get; }

        public DependencyAttribute(Type dependsOn)
        {
            DependsOn = dependsOn;
        }
    }
}
