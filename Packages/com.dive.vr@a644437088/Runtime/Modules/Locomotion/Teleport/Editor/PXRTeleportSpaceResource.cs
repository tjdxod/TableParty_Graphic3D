#if UNITY_EDITOR
using Dive.Utility;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public class PXRTeleportSpaceResource : ScriptableObject
    {
        #region Public Fields

        public static StaticVar<PXRTeleportSpaceResource> Instance;

        #endregion
        
        #region Private Fields

        [SerializeField, LabelText("외부 영역 선 두께")]
        private float baseThickness = 5;
        
        [SerializeField, LabelText("외부 영역 색상")]
        private Color baseColor = new Color(0, 0.8f, 0.4f, 1);
        
        [SerializeField, LabelText("외부 영역 포인트 색상")]
        private Color baseSphereColor = new Color(0.8f, 0.4f, 0.8f);
        
        [SerializeField, LabelText("외부 영역 포인트 크기")]
        private float basePointSize = 0.4f;

        [SerializeField, LabelText("외부 영역 포인트 인덱스 글씨 크기")]
        private int basePointIndexFontSize = 22;

        [SerializeField, LabelText("외부 영역 포인트 인덱스 글씨 색상")]
        private Color basePointIndexFontColor = Color.red;
        
        [SerializeField, LabelText("외부 영역 포인트 글씨 크기")]
        private int basePointFontSize = 18;
        
        [SerializeField, LabelText("외부 영역 포인트 글씨 색상")]
        private Color basePointFontColor = Color.black;

        [Space]
        
        [SerializeField, LabelText("내부 영역 선 두께")]
        private float innerThickness = 5f;
        
        [SerializeField, LabelText("내부 영역 색상")]
        private Color innerColor = new Color(1, 0.4f, 0.4f, 1);
        
        [SerializeField, LabelText("내부 영역 포인트 색상")]
        private Color innerSphereColor = new Color(0.8f, 0.4f, 0.8f);
        
        [SerializeField, LabelText("내부 영역 포인트 크기")]
        private float innerPointSize = 0.3f;
        
        [SerializeField, LabelText("내부 영역 포인트 인덱스 글씨 크기")]
        private int innerPointIndexFontSize = 22;
        
        [SerializeField, LabelText("내부 영역 포인트 인덱스 글씨 색상")]
        private Color innerPointIndexFontColor = Color.red;
        
        [SerializeField, LabelText("내부 영역 포인트 글씨 크기")]
        private int innerPointFontSize = 18;
        
        [SerializeField, LabelText("내부 영역 포인트 글씨 색상")]
        private Color innerPointFontColor = Color.black;    
        
        #endregion

        #region Public Properties

        public static float BaseThickness => Instance.Value.baseThickness;
        public static Color BaseColor => Instance.Value.baseColor;
        public static float BasePointSize => Instance.Value.basePointSize;
        public static Color BaseSphereColor => Instance.Value.baseSphereColor;
        public static int BasePointIndexFontSize => Instance.Value.basePointIndexFontSize;
        public static Color BasePointIndexFontColor => Instance.Value.basePointIndexFontColor;
        public static int BasePointFontSize => Instance.Value.basePointFontSize;
        public static Color BasePointFontColor => Instance.Value.basePointFontColor;
        
        public static float InnerThickness => Instance.Value.innerThickness;
        public static Color InnerColor => Instance.Value.innerColor;
        public static float InnerPointSize => Instance.Value.innerPointSize;
        public static Color InnerSphereColor => Instance.Value.innerSphereColor;
        public static int InnerPointIndexFontSize => Instance.Value.innerPointIndexFontSize;
        public static Color InnerPointIndexFontColor => Instance.Value.innerPointIndexFontColor;
        public static int InnerPointFontSize => Instance.Value.innerPointFontSize;
        public static Color InnerPointFontColor => Instance.Value.innerPointFontColor;

        #endregion
        
        #region Public Methods

        [InitializeOnLoadMethod]
        public static void UpdateArguments()
        {
            var load = Resources.Load<PXRTeleportSpaceResource>("PXRTeleportSpaceEditorArguments");

            if (load == null)
            {
                Debug.LogError("PXRTeleportSpaceEditorArguments is null.");
                return;
            }

            Instance = new StaticVar<PXRTeleportSpaceResource>(load);
        }        

        #endregion
    }
}
#endif