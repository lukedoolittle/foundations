using System;
using System.Reflection;
using Foundations.Attributes;

namespace Foundations.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Converts an enum to a string defined by metadata
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string EnumToString(this Enum instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            FieldInfo fi = instance.GetType().GetRuntimeField(instance.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return instance.ToString();
        }

        //TODO: this may not be working the way we would expect (via metadata)
        /// <summary>
        /// Converts a string to an enum based on the enums metadata
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum to convert to</typeparam>
        /// <param name="instance">The string in question</param>
        /// <returns></returns>
        public static TEnum StringToEnum<TEnum>(this string instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var enumType = typeof(TEnum);
            string[] names = Enum.GetNames(enumType);
            foreach (string name in names)
            {
                if (((Enum)Enum.Parse(enumType, name)).EnumToString().Equals(instance))
                {
                    return (TEnum)Enum.Parse(enumType, name);
                }
            }

            throw new ArgumentException();
        }
    }
}
