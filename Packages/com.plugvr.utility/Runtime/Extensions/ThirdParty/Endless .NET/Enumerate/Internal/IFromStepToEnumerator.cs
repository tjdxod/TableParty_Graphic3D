using System;
using System.Collections.Generic;

namespace Dive.Utility.CsharpExtensions
{
    internal interface IFromStepToEnumerator<T> : IEnumerator<T>
    {
        IFromStepToEnumerator<T> Clone();
        IFromStepToEnumerator<T> CloneWithThenRestriction(T then);
        IFromStepToEnumerator<T> CloneWithStepRestriction(Func<T, T> step);
        IFromStepToEnumerator<T> CloneWithToRestriction(T to);
    }
}