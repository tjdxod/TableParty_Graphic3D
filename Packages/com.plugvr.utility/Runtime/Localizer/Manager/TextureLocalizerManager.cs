using System;
using System.Collections;
using System.Collections.Generic;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#else

using NaughtyAttributes.Utility;

#endif

using TMPro;
using UnityEngine;

namespace Dive.Utility.Localizer
{
    public class TextureLocalizerManager : MonoBehaviour
    {
        public static TextureLocalizerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TextureLocalizerManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("TextureLocalizer");
                        instance = go.AddComponent<TextureLocalizerManager>();
                    }
                }

                return instance;
            }
        }

        private static TextureLocalizerManager instance;

    }
}