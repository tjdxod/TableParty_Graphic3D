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

namespace Dive.Utility.Localizer
{
    public partial class Localizer
    {
#if !ODIN_INSPECTOR
        [HorizontalLine(10f, color: EColor.White)]
#endif
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(localizerType), Type.Text), SerializeField]
        private TMP_Text textComponent;

        [FoldoutGroup("Text Localizer"), ShowIf(nameof(localizerType), Type.Text), SerializeField]
        private string currentTextKey;

        [FoldoutGroup("Text Localizer"), ShowIf(nameof(localizerType), Type.Text), SerializeField]
        private Language modifiedTextLanguage;

        [FoldoutGroup("Text Localizer"), ShowIf(nameof(localizerType), Type.Text), SerializeField]
        private FontState fontState = FontState.Normal;

        [FoldoutGroup("Text Localizer"), ShowIf(nameof(localizerType), Type.Text), SerializeField]
        private bool useOutline = false;

        public string CurrentTextKey
        {
            get { return currentTextKey; }
            set
            {
                currentTextKey = value;
                OnLocalizeChanged();
            }
        }

        public TMP_Text TextComponent
        {
            get { return textComponent; }
        }

        private bool HasText()
        {
            return (localizerType & Type.Text) == Type.Text;
        }

        #region 영어

        private FontStyles internalEnglishFontStyle;
        private TextAlignmentOptions internalEnglishAlignment;

        private bool HasTextAndEnglish => HasText() && HasEnglish();

        private bool HasEnglish()
        {
            return modifiedTextLanguage == Language.English;
        }

        [Space(15)]
#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        private float englishSize;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetEnglishFontStyle))]
        public FontStyle engFontStyle;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetEnglishFontCase))]
        public FontCase engFontCase;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        public float engCharacter;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        public float engWord;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        public float engLine;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        public float engParagraph;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetEnglishAlignmentHorizontal))]
        public AlignmentHorizontal engAlignmentHorizontal;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndEnglish)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasEnglish)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetEnglishAlignmentVertical))]
        public AlignmentVertical engAlignmentVertical;

        private void SetEnglishFontStyle()
        {
            if (engFontStyle == 0)
            {
                internalEnglishFontStyle &= ~(FontStyles)(int)FontStyle.B;
                internalEnglishFontStyle &= ~(FontStyles)(int)FontStyle.I;
                internalEnglishFontStyle &= ~(FontStyles)(int)FontStyle.U;
                internalEnglishFontStyle &= ~(FontStyles)(int)FontStyle.S;
            }
            else
            {
                internalEnglishFontStyle |= (FontStyles)(int)engFontStyle;
            }
        }

        private void SetEnglishFontCase()
        {
            internalEnglishFontStyle &= ~(FontStyles)(int)FontCase.Lowercase;
            internalEnglishFontStyle &= ~(FontStyles)(int)FontCase.Uppercase;
            internalEnglishFontStyle &= ~(FontStyles)(int)FontCase.Smallcaps;

            if (engFontCase != 0)
                internalEnglishFontStyle |= (FontStyles)(int)engFontCase;
        }

        private void SetEnglishAlignmentHorizontal()
        {
            internalEnglishAlignment = (TextAlignmentOptions)((int)engAlignmentHorizontal + (int)engAlignmentVertical);
        }

        private void SetEnglishAlignmentVertical()
        {
            internalEnglishAlignment = (TextAlignmentOptions)((int)engAlignmentHorizontal + (int)engAlignmentVertical);
        }

        #endregion

        #region 한국어

        private FontStyles internalKoreanFontStyle;
        private TextAlignmentOptions internalKoreanAlignment;

        private bool HasTextAndKorean => HasText() && HasKorean();

        private bool HasKorean()
        {
            return modifiedTextLanguage == Language.Korean;
        }

        [Space(15)]
