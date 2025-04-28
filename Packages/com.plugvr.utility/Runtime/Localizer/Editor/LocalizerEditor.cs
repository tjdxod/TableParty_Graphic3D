#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#else

using NaughtyAttributes.Utility;

#endif

using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;


namespace Dive.Utility.Localizer
{
    public partial class Localizer
    {
        // 에디터 인스펙터에서 텍스트 혹은 이미지 및 텍스쳐 현지화를 적용 가능
        // 해당 값에 따라 런타임 언어에 따라 텍스트, 이미지 및 텍스쳐 변경 -> 텍스쳐가 2배
        //  

        private void Reset()
        {
            if (textComponent == null)
                textComponent = GetComponent<TMP_Text>();

            if (textComponent != null)
            {
                koreanSize = textComponent.fontSize;
                englishSize = textComponent.fontSize;
                chineseSize = textComponent.fontSize;

                var tmpStyle = textComponent.fontStyle;

                engFontStyle = 0;
                korFontStyle = 0;
                chFontStyle = 0;

                if (tmpStyle.HasFlag(FontStyles.Bold))
                {
                    engFontStyle |= FontStyle.B;
                    korFontStyle |= FontStyle.B;
                    chFontStyle |= FontStyle.B;
                }

                if (tmpStyle.HasFlag(FontStyles.Italic))
                {
                    engFontStyle |= FontStyle.I;
                    korFontStyle |= FontStyle.I;
                    chFontStyle |= FontStyle.I;
                }

                if (tmpStyle.HasFlag(FontStyles.Underline))
                {
                    engFontStyle |= FontStyle.U;
                    korFontStyle |= FontStyle.U;
                    chFontStyle |= FontStyle.U;
                }

                if (tmpStyle.HasFlag(FontStyles.Strikethrough))
                {
                    engFontStyle |= FontStyle.S;
                    korFontStyle |= FontStyle.S;
                    chFontStyle |= FontStyle.S;
                }

                if (tmpStyle.HasFlag(FontStyles.LowerCase))
                {
                    engFontCase = FontCase.Lowercase;
                    korFontCase = FontCase.Lowercase;
                    chFontCase = FontCase.Lowercase;
                }
                else if (tmpStyle.HasFlag(FontStyles.UpperCase))
                {
                    engFontCase = FontCase.Uppercase;
                    korFontCase = FontCase.Uppercase;
                    chFontCase = FontCase.Uppercase;
                }
                else if (tmpStyle.HasFlag(FontStyles.SmallCaps))
                {
                    engFontCase = FontCase.Smallcaps;
                    korFontCase = FontCase.Smallcaps;
                    chFontCase = FontCase.Smallcaps;
                }

                engAlignmentHorizontal = (AlignmentHorizontal)((int)textComponent.alignment & 0b111111);
                engAlignmentVertical = (AlignmentVertical)((int)textComponent.alignment & 0b1111111000000);
                korAlignmentHorizontal = (AlignmentHorizontal)((int)textComponent.alignment & 0b111111);
                korAlignmentVertical = (AlignmentVertical)((int)textComponent.alignment & 0b1111111000000);
                chAlignmentHorizontal = (AlignmentHorizontal)((int)textComponent.alignment & 0b111111);
                chAlignmentVertical = (AlignmentVertical)((int)textComponent.alignment & 0b1111111000000);

                internalEnglishFontStyle = textComponent.fontStyle;
                internalEnglishAlignment = textComponent.alignment;
                internalKoreanFontStyle = textComponent.fontStyle;
                internalKoreanAlignment = textComponent.alignment;
                internalChineseFontStyle = textComponent.fontStyle;
                internalChineseAlignment = textComponent.alignment;

                engCharacter = textComponent.characterSpacing;
                engWord = textComponent.wordSpacing;
                engLine = textComponent.lineSpacing;
                engParagraph = textComponent.paragraphSpacing;

                korCharacter = textComponent.characterSpacing;
                korWord = textComponent.wordSpacing;
                korLine = textComponent.lineSpacing;
                korParagraph = textComponent.paragraphSpacing;

                chCharacter = textComponent.characterSpacing;
                chWord = textComponent.wordSpacing;
                chLine = textComponent.lineSpacing;
                chParagraph = textComponent.paragraphSpacing;
            }

            if (imageComponent == null)
                imageComponent = GetComponent<UnityEngine.UI.Image>();

            if (spriteRendererComponent == null)
                spriteRendererComponent = GetComponent<SpriteRenderer>();

            if (rendererComponent == null)
                rendererComponent = GetComponent<Renderer>();
        }

