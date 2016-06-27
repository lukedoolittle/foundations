using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Foundations.Extensions.SimpleCQRS.Framework;

namespace Foundations.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determine if the given type is a constructable implementation of another type
        /// </summary>
        /// <typeparam name="T">The base type or interface</typeparam>
        /// <param name="instance">The type to evaluate</param>
        /// <returns>True if the given type meets the criteria, false otherwise</returns>
        public static bool IsInstantiableConcreteImplementation<T>(this Type instance)
        {
            if (instance == null)
            {
                throw new NullReferenceException();
            }

            //If we cannot assign the type to the given type, it is not an implementation at all
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(instance.GetTypeInfo()))
            {
                return false;
            }

            var typeInfo = instance.GetTypeInfo();

            //If there is no public constructor it isn't instantiable
            if (!typeInfo.DeclaredConstructors.Any(c => c.IsPublic))
            {
                return false;
            }

            //Need a public, concrete class that is not an open generic type
            return typeInfo.IsPublic && typeInfo.IsClass && !typeInfo.IsAbstract && !typeInfo.IsGenericTypeDefinition;
        }

        /// <summary>
        /// Unbind a generic type from its arguments
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>And unbound (unconstructable) generic type</returns>
        public static Type Unbind(this Type instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            return instance.GetTypeInfo().IsGenericType ? 
                    instance.GetGenericTypeDefinition() : 
                    instance;
        }

        /// <summary>
        /// Wrapper around reflection call to MakeGenericType
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="genericParameters"></param>
        /// <returns></returns>
        public static Type WithGenericParameters(
            this Type instance,
            params Type[] genericParameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            var type = instance.MakeGenericType(genericParameters);
            return type;
        }

        /// <summary>
        /// Determines if the current instance has a particular base type
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool HasBase(this Type instance, Type baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException();
            }

            if (instance == null)
            {
                return false;
            }

            return instance.GetTypeInfo().BaseType == baseType ||
                   instance.GetTypeInfo().BaseType.HasBase(baseType);
        }

        /// <summary>
        /// Creates a well formatted string of the type name with generic arguments
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string GetGenericName(this Type instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            if (!instance.GetTypeInfo().IsGenericType)
            {
                return instance.Name;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(instance.Name.Substring(0, instance.Name.LastIndexOf("`", StringComparison.Ordinal)));
            stringBuilder.Append(instance.GetTypeInfo().GenericTypeParameters.Aggregate("<",
                (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetGenericName(type)));
            stringBuilder.Append(">");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates a well formatted string of the typename with no generic arguments
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string GetNonGenericName(this Type instance)
        {
            if (!instance.GetTypeInfo().IsGenericType)
            {
                return instance.Name;
            }

            else
            {
                var argumentCount = instance.GetTypeInfo().GenericTypeParameters.Count();
                if (argumentCount == 0)
                {
                    argumentCount = instance.GetTypeInfo().GenericTypeArguments.Count();
                }
                return instance.Name.Replace($"`{argumentCount}", "");
            }
        }

        /// <summary>
        /// Gets all the attributes for a type of the given type T
        /// </summary>
        /// <typeparam name="T">The type of attribute to get</typeparam>
        /// <param name="instance">The type of the object</param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributes<T>(this Type instance)
            where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }

            var info = instance.GetTypeInfo();
            var attributes = info.GetCustomAttributes(true);

            return attributes.OfType<T>().ToList();
        }

        /// <summary>
        /// Invokes all methods on the given object whose parameters match the given arguments
        /// </summary>
        /// <param name="instance">Object to invoke method(s) on</param>
        /// <param name="parameters">Arguments to pass to method</param>
        public static void InvokeMethodMatchingParameters(
            this object instance,
            params object[] parameters)
        {
            var methodInfos = instance
                .GetMethodInfosMatchingParameterSignature(
                    parameters);

            foreach (var methodInfo in methodInfos)
            {
                var method = methodInfo
                    .MakeGenericMethodFromArguments(
                        parameters);
                method.Invoke(instance, parameters);
            }
        }

        public static MethodInfo MakeGenericMethodFromArguments(
            this MethodInfo methodInfo,
            params object[] arguments)
        {
            if (!methodInfo.IsGenericMethodDefinition)
                return methodInfo;

            var methodGenericArguments = methodInfo.GetGenericArguments();
            var methodParameterTypes = methodInfo
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            var argumentTypes = arguments
                .Select(a => a.GetType())
                .ToArray();

            var engine = new TypeMapper();

            for (var i = 0; i < argumentTypes.Length; i++)
            {
                engine.MapTypes(argumentTypes[i], methodParameterTypes[i]);
            }

            var typeLookup = engine.ParameterToArgumentTypeMapping;

            return methodInfo
                .MakeGenericMethod(
                    methodGenericArguments
                        .Select(genericArgument => typeLookup[genericArgument])
                        .ToArray());
        }

        public static IEnumerable<MethodInfo> GetMethodInfosMatchingParameterSignature(
            this object instance,
            params object[] arguments)
        {
            var potentialMethods = instance
                .GetType()
                .GetTypeInfo()
                .DeclaredMethods;

            var matches = new List<MethodInfo>();
            foreach (var potentialMethod in potentialMethods)
            {
                var methodParameters = potentialMethod.GetParameters();

                if (arguments?[0] == null)
                {
                    if (methodParameters == null || methodParameters.Length == 0)
                    {
                        matches.Add(potentialMethod);
                    }
                }
                else if (methodParameters != null &&
                         methodParameters.Length == arguments.Length)
                {
                    var doAllArgumentsSatisfyParameters = !methodParameters
                        .Where((t, i) => !CanGivenTypeSatisfyParameter(
                            t.ParameterType,
                            arguments[i].GetType()))
                        .Any();

                    if (doAllArgumentsSatisfyParameters)
                    {
                        matches.Add(potentialMethod);
                    }
                }
            }

            return matches;
        }

        private static bool CanGivenTypeSatisfyParameter(
            Type parameterType,
            Type argumentType)
        {
            if (argumentType == null)
            {
                return false;
            }

            if (parameterType == argumentType)
            {
                return true;
            }

            if (argumentType.IsConstructedGenericType)
            {
                if (parameterType.GetTypeInfo().IsGenericType &&
                    parameterType.GetGenericTypeDefinition() ==
                    argumentType.GetGenericTypeDefinition())
                {
                    return true;
                }
            }
            else if (parameterType.IsGenericParameter)
            {
                if (parameterType.GetTypeInfo().IsAssignableFrom(
                    argumentType.GetTypeInfo()))
                {
                    return true;
                }
            }
            else
            {
                if (parameterType.GetTypeInfo().IsAssignableFrom(
                    argumentType.GetTypeInfo()))
                {
                    return true;
                }
            }

            return CanGivenTypeSatisfyParameter(
                parameterType,
                argumentType.GetTypeInfo().BaseType);
        }
    }
}
