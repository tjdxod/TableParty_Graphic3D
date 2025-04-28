using System.Collections;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 텔레포터에 닿는 LineRenderer 컨트롤
    /// </summary>
    public class PXRTeleporterLineRendererController : PXRLineRendererController
    {
        #region Private Fields

        private static readonly int TintColor = Shader.PropertyToID("_TintColor");

        /// <summary>
        /// 텔레포트 활성화 시 색상
        /// </summary>
        [SerializeField]
        private Color32 colorEnableTeleport;

        /// <summary>
        /// 텔레포트 비활성화 시 색상
        /// </summary>
        [SerializeField]
        private Color32 colorDisableTeleport;

        private IEnumerator routineScrollTexture;
        private readonly float scrollX = 0.5f;

        #endregion

        #region Public Properties

        public Color32 ColorEnableTeleport => colorEnableTeleport;
        public Color32 ColorDisableTeleport => colorDisableTeleport;

        #endregion
        
        #region Public Methods

        /// <summary>
        /// LineRenderer의 머테리얼을 활성화 색으로 변경
        /// </summary>
        public void ChangeEnableColor()
        {
            LineRenderer.material.SetColor(TintColor, colorEnableTeleport);
        }

        /// <summary>
        /// LineRenderer의 머테리얼을 비활성화 색으로 변경
        /// </summary>
        public void ChangeDisableColor()
        {
            LineRenderer.material.SetColor(TintColor, colorDisableTeleport);
        }

        #endregion

        #region Private Methods
        
        private void OnEnable()
        {
            if (routineScrollTexture != null)
            {
                StopCoroutine(routineScrollTexture);
                routineScrollTexture = null;
                return;
            }

            routineScrollTexture = CoroutineScrollTexture();
            StartCoroutine(routineScrollTexture);
        }

        private void OnDisable()
        {
            if (routineScrollTexture == null)
                return;

            StopCoroutine(routineScrollTexture);
            routineScrollTexture = null;
        }

        /// <summary>
        /// 스크롤 텍스쳐 변경 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoroutineScrollTexture()
        {
            var elapsedTime = 0f;

            while (true)
            {
                if(routineScrollTexture == null)
                    yield break;
                
                elapsedTime += Time.deltaTime;
                var offsetX = elapsedTime * scrollX;

                LineRenderer.material.mainTextureOffset = new Vector2(-offsetX, 0);
                yield return null;
            }
        }

        #endregion
    }
}