        private void OnChangeLocalizerType()
        {
            switch (localizerType)
            {
                case Type.Text:
                    if (textComponent == null)
                        textComponent = GetComponent<TMP_Text>();
                    break;
                case Type.Image:
                    if (imageComponent == null)
                        imageComponent = GetComponent<UnityEngine.UI.Image>();

                    if (spriteRendererComponent == null)
                        spriteRendererComponent = GetComponent<SpriteRenderer>();
                    break;
                case Type.Texture:
                    if (rendererComponent == null)
                        rendererComponent = GetComponent<Renderer>();
                    break;
                default:
                    return;
            }
        }

        #region Text

        #if ODIN_INSPECTOR
        [Button("Apply English"), ShowIf(nameof(HasTextAndEnglish))]
        #else
        [Button("Apply English"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)})]
        #endif
        private void ApplyTextEnglish()
        {
            var data = Resources.Load<LanguageData>(TextLocalizerManager.ResourcePath);

            if (data == null)
            {
                var tmpData = AssetDatabase.FindAssets("t:ScriptableObject LanguageData")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<LanguageData>)
                    .FirstOrDefault();

                if (tmpData == null)
                {
                    Debug.LogError("LanguageData가 존재하지 않습니다.");
                    return;
                }

                data = tmpData;
            }

            if (!data.GetInfo(Language.English).IsExistKey(currentTextKey))
            {
                Debug.LogError($"ObjectName: {textComponent.gameObject.name} - 키가 입력되지 않았습니다.");
                return;
            }

            ChangeFontState(fontState, Language.English, true);
            ChangeFontStyle(Language.English);
            ChangeFontAlignment(Language.English);
            ChangeFontSize(Language.English);
            ChangeSpacingOption(Language.English);

            textComponent.text = data.GetInfoLanguage(Language.English, currentTextKey);
        }

        #if ODIN_INSPECTOR
        [Button("Apply Korean"), ShowIf(nameof(HasTextAndKorean))]
        #else
        [Button("Apply Korean"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)})]
        #endif
        private void ApplyTextKorean()
        {
            var data = Resources.Load<LanguageData>(TextLocalizerManager.ResourcePath);

            if (data == null)
            {
                var tmpData = AssetDatabase.FindAssets("t:ScriptableObject LanguageData")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<LanguageData>)
                    .FirstOrDefault();

                if (tmpData == null)
                {
                    Debug.LogError("LanguageData가 존재하지 않습니다.");
                    return;
                }

                data = tmpData;
            }

            if (!data.GetInfo(Language.Korean).IsExistKey(currentTextKey))
            {
                Debug.LogError($"ObjectName: {textComponent.gameObject.name} - 키가 입력되지 않았습니다.");
                return;
            }

            ChangeFontState(fontState, Language.Korean, true);
            ChangeFontStyle(Language.Korean);
            ChangeFontAlignment(Language.Korean);
            ChangeFontSize(Language.Korean);
            ChangeSpacingOption(Language.Korean);

            textComponent.text = data.GetInfoLanguage(Language.Korean, currentTextKey);
        }

        #if ODIN_INSPECTOR
        [Button("Apply Chinese"), ShowIf(nameof(HasTextAndChinese))]
        #else
        [Button("Apply Chinese"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)})]
        #endif
        private void ApplyTextChinese()
        {
            var data = Resources.Load<LanguageData>(TextLocalizerManager.ResourcePath);

            if (data == null)
            {
                var tmpData = AssetDatabase.FindAssets("t:ScriptableObject LanguageData")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<LanguageData>)
                    .FirstOrDefault();

                if (tmpData == null)
                {
                    Debug.LogError("LanguageData가 존재하지 않습니다.");
                    return;
                }

                data = tmpData;
            }

            if (!data.GetInfo(Language.Chinese).IsExistKey(currentTextKey))
            {
                Debug.LogError($"ObjectName: {textComponent.gameObject.name} - 키가 입력되지 않았습니다.");
                return;
            }

            ChangeFontState(fontState, Language.Chinese, true);
            ChangeFontStyle(Language.Chinese);
            ChangeFontAlignment(Language.Chinese);
            ChangeFontSize(Language.Chinese);
            ChangeSpacingOption(Language.Chinese);

            textComponent.text = data.GetInfoLanguage(Language.Chinese, currentTextKey);
        }

        #endregion

#region Image

#endregion


#region Texture

#endregion
    }
}
#endif