using System.Collections.Generic;
using UnityEngine;

namespace Dive.Utility.UnityExtensions
{
    /// <summary>
    /// UnityEngine.Vector3의 확장 메서드
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// 가장 근접한 좌표를 반환
        /// </summary>
        /// <param name="position">월드 좌표</param>
        /// <param name="otherPositions">비교할 월드 좌표들</param>
        /// <returns>가장 근접한 좌표</returns>
        public static Vector3 GetClosest(this Vector3 position, IEnumerable<Vector3> otherPositions)
        {
            var closest = Vector3.zero;
            var shortestDistance = Mathf.Infinity;

            foreach (var otherPosition in otherPositions)
            {
                var distance = (position - otherPosition).sqrMagnitude;

                if (distance < shortestDistance)
                {
                    closest = otherPosition;
                    shortestDistance = distance;
                }
            }

            return closest;
        }
    }
}
