#if !ODIN_INSPECTOR

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Dive.Avatar.Meta
{
    internal enum InfoBoxType
    {
        None,
        Info,
        Warning,
        Error
    }
    
    internal class InfoBoxAttribute : PropertyAttribute
    {
        #region Public Fields
        
        public readonly string Message;
        public readonly InfoBoxType Type;
        
        #endregion
        
        #region Public Methods
        
        public InfoBoxAttribute(string message)
        {
            Message = message;
            Type = InfoBoxType.None;
        }

        public InfoBoxAttribute(string message, InfoBoxType type)
        {
            Message = message;
            Type = type;
        }
        
        #endregion
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    internal class InfoBoxDrawer : DecoratorDrawer
    {
        #region Public Methods
        
        public override float GetHeight()
        {
            return GetHelpBoxHeight() + 10f;
        }

        public override void OnGUI(Rect position)
        {
            if (!(attribute is InfoBoxAttribute infoBoxAttribute))
                return;

            var indentLength = GetIndentLength(position);
            var infoBoxRect = new Rect(
                position.x + indentLength,
                position.y + 5f,
                position.width - indentLength,
                GetHelpBoxHeight());

            var convertType = infoBoxAttribute.Type switch
            {
                InfoBoxType.Info => MessageType.Info,
                InfoBoxType.Warning => MessageType.Warning,
                InfoBoxType.Error => MessageType.Error,
                _ => MessageType.None
            };

            EditorGUI.HelpBox(infoBoxRect, infoBoxAttribute.Message, convertType);
        }
        
        #endregion
        
        #region Private Methods
        
        private float GetHelpBoxHeight()
        {
            var infoBoxAttribute = (InfoBoxAttribute)attribute;
            var minHeight = EditorGUIUtility.singleLineHeight * 2f;
            var desiredHeight = GUI.skin.box.CalcHeight(new GUIContent(infoBoxAttribute.Message),
                EditorGUIUtility.currentViewWidth);
            var height = Mathf.Max(minHeight, desiredHeight);

            return height;
        }

        private static float GetIndentLength(Rect sourceRect)
        {
            var indentRect = EditorGUI.IndentedRect(sourceRect);
            var indentLength = indentRect.x - sourceRect.x;

            return indentLength;
        }
        
        #endregion
    }
#endif
}

#endif