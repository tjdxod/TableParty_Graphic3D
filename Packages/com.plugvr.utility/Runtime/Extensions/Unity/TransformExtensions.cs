using System;
using UnityEngine;

namespace Dive.Utility.UnityExtensions
{
    /// <summary>
    /// UnityEngine.Transform의 확장 메서드
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// 게임 오브젝트들을 자식으로 추가
        /// </summary>
        /// <param name="transform">부모 Transform</param>
        /// <param name="children">게임오브젝트 배열</param>
        public static void AddChildren(this Transform transform, GameObject[] children)
        {
            Array.ForEach(children, child => child.transform.parent = transform);
        }

        /// <summary>
        /// 컴포넌트들을 자식으로 추가
        /// </summary>
        /// <param name="transform">부모 Transform</param>
        /// <param name="children">컴포넌트 배열</param>
        public static void AddChildren(this Transform transform, Component[] children)
        {
            Array.ForEach(children, child => child.transform.parent = transform);
        }

        /// <summary>
        /// 모든 자식 오브젝트의 Position 초기화
        /// </summary>
        /// <param name="transform">부모 Transform</param>
        /// <param name="recursive">자식의 자식들 적용여부</param>
        public static void ResetChildPositions(this Transform transform, bool recursive = false)
        {
            foreach (Transform child in transform)
            {
                child.position = Vector3.zero;

                if (recursive)
                {
                    child.ResetChildPositions(recursive);
                }
            }
        }

        /// <summary>
        /// 모든 자식 오브젝트의 레이어를 변경
        /// </summary>
        /// <param name="transform">부모 Transform</param>
        /// <param name="layerName">변경할 레이어의 이름</param>
        /// <param name="recursive">자식의 자식들 적용여부</param>
        public static void SetChildLayers(this Transform transform, string layerName, bool recursive = false)
        {
            var layer = LayerMask.NameToLayer(layerName);
            SetChildLayersHelper(transform, layer, recursive);
        }

        private static void SetChildLayersHelper(Transform transform, int layer, bool recursive)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = layer;

                if (recursive)
                {
                    SetChildLayersHelper(child, layer, recursive);
                }
            }
        }

        /// <summary>
        /// X 좌표 값만 변경
        /// </summary>
        /// <param name="x">변경할 X 값</param>
        public static void SetX( this Transform transform, float x )
        {
            transform.position = new Vector3( x, transform.position.y, transform.position.z );
        }

        /// <summary>
        /// Y 좌표 값만 변경
        /// </summary>
        /// <param name="y">변경할 Y 값</param>
        public static void SetY( this Transform transform, float y )
        {
            transform.position = new Vector3( transform.position.x, y, transform.position.z );
        }

        /// <summary>
        /// Z 좌표 값만 변경
        /// </summary>
        /// <param name="z">변경할 Z 값</param>
        public static void SetZ( this Transform transform, float z )
        {
            transform.position = new Vector3( transform.position.x, transform.position.y, z );
        }
    }
}
