using System;
using UnityEngine;

namespace Dive.VRModule
{
    public static class PXRCommonExtensions
    {
        public static T Find<T>(this T[] array, Predicate<T> match)
        {
            return Array.Find(array, match);
        }
        
        // 해당 레이어 포함하고 있는지 확인
        public static bool Includes(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) > 0;
        }
    }
}
