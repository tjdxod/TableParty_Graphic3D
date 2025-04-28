#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#else

using NaughtyAttributes.Utility;

#endif

using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Dive.Utility.Localizer
{
    public class LanguageDataWindow : EditorWindow
    {
        private static float width = 400;
        private static float height = 300;        
        
        [SerializeField] 
        private string LanguageSpreadURL = "https://docs.google.com/spreadsheets/d/1zxRnkj1a6MevArMMIXcF9vUUlNFvXrVqKLElOcsZZFw/export?format=tsv&gid=470818722";
        
        [SerializeField]
        private string CreateDataPath = "Assets/LanguageData.asset";
        
        [SerializeField]
        private LanguageData languageData;

        [MenuItem("DiveXR/Utility/Language Data")]
        public static void ShowWindow()
        {
            var window = GetWindow<LanguageDataWindow>();

            window.titleContent = new GUIContent("Language Data");
            window.minSize = new Vector2(width, height);
            window.maxSize = new Vector2(width, height);
            window.Show();
        }

        private void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 15, width, 50), Texture2D.normalTexture);

            GUILayout.BeginArea(new Rect(60, 25, width - 10, 35));

            var headStyle = new GUIStyle();
            headStyle.alignment = TextAnchor.MiddleLeft;
            headStyle.fontSize = 25;
            headStyle.fontStyle = UnityEngine.FontStyle.Bold;
            headStyle.normal.textColor = Color.white;
            headStyle.hover.textColor = Color.white;
            headStyle.focused.textColor = Color.white;
            headStyle.active.textColor = Color.white;

            GUILayout.Label("Language Data", headStyle);
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(10, 80, width - 10, 60));

            var contentStyle = new GUIStyle();
            contentStyle.alignment = TextAnchor.MiddleLeft;
            contentStyle.fontSize = 14;
            contentStyle.stretchWidth = true;
            contentStyle.normal.textColor = Color.white;
            contentStyle.hover.textColor = Color.white;
            contentStyle.focused.textColor = Color.white;
            contentStyle.active.textColor = Color.white;

            GUILayout.Label("Create or Update Language Data", contentStyle);

            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(10, 120, width - 20, 40));

            languageData =  EditorGUILayout.ObjectField("Language Data", languageData, typeof(LanguageData), false) as LanguageData;

            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(10, 160, width - 20, 60));
            
            if (GUILayout.Button("Create Empty Language Data", new GUIStyle("LargeButtonMid")))
            {
                CreateEmptyData();
            }

            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(10, 190, width - 20, 60));
            
            if (GUILayout.Button("Update Language Data", new GUIStyle("LargeButtonMid")))
            {
                UpdateData();
            }

            GUILayout.EndArea();
        }

        private void CreateEmptyData()
        {
            var asset = CreateInstance<LanguageData>();
            
            AssetDatabase.CreateAsset(asset, CreateDataPath);
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();

            languageData = asset;
        }

        private void UpdateData()
        {
            if (!languageData)
                return;

            UnityWebRequest req = UnityWebRequest.Get(LanguageSpreadURL);
            UnityWebRequestAsyncOperation op = req.SendWebRequest();

            op.completed += (aop) =>
            {
                //Debug.Log("completed: " + req.downloadHandler.text);
                SetLanguageTextData(req.downloadHandler.text);

                req.Dispose();
            };
        }
        
        private void SetLanguageTextData(string tsv)
        {
            try
            {
                var row = tsv.Split('\n');

                var rowSize = row.Length;
                var columnSize = row[0].Split('\t').Length;

                var sentence = new string[rowSize, columnSize];

                for (var i = 0; i < rowSize; i++)
                {
                    var column = row[i].Split('\t');

                    if (column[0] == string.Empty || column[0].Contains("/"))
                        continue;

                    for (var j = 0; j < columnSize; j++)
                    {
                        sentence[i, j] = column[j];
                    }
                }

                if(languageData.infoList == null)
                    languageData.infoList = new List<LanguageTextInfo>();
                
                if(languageData.keyList == null)
                    languageData.keyList = new List<Language>();
                    
                languageData.infoList.Clear();
                languageData.keyList.Clear();
                
                var languages = new List<Language>();
                
                for (var i = 0; i < columnSize; i++)
                {
                    var tmp = sentence[0, i].Replace(" ", string.Empty).Split('_')[0];
                    
                    foreach (var language in Enum.GetValues(typeof(Language)))
                    {
                        if (!tmp.Contains(language.ToString())) 
                            continue;
                        
                        if(languages.Exists(l => l == (Language)language))
                            continue;
                        
                        languages.Add((Language)language);
                        break;
                    }
                }

                languageData.currentLanguage = languages[1];
                
                for (var i = 0; i < languages.Count; i++)
                {
                    var textInfo = new LanguageTextInfo();
                    
                    #if !PLUGVR_NETWORK_QIYU
 
                    if (!sentence[0, i].Contains(Language.English.ToString()) && !sentence[0, i].Contains("iQIYI"))
                        continue;
                    
                    #else
                    
                    if (sentence[0, i] != Language.English.ToString() && sentence[0, i] != Language.Korean.ToString()
                        && sentence[0, i].Contains(Language.Chinese.ToString()))
                        continue;
                    
                    #endif
                    
                    if(languageData.keyList.Contains(languages[i]))
                        continue;
                    
                    languageData.keyList.Add(languages[i]);
                    textInfo.language = languages[i];

                    var sentenceIndex = -1;
                    
                    for(var a = 0; a < columnSize; a++)
                    {
                        if (sentence[0, a] == null)
                            continue;
                        
                        if (sentence[0, a].Contains(languages[i].ToString()))
                        {
                            sentenceIndex = a;
                            break;
                        }
                    }
                    
                    for (var j = 1; j < rowSize; j++)
                    {
                        if (sentence[j, 0] == null)
                            continue;
                        
                        // 맨 뒤에 붙는 아스키 코드값 13 제거 
                        // 아스키 코드 13 : Carriage Return

                        var key = sentence[j, 0].Replace(" ", string.Empty);
                        var tmp = sentence[j, sentenceIndex].Replace((char) 13, (char) 0);

                        if (textInfo.IsExistKey(key))
                        {
                            Debug.LogWarning($"중복 키 : {key} - {tmp}");
                            continue;
                        }

                        if (tmp.Equals(string.Empty))
                        {
                            tmp = sentence[j, 1].Replace((char) 13, (char) 0);
                            textInfo.languageContextKeys.Add(key);
                            textInfo.languageContextValue.Add(tmp);
                        }
                        else
                        {
                            textInfo.languageContextKeys.Add(key);
                            textInfo.languageContextValue.Add(tmp);
                        }
                    }
                    
                    languageData.infoList.Add(textInfo);
                }
                
                AssetDatabase.SaveAssetIfDirty(languageData);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void SetLanguageTextureData(string tsv)
        {
            
        }

        private void SetLanguageImageData(string tsv)
        {
            
        }
    }
}

#endif