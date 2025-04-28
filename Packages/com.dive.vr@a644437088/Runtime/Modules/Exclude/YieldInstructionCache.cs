using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 코루틴 Yield를 Cache에 저장하여 사용 
    /// </summary>
    public static class YieldInstructionCache
    {
        private class FloatComparer : IEqualityComparer<float>
        {
            bool IEqualityComparer<float>.Equals(float x, float y)
            {
                return Math.Abs(x - y) < 0.001f;
            }

            int IEqualityComparer<float>.GetHashCode(float obj)
            {
                return obj.GetHashCode();
            }
        }

        #region Public Fields

        /// <summary>
        /// WaitForEndOfFrame 실행
        /// </summary>
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// WaitForFixedUpdate 실행
        /// </summary>
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

        #endregion

        #region Private Fields
        
        private static readonly Dictionary<float, WaitForSeconds> TimeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());

        #endregion

        #region Public Methods
        
        /// <summary>
        /// WaitForSeconds의 Cache가 있는 경우 Cache에서 실행, 그렇지 않은경우 실행 후 Cache에 저장
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            if (!TimeInterval.TryGetValue(seconds, out var wfs))
                TimeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
            return wfs;
        }
        
        #endregion
    }
}