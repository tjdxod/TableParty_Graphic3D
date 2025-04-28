#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 패키지 임포트 시 자동 레이어 추가
    /// </summary>
    [InitializeOnLoad]
    public class PXRAutoLayer
    {
        #region Private Fields

        private const int MaxLayers = 31;

        private static readonly bool IsInitialize;

        #endregion

        #region Private Methods

        /// <summary>
        /// 레이어를 유무를 판단하여 가능한 경우 생성
        /// </summary>
        /// <returns>생성 가능한 경우 생성하고 true, 그렇지 않은 경우 false</returns>
        /// <param name="layerName">레이어 이름</param>
        private static bool CreateLayer(string layerName)
        {
            // Open tag manager
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            
            // Layers Property
            var layersProp = tagManager.FindProperty("layers");

            if (PropertyExists(layersProp, 0, MaxLayers, layerName)) 
                return false;

            // Start at layer 9th index -> 8 (zero based) => first 8 reserved for unity / greyed out
            for (int i = 8, j = MaxLayers; i < j; i++)
            {
                var sp = layersProp.GetArrayElementAtIndex(i);
                if (sp.stringValue != "") 
                    continue;
                
                // Assign string value to layer
                sp.stringValue = layerName;
                Debug.Log("Layer: " + layerName + " has been added");
                // Save settings
                tagManager.ApplyModifiedProperties();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 레이어를 유무를 판단하여 가능한 경우 생성
        /// </summary>
        /// <returns>생성 가능한 경우 생성하고 true, 그렇지 않은 경우 false</returns>
        /// <param name="layerIndex">레이어 인덱스</param>
        /// <param name="layerName">레이어 이름</param>
        private static bool CreateLayer(int layerIndex, string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");
            if (PropertyExists(layersProp, 0, MaxLayers, layerName))
                return false;

            var sp = layersProp.GetArrayElementAtIndex(layerIndex);
            if (sp.stringValue != "")
                return false;

            sp.stringValue = layerName;
            Debug.Log("Layer: " + layerName + " has been added");
            tagManager.ApplyModifiedProperties();
            return true;
        }

        /// <summary>
        /// 새로운 레이어를 생성
        /// </summary>
        /// <param name="name">생성할 레이어의 이름</param>
        /// <returns>생성된 레이어의 이름</returns>
        private static string NewLayer(string name)
        {
            CreateLayer(name);

            return name;
        }

        /// <summary>
        /// 새로운 레이어를 생성
        /// </summary>
        /// <param name="index">생성할 레이어의 인덱스</param>
        /// <param name="name">생성할 레이어의 이름</param>
        /// <returns>생성된 레이어의 이름</returns>
        private static string NewLayer(int index, string name)
        {
            if (index is >= 0 and <= 31)
            {
                CreateLayer(index, name);
            }

            return name;
        }

        /// <summary>
        /// 레이어 제거
        /// </summary>
        /// <param name="layerName">제거할 레이어 이름</param>
        /// <returns>정상적으로 제거된 경우 true, 그렇지 않은 경우 false</returns>
        private static bool RemoveLayer(string layerName)
        {
            // Open tag manager
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Tags Property
            var layersProp = tagManager.FindProperty("layers");

            if (!PropertyExists(layersProp, 0, layersProp.arraySize, layerName)) 
                return false;

            for (int i = 0, j = layersProp.arraySize; i < j; i++)
            {
                var sp = layersProp.GetArrayElementAtIndex(i);

                if (sp.stringValue != layerName) 
                    continue;
                    
                sp.stringValue = "";
                Debug.Log("Layer: " + layerName + " has been removed");
                // Save settings
                tagManager.ApplyModifiedProperties();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 레이어가 존재하는지 확인
        /// </summary>
        /// <param name="layerName">확인할 레이어 이름</param>
        /// <returns>존재하는 경우 true, 그렇지 않은 경우 false</returns>
        private static bool LayerExists(string layerName)
        {
            // Open tag manager
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Layers Property
            var layersProp = tagManager.FindProperty("layers");
            return PropertyExists(layersProp, 0, MaxLayers, layerName);
        }

        /// <summary>
        /// 속성이 있는지 확인
        /// </summary>
        /// <returns>속성이 존재하면 true, 그렇지 않은경우 false</returns>
        /// <param name="property">속성</param>
        /// <param name="start">시작 인덱스</param>
        /// <param name="end">종료 인덱스</param>
        /// <param name="value">이름</param>
        private static bool PropertyExists(SerializedProperty property, int start, int end, string value)
        {
            for (var i = start; i < end; i++)
            {
                var p = property.GetArrayElementAtIndex(i);
                if (p.stringValue.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        /// 생성자
        /// </summary>
        static PXRAutoLayer()
        {
            if (IsInitialize)
                return;

            IsInitialize = true;

            var pxrLayers = PXRLayer.Layers;

            foreach (var layer in pxrLayers)
            {
                NewLayer(layer.Key, layer.Value);
            }
        }
    }
}
#endif