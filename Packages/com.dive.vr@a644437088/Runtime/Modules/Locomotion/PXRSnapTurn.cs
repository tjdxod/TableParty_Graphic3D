using System;
using System.Collections;
using UnityEngine;

namespace Dive.VRModule.Locomotion
{
    public class PXRSnapTurn : MonoBehaviour
    {
        #region Public Fields

        public event Action<float> ExecuteTurnEvent;
        public event Action BeforeSnapTurnEvent;
        public event Action ExecuteSnapTurnEvent;
        public event Action AfterSnapTurnEvent;        
        
        #endregion
        
        #region Private Fields

        [SerializeField]
        private bool canTurn = true;

        [SerializeField]
        private bool canGrabTurn = true;
        
        private PXRPlayerController playerController;
        
        private readonly float xAxisThreshold = 0.8f;
        private readonly float yAxisThreshold = 0.4f;
        private readonly float fadeTime = 0.2f;
        private readonly float holdTime = 0.3f;
        private float elapsedHoldTime = 0f;        
        
        private IEnumerator coroutineDoTurn;
        
        // 이 클래스에서가 아닌 강제 이동인 경우를 체크하기 위한 변수
        private bool isMoving;        
        
        #endregion
        
        #region Public Properties
        
        [field: SerializeField]
        public HandSide HandSide { get; private set; }        

        [field: SerializeField]
        public float TurnAngle { get; private set; } = 45f;        
        
        #endregion

        #region Public Methods

        /// <summary>
        /// 회전이 가능한 상태인지 체크
        /// </summary>
        /// <returns>회전 가능 상태</returns>
        public bool CheckCanTurn()
        {
            if (!canTurn)
                return false;

            var handState = playerController.GetHandState(HandSide);

            if (canGrabTurn)
            {
                if (handState != HandInteractState.None && handState != HandInteractState.Grabbing)
                    return false;
            }
            else
            {
                if (handState != HandInteractState.None)
                    return false;
            }
            
            if (playerController.IsMoving)
                return false;

            if (isMoving)
                return false;

            return true;
        }

        /// <summary>
        /// 회전 취소
        /// </summary>
        public void CancelSnapTurn()
        {
            if (isMoving)
            {
                if (coroutineDoTurn != null)
                {
                    StopCoroutine(coroutineDoTurn);
                    coroutineDoTurn = null;
                }

                isMoving = false;
                playerController.IsMoving = false;

                PXRScreenFade.StopFade();
                PXRScreenFade.ClearCameraImage();
            }
        }


        /// <summary>
        /// 회전 실행
        /// </summary>
        /// <param name="angle">회전 각도</param>
        public void Turn(float angle)
        {
            isMoving = true;
            playerController.IsMoving = true;

            if (coroutineDoTurn != null)
            {
                StopCoroutine(coroutineDoTurn);
                coroutineDoTurn = null;
            }

            coroutineDoTurn = CoroutineDoTurn(angle);
            StartCoroutine(coroutineDoTurn);
        }        
        
        public void ChangeCanGrabTurn(bool value)
        {
            canGrabTurn = value;
        }
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            playerController = GetComponentInParent<PXRPlayerController>();
        }
        
        private void Update()
        {
            if (!PXRTeleporter.UseTeleport.Value)
                return;
            
            if (PXRRig.PlayerController.UseMovement && HandSide == HandSide.Left)
                return;
            
            // if (PXRTeleporter.IsDisableTeleport.Value)
                // return;
            
            if (!CheckCanTurn())
                return;

            TryTurn();
        }
        
        /// <summary>
        /// 회전 가능 체크 후 회전 시도
        /// </summary>
        private void TryTurn()
        {
            var turnAxis = PXRInputBridge.GetXRController(HandSide).GetAxisValue(ControllerAxis.Primary);

            // 연속으로 실행되지 않게 하기 위해 && 뒷부분 실행
            if (Mathf.Abs(turnAxis.x) > xAxisThreshold && Mathf.Abs(turnAxis.y) < yAxisThreshold)
            {
                if (elapsedHoldTime == 0f)
                {
                    var sign = Mathf.Sign(turnAxis.x);
                    Turn(sign * TurnAngle);
                    elapsedHoldTime += Time.deltaTime;
                }
                else
                {
                    elapsedHoldTime += Time.deltaTime;
                    if (elapsedHoldTime > holdTime)
                        elapsedHoldTime = 0f;
                }                
            }
            else
            {
                elapsedHoldTime = 0f;
            }
        }        
        
        /// <summary>
        /// 회전 진행
        /// </summary>
        /// <param name="angle">회전 각도</param>
        /// <returns></returns>
        private IEnumerator CoroutineDoTurn(float angle)
        {
            BeforeSnapTurnEvent?.Invoke();

            try
            {
                PXRScreenFade.StartCameraFade(1, fadeTime);
                while (PXRScreenFade.Instance.IsCameraFading)
                    yield return null;
                
                // 강제이동때문에 IsMoving이 바뀔수가 있어서 체크
                if (!playerController.IsMoving)
                {
                    isMoving = false;
                    yield break;
                }

                playerController.TurnToAngle(angle);
                ExecuteTurnEvent?.Invoke(angle);
                ExecuteSnapTurnEvent?.Invoke();

                PXRScreenFade.StartCameraFade(0, fadeTime);
                while (PXRScreenFade.Instance.IsCameraFading)
                    yield return null;

                AfterSnapTurnEvent?.Invoke();
            }
            finally
            {
                PXRScreenFade.ClearCameraImage();

                isMoving = false;
                playerController.IsMoving = false;
            }            
        }        
        
        #endregion
    }
}
