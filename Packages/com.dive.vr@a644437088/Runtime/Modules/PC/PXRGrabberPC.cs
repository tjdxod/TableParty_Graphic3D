using System.Collections;
using UnityEngine;

namespace Dive.VRModule
{
    public class PXRGrabberPC : PXRGrabber
    {
        #region Private Fields

        private Vector3 originLocalPos;
        private Buttons gripButton;
        private IEnumerator routineCheckPressing;

        // 블록쌓기에서 사용할, 잡았을 때 가운데로 오게 하는 
        private bool isOneHand = false;
        private float gripButtonPressTime = 0f;

        #endregion

        #region Public Methods

        protected override void Awake()
        {
            base.Awake();

            originLocalPos = transform.localPosition;
        }

        private void Start()
        {
            playerController.AfterDeactivatedAllEvent += ChangeTwoHand;

            gripButton = HandSide switch
            {
                HandSide.Left => Buttons.LeftMouse,
                HandSide.Right => Buttons.RightMouse,
                _ => gripButton
            };
        }

        private void OnDestroy()
        {
            playerController.AfterDeactivatedAllEvent -= ChangeTwoHand;
        }
        
        protected override void Update()
        {
            if (!inputHandler.GetButtonState(gripButton).isDown)
                return;

            if (!IsGrabbing)
            {
                TryGrab();
            }
            else
            {
                if (routineCheckPressing != null)
                {
                    StopCoroutine(routineCheckPressing);
                    routineCheckPressing = null;
                }

                routineCheckPressing = CheckPressing();
                StartCoroutine(routineCheckPressing);
            }
        }
        
        private IEnumerator CheckPressing()
        {
            gripButtonPressTime = 0f;

            while (!inputHandler.GetButtonState(gripButton).isUp)
            {
                gripButtonPressTime += Time.deltaTime;
                yield return null;
            }

            TryRelease();
        }
        
        protected override void TryGrab()
        {
            if (IsGrabbing && !GrabbedGrabbable)
            {
                InitializeGrabber();
                return;
            }

            GrabbedGrabbable = null;

            var grabbable = pointer.CurrentObjectOnPointer.GetGrabbable();

            if (!grabbable || !grabbable.enabled || grabbable.TransferState == TransferState.None || grabbable.IsGrabbed)
                return;

            if (grabbable && !grabbable.IsEnableDistanceGrab)
                return;

            if (isOneHand && otherGrabber.IsGrabbing)
                return;

            // 테스트해보기
            CompletedTryGrab(grabbable);
        }
        
        private void CompletedTryGrab(PXRGrabbable grabbable)
        {
            // PC에서 클릭했을 때 자동으로 움직이는 Grabbable 실행 (ex.주사위컵)
            if (grabbable.MouseClickEvent != null)
            {
                grabbable.MouseClickEvent?.Invoke();
                return;
            }

            rotationPivot.transform.rotation = Quaternion.identity;

            BeforeGrab(ref grabbable);

            if (!grabbable)
                return;

            GrabOnPointer();
            AfterGrab();
        }
        
        protected override void AfterGrab()
        {
            pointer.CanProcess = false;

            GrabbedGrabbable.Grabbed(this);
            OnGrabbed();

            directionPivot.transform.localPosition = Vector3.forward;
            // ReSharper disable once Unity.InefficientPropertyAccess
            directionPivot.transform.rotation = Quaternion.identity;

            // 무조건 손으로 바로 오게
            GrabbedGrabbable.ResetVelocity();
            targetDistance = 0f;
        }
        
        protected override void Release()
        {
            gripButtonPressTime = gripButtonPressTime < 1f ? 0f : Mathf.Clamp(gripButtonPressTime, 1f, 3f);
            GrabbedGrabbable.ForceChangeVelocity(transform.forward * (gripButtonPressTime * 2f), Vector3.zero);

            base.Release();
        }
        
        private void ChangeHandCount()
        {
            // 두손으로 변경
            if (isOneHand)
            {
                ChangeTwoHand();
            }
            // 한손으로 변경
            else
            {
                ChangeOneHand();
            }
        }

        #endregion

        #region Private Methods

        public void ChangeOneHand()
        {
            isOneHand = true;
            var tmp = transform.localPosition;
            tmp.x = 0f;
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.localPosition = tmp;
        }


        public void ChangeTwoHand()
        {
            isOneHand = false;
            transform.localPosition = originLocalPos;
        }

        #endregion
    }
}