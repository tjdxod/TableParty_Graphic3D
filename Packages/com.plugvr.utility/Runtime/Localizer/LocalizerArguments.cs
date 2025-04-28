using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Dive.Utility.Localizer
{
    [Serializable]
    public class LanguageTextInfo
    {
        public Language language;

        public List<FontState> languageTextFontKeys = new List<FontState>();
        public List<LanguageTextFont> languageTextFontValues = new List<LanguageTextFont>();

        public List<string> languageContextKeys = new List<string>();
        public List<string> languageContextValue = new List<string>();

        public string GetContext(string key)
        {
            var index = languageContextKeys.IndexOf(key);
            return index == -1 ? string.Empty : languageContextValue[index];
        }
        
        public LanguageTextFont GetTextFont(FontState fontState)
        {
            var index = languageTextFontKeys.IndexOf(fontState);
            return index == -1 ? default : languageTextFontValues[index];
        }
        
        public bool IsExistFont(FontState fontState)
        {
            var isExist = languageTextFontKeys.Contains(fontState);
            return isExist;
        }
        
        public bool IsExistKey(string key)
        {
            var isExist = languageContextKeys.Contains(key);
            return isExist;
        }
        
        public bool IsTryGetValue(string key, out string value)
        {
            var index = languageContextKeys.IndexOf(key);
            if (index == -1)
            {
                value = string.Empty;
                return false;
            }
            else
            {
                value = languageContextValue[index];
                return true;
            }
        }
    }        
    
    [Serializable]
    public struct LanguageTextFont
    {
        public TMP_FontAsset fontAsset;
        public Material normalMaterial;
        public Material outlineMaterial;
    }
    
    public enum Language
    {
        None,
        English, // 지원
        Korean, // 지원 
        Chinese, // 지원
        Japanese, // 현재 미지원
        French, // 현재 미지원
        German, // 현재 미지원
        Spanish // 현재 미지원
    }

    [Flags]
    public enum Type
    {
        Text = 1,
        Image = 2,
        Texture = 4
    }

    [Flags]
    public enum FontState
    {
        Normal,
        Bold,
        UI
    }


    
    [Flags]
    public enum FontStyle
    {
        Off = 0,
        B = 1,
        I = 2,
        U = 4,
        S = 64,
    }

    public enum FontCase
    {
        Off = 0,
        Lowercase = 8,
        Uppercase = 16,
        Smallcaps = 32
    }

    public enum AlignmentHorizontal
    {
        Left = 1,
        Center = 2,
        Right = 4,
        Justified = 8,
        Flush = 16,
        Geometry = 32
    }

    public enum AlignmentVertical
    {
        Top = 256,
        Middle = 512,
        Bottom = 1024,
        Baseline = 2048,
        MidLine = 4096,
        Capline = 8192
    }
}