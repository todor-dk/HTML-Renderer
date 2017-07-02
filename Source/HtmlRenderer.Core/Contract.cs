/*
 * **************************************************************************
 *
 * Copyright (c) Todor Todorov / Scientia Software. 
 *
 * This source code is subject to terms and conditions of the 
 * license agreement found in the project directory. 
 * See: $(ProjectDir)\LICENSE.txt ... in the root of this project.
 * By using this source code in any fashion, you are agreeing 
 * to be bound by the terms of the license agreement.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Internal;

namespace Scientia.HtmlRenderer
{
    /// <summary>
    /// This class contains utility methods to validate the contract between a
    /// method implementor and the user of a method.
    /// <para/>
    /// Most commonly used is the <see cref="RequiresNotNull"/> method which
    /// ensures that arguments are not null.
    /// </summary>
    /// <remarks>
    /// The validations in this class are only enabled for debug builds.
    /// For release builds, the validations are disabled.
    /// </remarks>
    internal static class Contract
    {
        /// <summary>
        /// Validate that the given value is not null.
        /// </summary>
        /// <typeparam name="TValue">Type of the value to be validated.</typeparam>
        /// <param name="value">Value to validate for null.</param>
        /// <param name="name">Name of the argument (value) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given value is null.</exception>
        [System.Diagnostics.Contracts.ContractAbbreviator]
        [System.Diagnostics.Contracts.ContractArgumentValidator]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequiresNotNull<TValue>([ValidatedNotNull] TValue value, string name)
        {
#if DEBUG
            if (value == null)
                throw new ArgumentNullException(name);
#endif
        }

        /// <summary>
        /// Validate that the given value is null.
        /// </summary>
        /// <typeparam name="TValue">Type of the value to be validated.</typeparam>
        /// <param name="value">Value to validate for null.</param>
        /// <param name="name">Name of the argument (value) being validated.</param>
        /// <exception cref="ArgumentException"> is thrown if the given value is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequiresNull<TValue>(TValue value, string name)
        {
#if DEBUG
            if (value != null)
                throw new ArgumentException("Expected null", name);
#endif
        }

        /// <summary>
        /// Validate that the given string is not null or empty.
        /// </summary>
        /// <param name="value">String to validate.</param>
        /// <param name="name">Name of the argument (string) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given string is null or empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequiresNotEmpty(string value, string name)
        {
#if DEBUG
            if (String.IsNullOrEmpty(value))
                throw new ArgumentNullException(name);
#endif
        }

        /// <summary>
        /// Validate that the given Guid is not empty.
        /// </summary>
        /// <param name="value">Guid to validate.</param>
        /// <param name="name">Name of the argument (Guid) being validated.</param>
        /// <exception cref="ArgumentException"> is thrown if the given Guid is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequiresNotEmpty(Guid value, string name)
        {
#if DEBUG
            if (value.Equals(Guid.Empty))
                throw new ArgumentException(name);
#endif
        }

        /// <summary>
        /// Validate that the given array is not null and that it is not empty.
        /// </summary>
        /// <typeparam name="TValue">Type of the value to be validated.</typeparam>
        /// <param name="value">Array to validate for null.</param>
        /// <param name="name">Name of the argument (value) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given value is null.</exception>
        [System.Diagnostics.Contracts.ContractAbbreviator]
        [System.Diagnostics.Contracts.ContractArgumentValidator]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequiresNotEmpty<TValue>([ValidatedNotNull] TValue[] value, string name)
        {
#if DEBUG
            if ((value == null) || (value.Length == 0))
                throw new ArgumentNullException(name);
#endif
        }

        /// <summary>
        /// Validate that the given string is not null, empty or contains only whitespaces.
        /// </summary>
        /// <param name="value">String to validate.</param>
        /// <param name="name">Name of the argument (string) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given string is null, empty or contains only whitespaces.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequiresNotEmptyOrWhiteSpace(string value, string name)
        {
#if DEBUG
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(name);
#endif
        }

        /// <summary>
        /// Validate that the given value is of the requested type.
        /// </summary>
        /// <typeparam name="TValue">Type of the value to be validated.</typeparam>
        /// <typeparam name="TRequired">Type that the tests expects the value to be.</typeparam>
        /// <param name="value">Value to validate.</param>
        /// <param name="name">Name of the argument (string) being validated.</param>
        /// <exception cref="ArgumentException"> if the given value <paramref name="value"/> is not of the requested type <typeparamref name="TRequired"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "NA")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequiresType<TValue, TRequired>(TValue value, string name)
        {
#if DEBUG
            if (!(value is TRequired))
            {
                throw new ArgumentException(
                    String.Format(
                        "Expected argument to be of type {0} but argument was {1}",
                        typeof(TRequired),
                        (value == null) ? null : value.GetType()),
                    name);
            }
#endif
        }

        /// <summary>
        /// Validate that the given collection does not contain null elements.
        /// </summary>
        /// <typeparam name="TValue">The type of the values in the collection.</typeparam>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="name">Name of the argument (string) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given collection is null.</exception>
        /// <exception cref="ArgumentException"> if the given collection <paramref name="collection"/> contains elements that are null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CollectionMayNotContainNulls<TValue>(IEnumerable<TValue> collection, string name)
            where TValue : class
        {
#if DEBUG
            if (collection == null)
                throw new ArgumentNullException(name);
            if (collection.Any(elem => elem == null))
                throw new AggregateException(String.Format("Argument {0}. Collection may not contain null values.", name));
#endif
        }

        /// <summary>
        /// Validate that the given collection does not contain null or empty elements.
        /// </summary>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="name">Name of the argument (string) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given collection is null.</exception>
        /// <exception cref="ArgumentException"> if the given collection <paramref name="collection"/> contains elements that evaluate true to <see cref="string.IsNullOrEmpty(string)"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CollectionMayNotContainNullOrEmpty(IEnumerable<string> collection, string name)
        {
#if DEBUG
            if (collection == null)
                throw new ArgumentNullException(name);
            if (collection.Any(elem => String.IsNullOrEmpty(elem)))
                throw new AggregateException(String.Format("Argument {0}. Collection may not contain null or empty values.", name));
#endif
        }

        /// <summary>
        /// Validate that the given collection does not contain null, empty or whitespace elements.
        /// </summary>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="name">Name of the argument (string) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given collection is null.</exception>
        /// <exception cref="ArgumentException"> if the given collection <paramref name="collection"/> contains elements that evaluate true to <see cref="string.IsNullOrWhiteSpace(string)"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CollectionMayNotContainNullOrWhiteSpaces(IEnumerable<string> collection, string name)
        {
#if DEBUG
            if (collection == null)
                throw new ArgumentNullException(name);
            if (collection.Any(elem => String.IsNullOrWhiteSpace(elem)))
                throw new AggregateException(String.Format("Argument {0}. Collection may not contain null, empty or whitespace values.", name));
#endif
        }

        /// <summary>
        /// Validate that the given collection contain any values (is not empty).
        /// </summary>
        /// <typeparam name="TValue">The type of the values in the collection.</typeparam>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="name">Name of the argument (string) being validated.</param>
        /// <exception cref="ArgumentNullException"> is thrown if the given collection is null.</exception>
        /// <exception cref="ArgumentException"> if the given collection <paramref name="collection"/> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CollectionMustContainValues<TValue>(IEnumerable<TValue> collection, string name)
        {
#if DEBUG
            if (collection == null)
                throw new ArgumentNullException(name);
            if (!collection.Any())
                throw new AggregateException(String.Format("Argument {0}. Collection may not contain null values.", name));
#endif
        }

        /// <summary>
        /// This method can be used to satisfy Code Analysis (FxCop) warning that a method parameter is not used.
        /// </summary>
        /// <remarks>
        /// Sometimes, a method may be designed in such a way that a parameter is included in the argument list,
        /// but that parameter is currently not used. The idea is that the caller must supply the parameter,
        /// but only future versions of the method will start using the parameter.
        /// <para/>
        /// In those cases, Code Analysis (FxCop) will warn about the unused parameter. To avoid those warnings,
        /// call this method to trick Code Analysis (FxCop) to think that the parameter is used.
        /// </remarks>
        /// <example>
        /// <code>
        /// private void TestMethod(bool unused)
        /// {
        ///     // This will satisfy FxCop and think that the parameter is used.
        ///     Contract.ParameterNotUsed(unused);
        /// }
        /// </code>
        /// </example>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="value">This value is not used.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "This method is used to suppress exactly this message")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ParameterNotUsed<TValue>(TValue value)
        {
            // Nothing here!
        }
    }
}
