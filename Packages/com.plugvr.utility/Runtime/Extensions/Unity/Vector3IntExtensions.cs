using System;
using UnityEngine;

namespace Dive.Utility.UnityExtensions
{
    /// <summary>
    /// UnityEngine.Vector3Int의 확장 메서드
    /// </summary>
    public static class Vector3IntExtensions
    {
        /// <summary>
        /// Vector3Int 구조체를 Vector3로 변환
        /// </summary>
        /// <param name="vector">변경할 Vector3Int</param>
        /// <returns>변환된 Vector3</returns>
        public static Vector3 ToVector3(this Vector3Int vector)
        {
            return new Vector3(
                Convert.ToSingle(vector.x),
                Convert.ToSingle(vector.y),
                Convert.ToSingle(vector.z)
            );
        }
    }
}
