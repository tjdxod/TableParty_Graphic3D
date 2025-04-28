using UnityEngine;

namespace Dive.Utility.UnityExtensions
{
    /// <summary>
    /// UnityEngine.Rigidbody의 확장 메서드
    /// </summary>
    public static class RigidbodyExtensions
    {
        /// <summary>
        /// 속도를 변경하지 않고 Rigidbody의 방향을 변경
        /// </summary>
        /// <param name="rigidbody">Rigidbody.</param>
        /// <param name="direction">변경할 direction.</param>
        public static void ChangeDirection(this Rigidbody rigidbody, Vector3 direction)
        {
            rigidbody.velocity = direction * rigidbody.velocity.magnitude;
        }
    }
}
