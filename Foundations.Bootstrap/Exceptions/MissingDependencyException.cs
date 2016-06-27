using System;

namespace Foundations.Bootstrap.Exceptions
{
    public class MissingDependencyException : Exception
    {
        public MissingDependencyException(Type dependentType, Type missingType) :
            base($"Type {dependentType.Name} depends on {missingType.Name}")
        {
            
        }
    }
}