#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        private float koreanSize;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetKoreanFontStyle))]
        public FontStyle korFontStyle;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetKoreanFontCase))]
        public FontCase korFontCase;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        public float korCharacter;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        public float korWord;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        public float korLine;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        public float korParagraph;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetKoreanAlignmentHorizontal))]
        public AlignmentHorizontal korAlignmentHorizontal;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndKorean)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasKorean)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetKoreanAlignmentVertical))]
        public AlignmentVertical korAlignmentVertical;

        private void SetKoreanFontStyle()
        {
            if (korFontStyle == 0)
            {
                internalKoreanFontStyle &= ~(FontStyles)(int)FontStyle.B;
                internalKoreanFontStyle &= ~(FontStyles)(int)FontStyle.I;
                internalKoreanFontStyle &= ~(FontStyles)(int)FontStyle.U;
                internalKoreanFontStyle &= ~(FontStyles)(int)FontStyle.S;
            }
            else
            {
                internalKoreanFontStyle |= (FontStyles)(int)korFontStyle;
            }
        }

        private void SetKoreanFontCase()
        {
            internalKoreanFontStyle &= ~(FontStyles)(int)FontCase.Lowercase;
            internalKoreanFontStyle &= ~(FontStyles)(int)FontCase.Uppercase;
            internalKoreanFontStyle &= ~(FontStyles)(int)FontCase.Smallcaps;

            if (korFontCase != 0)
                internalKoreanFontStyle |= (FontStyles)(int)korFontCase;
        }

        private void SetKoreanAlignmentHorizontal()
        {
            internalKoreanAlignment = (TextAlignmentOptions)((int)korAlignmentHorizontal + (int)korAlignmentVertical);
        }

        private void SetKoreanAlignmentVertical()
        {
            internalKoreanAlignment = (TextAlignmentOptions)((int)korAlignmentHorizontal + (int)korAlignmentVertical);
        }

        #endregion

        #region 중국어

        private FontStyles internalChineseFontStyle;
        private TextAlignmentOptions internalChineseAlignment;

        private bool HasTextAndChinese => HasText() && HasChinese();

        private bool HasChinese()
        {
            return modifiedTextLanguage == Language.Chinese;
        }

        [Space(15)]
#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        private float chineseSize;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetChineseFontStyle))]
        public FontStyle chFontStyle;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetChineseFontCase))]
        public FontCase chFontCase;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        public float chCharacter;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        public float chWord;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        public float chLine;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        public float chParagraph;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        [OnValueChanged(nameof(SetChineseAlignmentHorizontal))]
        public AlignmentHorizontal chAlignmentHorizontal;

#if ODIN_INSPECTOR
        [FoldoutGroup("Text Localizer"), ShowIf(nameof(HasTextAndChinese)), SerializeField]
#else
        [FoldoutGroup("Text Localizer"), ShowIf(EConditionOperator.And, new[] {nameof(HasText), nameof(HasChinese)}), SerializeField]
