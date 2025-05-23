using System;
using System.Collections.Generic;
using System.Linq;

namespace Dive.Utility.CsharpExtensions
{
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Sorts the elements of the sequence in ascending order
        /// </summary>
        public static IEnumerable<T> Sort<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.OrderBy(Function<T>.Identity);
        }

        /// <summary>
        /// Sorts the elements of the sequence in ascending order
        /// </summary>
        public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.OrderBy(Function<T>.Identity, comparer);
        }

        /// <summary>
        /// Sorts the elements of the sequence in descending order
        /// </summary>
        public static IEnumerable<T> SortDescending<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.OrderByDescending(Function<T>.Identity);
        }

        /// <summary>
        /// Sorts the elements of the sequence in descending order
        /// </summary>
        public static IEnumerable<T> SortDescending<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.OrderByDescending(Function<T>.Identity, comparer);
        }
    }
}