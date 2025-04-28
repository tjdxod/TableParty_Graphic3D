using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Dive.Utility;


namespace Dive.VRModule.Locomotion
{
    // 오큘러스 기준
    // secondary가 위, primary가 아래
    public class PXREyeHeightChanger : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static StaticVar<bool> IsEnableAB = new StaticVar<bool>(true);

        /// <summary>
        /// 
        /// </summary>
        public bool isApplyPlayerPrefab = true;

        /// <summary>
        /// 
        /// </summary>
        public event Action<float> BeforeChangeHeightEvent;

        /// <summary>
        /// 
        /// </summary>
        public event Action<float> AfterChangeHeightEvent;

        #endregion

        #region Private Fields

        [SerializeField]
        private HandSide handSide;

        [SerializeField]
        private LayerMask floorLayer;

        [SerializeField]
        private LayerMask ceilingLayer;

        private PXRPlayerController playerController;
        private Camera mainCam;
        private IEnumerator coroutineResetEyeHeight;

        private const string HeightKey = "Height";

        private bool isMovingHeight;

        #endregion

        #region Public Properties

        [field: SerializeField]
        public float UpLimit { get; private set; } = 1.5f;
        
        [field: SerializeField]
        public float DownLimit { get; private set; } = 0.5f;
        
        [field: SerializeField, Range(0.01f, 1f)]
        public float EyeMoveTime { get; private set; } = 0.2f;

        [field: SerializeField]
        public float EyeMoveHeight { get; private set; } = 0.1f;

        #endregion

        #region Private Properties

        private PXRPlayerController PlayerController
        {
            get
            {
                if (playerController == null)
                    playerController = GetComponent<PXRPlayerController>();

                return playerController;
            }
        }

        private Camera MainCam
        {
            get
            {
                if (mainCam == null)
                {
                    mainCam = Camera.main;
                }

                return mainCam;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 현재 진행중인 이동을 중지
        /// </summary>
        public void CancelMovingHeight()
        {
            DOTween.KillAll();

            if (coroutineResetEyeHeight != null)
            {
                StopCoroutine(coroutineResetEyeHeight);
                coroutineResetEyeHeight = null;
            }

            isMovingHeight = false;
        }

        /// <summary>
        /// 올라갈 수 있는 상태인지 체크
        /// 닿는게 없으면 못올라감
        /// </summary>
        /// <returns>올라갈 수 있는 상태</returns>
        public bool CheckCanMoveUp()
        {
            var ray = new Ray(MainCam.transform.position, Vector3.up);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ceilingLayer, QueryTriggerInteraction.UseGlobal))
            {
                return hit.distance > UpLimit;
            }

            return false;
        }

