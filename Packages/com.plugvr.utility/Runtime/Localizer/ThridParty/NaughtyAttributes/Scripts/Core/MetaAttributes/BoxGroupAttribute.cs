﻿#if !ODIN_INSPECTOR

using System;

namespace NaughtyAttributes.Utility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BoxGroupAttribute : MetaAttribute, IGroupAttribute
    {
        public string Name { get; private set; }

        public BoxGroupAttribute(string name = "")
        {
            Name = name;
        }
    }
}

#endif