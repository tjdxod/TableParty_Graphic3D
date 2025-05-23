using System.Collections.Generic;

namespace Dive.Utility.CsharpExtensions
{
    public interface IFromStepEnumerable<T> : IEnumerable<T>
    {
        /// <summary>
        /// Bounds the collection of numbers from the right.
        /// </summary>
        /// <returns>(n0, ..., m)</returns>
        IEnumerable<T> To(T toNumber);
    }
}