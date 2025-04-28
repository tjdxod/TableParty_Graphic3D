using UnityEngine;

namespace Dive.Utility.UnityExtensions
{
    /// <summary>
    /// UnityEngine.Component의 확장 메서드
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// GameObject가 아닌 Component에서 AddComponent 실행
        /// </summary>
        /// <param name="component">Component</param>
        /// <returns>새롭게 추가된 Component</returns>
        public static T AddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// GetComponent를 실행하고, 없으면 AddComponent를 실행하여 반환
        /// </summary>
        /// <param name="component">Component</param>
        /// <returns>Component 혹은 새롭게 추가된 Component</returns>
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.GetComponent<T>() ?? component.AddComponent<T>();
        }

        /// <summary>
        /// Component가 있는지 확인
        /// </summary>
        /// <param name="component">Component</param>
        /// <returns>True인 경우 Component가 존재</returns>
        public static bool HasComponent<T>(this Component component) where T : Component
        {
            return component.GetComponent<T>() != null;
        }
    }
}
