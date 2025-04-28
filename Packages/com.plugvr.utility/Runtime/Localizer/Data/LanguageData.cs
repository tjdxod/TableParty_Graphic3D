using System.Collections;
using System.Collections.Generic;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#else

using NaughtyAttributes.Utility;

#endif

using Dive.Utility.Localizer;
using UnityEngine;

namespace Dive.Utility.Localizer
{
    public partial class LanguageData : ScriptableObject
    {
#if UNITY_EDITOR

        [Button]
        private void Save()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

#endif
    }
}