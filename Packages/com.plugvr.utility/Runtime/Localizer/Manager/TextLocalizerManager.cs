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
    public class TextLocalizerManager : MonoBehaviour
    {
        public static TextLocalizerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TextLocalizerManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("TextLocalizer");
                        instance = go.AddComponent<TextLocalizerManager>();
                    }
                }

                return instance;
            }
        }

        private static TextLocalizerManager instance;
        
#if UNITY_EDITOR

        public static string ResourcePath = "GameData/LanguageData";

#endif
        
        public event Action LocalizeChangedEvent;
        public event Action LocalizeSettingChangedEvent;
        
        [field: SerializeField]
        public LanguageData LanguageData { get; private set; }
        
        private void Awake()
        {
            ChangeLanguageIndex(GetCurrentLanguage());
        }
        
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

        public static Language GetDefaultLanguage()
        {
            return Language.English;
        }
        
        public static TMP_FontAsset GetFontAsset(FontState fontState)
        {
            var data = Instance.LanguageData;
            return data.GetInfoFonts(GetCurrentLanguage(), fontState).fontAsset;
        }
        
        public static TMP_FontAsset GetFontAsset(Language language, FontState fontState)
        {
            var data = Instance.LanguageData;
            return data.GetInfoFonts(language, fontState).fontAsset;
        }
        
        public static string Localize(string key)
        {
            var data = Instance.LanguageData;
            return data.GetInfoLanguage(data.currentLanguage, key);
        }      
        
        public void ChangeLanguageIndex(Language language)
        {
            LanguageData.currentLanguage = LanguageData.keyList[(int)language];

            LocalizeChangedEvent?.Invoke();
            LocalizeSettingChangedEvent?.Invoke();
        }
    }
}