        /// <summary>
        /// 내려갈 수 있는 상태인지 체크
        /// 닿는게 없으면 못내려감
        /// </summary>
        /// <returns>내려갈 수 있는 상태</returns>
        public bool CheckCanMoveDown()
        {
            var ray = new Ray(MainCam.transform.position, Vector3.down);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, floorLayer, QueryTriggerInteraction.UseGlobal))
            {
                return hit.distance > DownLimit;
            }

            return false;
        }

        /// <summary>
        /// 플레이어의 눈높이를 Lerp하게 변경
        /// </summary>
        /// <param name="isUp">true인 경우 위로, false인 경우 아래로</param>
        public void ChangeEyeHeight(bool isUp)
        {
            if (isMovingHeight)
                return;

            isMovingHeight = true;

            var moveLocalHeight = isUp ? EyeMoveHeight : -EyeMoveHeight;

            if (isApplyPlayerPrefab)
            {
                if (isUp)
                {
                    var getHeight = PlayerPrefs.GetInt(HeightKey, 0);
                    PlayerPrefs.SetInt(HeightKey, getHeight + 1);
                }
                else
                {
                    var getHeight = PlayerPrefs.GetInt(HeightKey, 0);
                    PlayerPrefs.SetInt(HeightKey, getHeight - 1);
                }
            }

            var nextHeight = transform.position.y + moveLocalHeight;
            PlayerController.CurrentHeight += moveLocalHeight;

            BeforeChangeHeightEvent?.Invoke(nextHeight);

            PXRPCPlayerMovement pcMovement = null;
            if (!PXRRig.IsVRPlay)
            {
                pcMovement = GetComponent<PXRPCPlayerMovement>();
                pcMovement.DisableMove();
            }

            transform.DOMoveY(nextHeight, EyeMoveTime)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    AfterChangeHeightEvent?.Invoke(moveLocalHeight);
                    isMovingHeight = false;

                    Vector3 tmpVec = PlayerController.TeleportPivot.position;
                    tmpVec.y = nextHeight;
                    PlayerController.TeleportPivot.position = tmpVec;

                    if (pcMovement)
                        pcMovement.EnableMove();
                });
        }

        public void ChangeEyeTargetHeight(float targetHeight)
        {
            if (isMovingHeight)
                return;

            isMovingHeight = true;

            PlayerController.CurrentHeight = targetHeight;

            BeforeChangeHeightEvent?.Invoke(targetHeight);

            PXRPCPlayerMovement pcMovement = null;
            if (!PXRRig.IsVRPlay)
            {
                pcMovement = GetComponent<PXRPCPlayerMovement>();
                pcMovement.DisableMove();
            }

            transform.DOMoveY(targetHeight, EyeMoveTime)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    AfterChangeHeightEvent?.Invoke(transform.position.y - targetHeight);
                    isMovingHeight = false;

                    Vector3 tmpVec = PlayerController.TeleportPivot.position;
                    tmpVec.y = targetHeight;
                    PlayerController.TeleportPivot.position = tmpVec;

                    if (pcMovement)
                        pcMovement.EnableMove();
                });
        }

        /// <summary>
        /// 플레이어의 눈높이를 즉시 변경
        /// </summary>
        /// <param name="count">count * 0.1f 만큼 변경</param>
        public void ChangeEyeHeightImmediately(int count)
        {
            if (isMovingHeight)
                return;

            isMovingHeight = true;

            var moveLocalHeight = EyeMoveHeight * count;
            var nextHeight = transform.position.y + moveLocalHeight;

            PlayerController.CurrentHeight += moveLocalHeight;

            // PC
            PXRPCPlayerMovement pcMovement = null;
            if (!PXRRig.IsVRPlay)
            {
                pcMovement = GetComponent<PXRPCPlayerMovement>();
                pcMovement.DisableMove();
            }

            var position = transform.position;

            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.position = new Vector3(position.x, nextHeight, position.z);
            isMovingHeight = false;

            Vector3 tmpVec = PlayerController.TeleportPivot.position;
            tmpVec.y = nextHeight;
            PlayerController.TeleportPivot.position = tmpVec;

            // PC
            if (pcMovement)
                pcMovement.EnableMove();
        }

        /// <summary>
        /// 플레이어의 눈 높이를 초기 상태로 변경
        /// </summary>
        /// <param name="fadeTime">변경 시 페이드 인 아웃 타임</param>
        public void ResetEyeHeight(float fadeTime = 0.2f)
        {
            if (coroutineResetEyeHeight != null)
            {
                StopCoroutine(coroutineResetEyeHeight);
                coroutineResetEyeHeight = null;
            }

            coroutineResetEyeHeight = CoroutineResetEyeHeight(fadeTime);
            StartCoroutine(coroutineResetEyeHeight);
        }

        #endregion

        #region Private Methods

        private void Update()
        {
            if (!IsEnableAB.Value)
                return;

            if (PXRInputBridge.GetXRController(handSide).GetButtonState(Buttons.Secondary).isStay)
            {
                if (CheckCanMoveUp())
                    ChangeEyeHeight(true);
            }
            else if (PXRInputBridge.GetXRController(handSide).GetButtonState(Buttons.Primary).isStay)
            {
                if (CheckCanMoveDown())
                    ChangeEyeHeight(false);
            }
        }

        private IEnumerator CoroutineResetEyeHeight(float fadeTime = 0.2f)
        {
            var isComplete = false;

            try
            {
                PXRScreenFade.StartCameraFade(1, fadeTime);
                while (PXRScreenFade.Instance.IsCameraFading)
                    yield return null;

                EyeHeightToZero();

                PXRScreenFade.StartCameraFade(0, fadeTime);
                while (PXRScreenFade.Instance.IsCameraFading)
                    yield return null;

                isComplete = true;
            }
            finally
            {
                if (!isComplete)
                {
                    EyeHeightToZero();
                }
            }
        }

        private void EyeHeightToZero()
        {
            var localPos = transform.localPosition;
            localPos.y = 0f;
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.localPosition = localPos;

            var tmpVec = PlayerController.TeleportPivot.position;
            tmpVec.y = 0f;
            PlayerController.TeleportPivot.position = tmpVec;

            PlayerController.CurrentHeight = 0f;
        }

        #endregion
    }
}