using System;

namespace Foundations.Bootstrap.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BootstrapperAssemblyAttribute : Attribute
    {
        public object Parameter { get; }

        public BootstrapperAssemblyAttribute(object parameter = null)
        {
            Parameter = parameter;
        }
    }
}
