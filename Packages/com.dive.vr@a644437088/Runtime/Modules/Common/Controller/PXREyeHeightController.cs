using Dive.VRModule.Locomotion;
using UnityEngine;

namespace Dive.VRModule
{
    // PXREyeHeightController.cs
    public partial class PXRPlayerController
    {
        #region Private Fields

        [SerializeField, Tooltip("개발자 표준 높이")]
        private float standardHeight = 1.1f;

        private float originTrackingSpaceY = 0f;

        private bool isChangeStandardHeight = false;
        private float initStandardHeight = 0f;

        private bool isHeightChangeActive = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Rig의 높이 조절
        /// </summary>
        public PXREyeHeightChanger EyeHeightChanger { get; private set; }

        /// <summary>
        /// 현재 플레이어의 높이
        /// </summary>
        public float CurrentHeight { get; set; }

        /// <summary>
        /// 표준 높이
        /// </summary>
        public float StandardHeight => standardHeight;

        #endregion

        #region Public Methods

        /// <summary>
        /// 강제로 특정 높이(standardHeight)로 눈 높이(카메라 위치)를 세팅
        /// </summary>
        public void ForceChangeEyeHeight()
        {
            ActivateChangeEyeHeight();

            var camPositionY = 0f;
            camPositionY = Camera.main.transform.position.y;

            var position = transform.position;
            originTrackingSpaceY = position.y;
            position.y = 0f;

            if (originTrackingSpaceY > 0)
            {
                camPositionY -= Mathf.Abs(originTrackingSpaceY);
            }
            else
            {
                camPositionY += Mathf.Abs(originTrackingSpaceY);
            }

            var compare = 0f;

            if (camPositionY >= standardHeight)
            {
                compare = -Mathf.Abs(camPositionY - standardHeight);
            }
            else
            {
                compare = Mathf.Abs(camPositionY - standardHeight);
            }

            position = new Vector3(position.x, compare, position.z);
            transform.position = position;

            CurrentHeight = compare;
        }

        /// <summary>
        /// 강제로 특정 높이로 눈 높이(카메라 위치)를 세팅
        /// </summary>
        public void ForceChangeEyeHeight(float targetHeight)
        {
            ActivateChangeEyeHeight();

            var camPositionY = 0f;
            camPositionY = Camera.main.transform.position.y;

            var position = transform.position;
            originTrackingSpaceY = position.y;
            position.y = 0f;

            if (originTrackingSpaceY > 0)
            {
                camPositionY -= Mathf.Abs(originTrackingSpaceY);
            }
            else
            {
                camPositionY += Mathf.Abs(originTrackingSpaceY);
            }

            var compare = 0f;

            if (camPositionY >= targetHeight)
            {
                compare = -Mathf.Abs(camPositionY - targetHeight);
            }
            else
            {
                compare = Mathf.Abs(camPositionY - targetHeight);
            }

            position = new Vector3(position.x, compare, position.z);
            transform.position = position;

            CurrentHeight = compare;
        }

        /// <summary>
        /// 눈 높이를 변경 활성화
        /// </summary>
        public void ActivateChangeEyeHeight()
        {
            EyeHeightChanger.enabled = true;
            isHeightChangeActive = true;
        }

        /// <summary>
        /// 눈 높이를 변경 비활성화
        /// </summary>
        public void DeactivateChangeEyeHeight()
        {
            EyeHeightChanger.CancelMovingHeight();
            EyeHeightChanger.enabled = false;
            isHeightChangeActive = false;
        }

        /// <summary>
        /// 눈의 높이를 초기화
        /// </summary>
        /// <param name="fadeTime">페이드 아웃 시간</param>
        public void ResetEyeHeight(float fadeTime = 0.2f)
        {
            EyeHeightChanger.ResetEyeHeight(fadeTime);
        }

        /// <summary>
        /// 표준 높이를 설정
        /// </summary>
        /// <param name="height">높이</param>
        public void SetStandardHeight(float height)
        {
            if (!isChangeStandardHeight)
            {
                initStandardHeight = standardHeight;
                isChangeStandardHeight = true;
            }

            standardHeight = height;
        }

        /// <summary>
        /// 표준 높이를 초기 설정값으로 리셋
        /// </summary>
        public void ResetStandardHeight()
        {
            standardHeight = isChangeStandardHeight ? initStandardHeight : standardHeight;
        }

        #endregion
    }
}