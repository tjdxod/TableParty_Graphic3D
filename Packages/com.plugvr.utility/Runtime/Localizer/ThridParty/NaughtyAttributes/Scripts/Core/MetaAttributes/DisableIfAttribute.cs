﻿#if !ODIN_INSPECTOR

using System;

namespace NaughtyAttributes.Utility
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DisableIfAttribute : EnableIfAttributeBase
    {
        public DisableIfAttribute(string condition)
            : base(condition)
        {
            Inverted = true;
        }

        public DisableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions)
        {
            Inverted = true;
        }

        public DisableIfAttribute(string enumName, object enumValue)
            : base(enumName, enumValue as Enum)
        {
            Inverted = true;
        }
    }
}

#endif