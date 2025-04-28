using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.Utility.Localizer
{
    public class ImageLocalizerManager : MonoBehaviour
    {
        public static ImageLocalizerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ImageLocalizerManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("ImageLocalizer");
                        instance = go.AddComponent<ImageLocalizerManager>();
                    }
                }

                return instance;
            }
        }

        private static ImageLocalizerManager instance;
        
        [field: SerializeField]
        public LanguageData LanguageData { get; private set; }
        
        public static Language GetCurrentLanguage()
        {
            var language = Language.None;
            var current = Instance.LanguageData.currentLanguage;

            if (instance.LanguageData.keyList.Contains(current))
            {
                language = (Language) Instance.LanguageData.keyList.IndexOf(current);
            }

            return language;
        }
    }
}
