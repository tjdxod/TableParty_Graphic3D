#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dive.Utility
{
    public class StaticStorage
    {
        private static readonly List<IStaticVar> StaticVarList = new();

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            foreach (var staticVar in StaticVarList)
            {
                staticVar.Reset();
            }

            StaticVarList.Clear();
        }

        public static void Register<T>(StaticVar<T> staticVar)
        {
            StaticVarList.Add(staticVar);
        }
    }
}
#endif