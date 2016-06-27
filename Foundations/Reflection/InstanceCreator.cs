using System;
using System.Collections.Generic;

namespace Foundations.Reflection
{
    /// <summary>
    /// Provides a fluent way to create an instance using reflection
    /// </summary>
    public class InstanceCreator
    {
        private readonly Type _type;
        private readonly List<Type> _genericArguments;
        private readonly List<object> _constructorParameters;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">The type to create</param>
        public InstanceCreator(Type type)
        {
            _type = type;

            _genericArguments = new List<Type>();
            _constructorParameters = new List<object>();
        }

        /// <summary>
        /// Add a generic argument
        /// </summary>
        /// <param name="generic"></param>
        /// <returns></returns>
        public InstanceCreator AddGenericParameter(Type generic)
        {
            _genericArguments.Add(generic);

            return this;
        }

        /// <summary>
        /// Add a constructor argument
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public InstanceCreator AddConstructorParameter(object parameter)
        {
            _constructorParameters.Add(parameter);

            return this;
        }

        public T Create<T>()
        {
            Type concreteType = _type;
            if (_genericArguments.Count > 0)
            {
                concreteType = concreteType.MakeGenericType(
                    _genericArguments.ToArray());
            }

            if (_constructorParameters.Count == 0)
            {
                return (T)Activator.CreateInstance(
                    concreteType,
                    null);
            }
            else
            {
                return (T)Activator.CreateInstance(
                    concreteType,
                    _constructorParameters.ToArray());
            }
        }
    }
}
