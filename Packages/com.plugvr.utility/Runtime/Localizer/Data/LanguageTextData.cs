using System.Collections.Generic;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#else

using NaughtyAttributes.Utility;

#endif

namespace Dive.Utility.Localizer
{
    public partial class LanguageData
    {
        [FoldoutGroup("Text")]
        public Language currentLanguage;

        [FoldoutGroup("Text")]
        public List<Language> keyList = new List<Language>();
        
        [FoldoutGroup("Text")]
        public List<LanguageTextInfo> infoList = new List<LanguageTextInfo>();
        
        public LanguageTextInfo GetInfo(Language language)
        {
            var index = keyList.IndexOf(language);

            if (index == -1)
                return null;

            if (infoList.Count > index)
            {
                return infoList[index].language == language ? infoList[index] : null;
            }

            return null;
        }

        public string GetInfoLanguage(Language language, string key)
        {
            var index = keyList.IndexOf(language);
            
            if(index == -1)
                return string.Empty;

            if (infoList.Count > index)
            {
                return infoList[index].language == language ? infoList[index].GetContext(key) : string.Empty;
            }

            return string.Empty;
        }

        public LanguageTextFont GetInfoFonts(Language language, FontState fontState)
        {
            var index = keyList.IndexOf(language);

            if (index == -1)
                return default;

            if (infoList.Count > index)
            {
                return infoList[index].language == language ? infoList[index].GetTextFont(fontState) : default;
            }

            return default;
        }



    }
}