#endif
        
        

        [OnValueChanged(nameof(SetChineseAlignmentVertical))]
        public AlignmentVertical chAlignmentVertical;

        private void SetChineseFontStyle()
        {
            if (engFontStyle == 0)
            {
                internalChineseFontStyle &= ~(FontStyles)(int)FontStyle.B;
                internalChineseFontStyle &= ~(FontStyles)(int)FontStyle.I;
                internalChineseFontStyle &= ~(FontStyles)(int)FontStyle.U;
                internalChineseFontStyle &= ~(FontStyles)(int)FontStyle.S;
            }
            else
            {
                internalChineseFontStyle |= (FontStyles)(int)chFontStyle;
            }
        }

        private void SetChineseFontCase()
        {
            internalChineseFontStyle &= ~(FontStyles)(int)FontCase.Lowercase;
            internalChineseFontStyle &= ~(FontStyles)(int)FontCase.Uppercase;
            internalChineseFontStyle &= ~(FontStyles)(int)FontCase.Smallcaps;

            if (engFontCase != 0)
                internalChineseFontStyle |= (FontStyles)(int)chFontCase;
        }

        private void SetChineseAlignmentHorizontal()
        {
            internalChineseAlignment = (TextAlignmentOptions)((int)chAlignmentHorizontal + (int)chAlignmentVertical);
        }

        private void SetChineseAlignmentVertical()
        {
            internalChineseAlignment = (TextAlignmentOptions)((int)chAlignmentHorizontal + (int)chAlignmentVertical);
        }

        #endregion

        private void TextChanged()
        {
            if (currentTextKey.Equals(string.Empty))
                return;

            var language = TextLocalizerManager.GetCurrentLanguage();

            ChangeFontState(fontState);
            ChangeFontStyle(language);
            ChangeFontAlignment(language);
            ChangeFontSize(language);
            ChangeSpacingOption(language);
            // TODO 언어 확정되면 변경

            textComponent.text = LocalizeText(currentTextKey);
        }

        private string LocalizeText(string key)
        {
            LanguageData data = TextLocalizerManager.Instance.LanguageData;
            if (!data.GetInfo(data.currentLanguage).IsTryGetValue(key, out var value))
            {
                Debug.Log($"{key} - ##KeyNotFound##");
                return "##KeyNotFound##";
            }

            if (value == string.Empty)
            {
                value = data.GetInfoLanguage(TextLocalizerManager.GetDefaultLanguage(), key);
            }

            return value;
        }

        private void ChangeFontStyle(Language language)
        {
            textComponent.fontStyle = language switch
            {
                Language.English => internalEnglishFontStyle,
                Language.Korean => internalKoreanFontStyle,
                Language.Chinese => internalChineseFontStyle,
                _ => textComponent.fontStyle
            };
        }

        private void ChangeFontSize(Language language)
        {
            textComponent.fontSize = language switch
            {
                Language.English => englishSize,
                Language.Korean => koreanSize,
                Language.Chinese => chineseSize,
                _ => textComponent.fontSize
            };
        }

        private void ChangeFontState(FontState fontState, Language editorLanguage = Language.English,
            bool isEditor = false)
        {
            LanguageData data = null;
            Language language = Language.English;

            if (isEditor)
            {
#if UNITY_EDITOR

                data = Resources.Load<LanguageData>(TextLocalizerManager.ResourcePath);

                if (data == null)
                {
                    var tmpData = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject LanguageData")
                        .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                        .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<LanguageData>)
                        .FirstOrDefault();

                    if (tmpData == null)
                    {
                        Debug.LogError("LanguageData가 존재하지 않습니다.");
                        return;
                    }

                    data = tmpData;
                }

                language = editorLanguage;

#endif
            }
            else
            {
                data = TextLocalizerManager.Instance.LanguageData;
                language = TextLocalizerManager.GetCurrentLanguage();
            }

            var fontKey = data.GetInfo(language).languageTextFontKeys;
            var fontValue = data.GetInfo(language).languageTextFontValues;

            if (fontKey.Count == 0 || fontValue.Count == 0)
                return;

            var index = fontKey.IndexOf(fontState);
            var textFont = fontValue[index];


            textComponent.font = textFont.fontAsset;
            textComponent.fontMaterial = useOutline ? textFont.outlineMaterial : textFont.normalMaterial;
        }

        private void ChangeFontAlignment(Language language)
        {
            switch (language)
            {
                case Language.English:
                    internalEnglishAlignment =
                        (TextAlignmentOptions)((int)engAlignmentHorizontal + (int)engAlignmentVertical);
                    textComponent.alignment = internalEnglishAlignment;
                    break;
                case Language.Korean:
                    internalKoreanAlignment =
                        (TextAlignmentOptions)((int)korAlignmentHorizontal + (int)korAlignmentVertical);
                    textComponent.alignment = internalKoreanAlignment;
                    break;
                case Language.Chinese:
                    internalChineseAlignment =
                        (TextAlignmentOptions)((int)chAlignmentHorizontal + (int)chAlignmentVertical);
                    textComponent.alignment = internalChineseAlignment;
                    break;
            }
        }

        private void ChangeSpacingOption(Language language)
        {
            if (language == Language.English)
            {
                textComponent.characterSpacing = engCharacter;
                textComponent.wordSpacing = engWord;
                textComponent.lineSpacing = engLine;
                textComponent.paragraphSpacing = engParagraph;
            }
            else if (language == Language.Korean)
            {
                textComponent.characterSpacing = korCharacter;
                textComponent.wordSpacing = korWord;
                textComponent.lineSpacing = korLine;
                textComponent.paragraphSpacing = korParagraph;
            }
            else if (language == Language.Chinese)
            {
                textComponent.characterSpacing = chCharacter;
                textComponent.wordSpacing = chWord;
                textComponent.lineSpacing = chLine;
                textComponent.paragraphSpacing = chParagraph;
            }
        }

        public void ChangeFontSize(int size)
        {
            textComponent.fontSize = size;
        }

        // 튜토리얼에서 사용....
        // 1. 텍스트 키만 변경
        // 바로 출력되지 않길 원할 때 사용
        public string ChangeTextKey(string newKey)
        {
            CurrentTextKey = newKey;
            return LocalizeText(currentTextKey);
        }
    }
}