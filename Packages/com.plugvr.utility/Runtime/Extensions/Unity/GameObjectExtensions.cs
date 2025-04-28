using UnityEngine;

namespace Dive.Utility.UnityExtensions
{
    /// <summary>
    /// UnityEngine.GameObject의 확장 메서드
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// GetComponent를 실행하고, 없으면 AddComponent를 실행하여 반환
        /// </summary>
        /// <param name="gameObject">게임 오브젝트</param>
        /// <returns>Component 혹은 새롭게 추가된 Component</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Component가 있는지 확인
        /// </summary>
        /// <param name="gameObject">게임 오브젝트</param>
        /// <returns>True인 경우 Component가 존재</returns>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }
    }
}
