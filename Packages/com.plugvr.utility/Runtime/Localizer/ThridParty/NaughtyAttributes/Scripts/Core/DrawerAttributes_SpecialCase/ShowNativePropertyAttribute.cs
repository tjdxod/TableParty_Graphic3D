#if !ODIN_INSPECTOR

using System;

namespace NaughtyAttributes.Utility
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ShowNativePropertyAttribute : SpecialCaseDrawerAttribute
    {
    }
}

#endif