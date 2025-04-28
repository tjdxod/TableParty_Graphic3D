#if !ODIN_INSPECTOR

using System;

namespace NaughtyAttributes.Utility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MinValueAttribute : ValidatorAttribute
    {
        public float MinValue { get; private set; }

        public MinValueAttribute(float minValue)
        {
            MinValue = minValue;
        }

        public MinValueAttribute(int minValue)
        {
            MinValue = minValue;
        }
    }
}

#endif