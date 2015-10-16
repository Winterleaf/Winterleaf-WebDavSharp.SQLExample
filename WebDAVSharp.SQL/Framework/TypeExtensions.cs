using System;
using System.Collections.Generic;
using System.Linq;

namespace WebDAVSharp.SQL.Framework
{
    /// <summary>
    ///     Extension methods for the Type class
    /// </summary>
    public static class TypeExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Loads the configuration from assembly attributes
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the custom attribute to find.
        /// </typeparam>
        /// <param name="typeWithAttributes">
        ///     The calling assembly to search.
        /// </param>
        /// <returns>
        ///     The custom attribute of type T, if found.
        /// </returns>
        public static T GetAttribute<T>(this Type typeWithAttributes) where T : Attribute
        {
            return GetAttributes<T>(typeWithAttributes).FirstOrDefault();
        }

        /// <summary>
        ///     Loads the configuration from assembly attributes
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the custom attribute to find.
        /// </typeparam>
        /// <param name="typeWithAttributes">
        ///     The calling assembly to search.
        /// </param>
        /// <returns>
        ///     An enumeration of attributes of type T that were found.
        /// </returns>
        public static IEnumerable<T> GetAttributes<T>(this Type typeWithAttributes) where T : Attribute
        {
            // Try to find the configuration attribute for the default logger if it exists
            object[] configAttributes = Attribute.GetCustomAttributes(typeWithAttributes, typeof (T), false);

            // get just the first one
            if (configAttributes.Length <= 0) yield break;
            foreach (T attribute in configAttributes)
            {
                yield return attribute;
            }
        }

        #endregion
    }
}