using System;
using System.Collections;
using System.Collections.Generic;
using Dive.VRModule.Locomotion;
using Dive.Utility;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.
    public partial class PXRTeleporter : MonoBehaviour
    {
        #region Public Fields

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static StaticVar<bool> UseTeleport = new StaticVar<bool>(true);

        public event Action ExecuteTeleportEvent;
        public event Action BeforeTeleportEvent;
        public event Action AfterTeleportEvent;
        public event Action CancelTeleportEvent;

        #endregion

        #region Private Fields

        [Header("컨트롤러")]
        [SerializeField, LabelText("컨트롤러의 2D 축")]
        private ControllerAxis controllerAxis;

        [SerializeField, LabelText("텔레포트 방향의 기준이 되는 오브젝트")]
        private Transform teleporterOrigin;

        [SerializeField, LabelText("이동 지점을 나타내는 오브젝트")]
        private PXRTeleportMarker teleportMarker;

        [SerializeField, LabelText("방향을 나타내는 오브젝트")]
        private Transform teleportMarkerDirection;

        private Vector2 axisValue = Vector2.zero;

        private PXRInputHandlerBase inputHandler;
        private IEnumerator routineMoveDestination;

        private bool isMoving;
        private bool isTeleporting = false;
        private bool isTeleportCancel = false;
        private bool isOriginScale = false;
        
        private readonly List<PXRTeleportSpaceBase> teleportSpaces = new List<PXRTeleportSpaceBase>();
        private const float FadeTime = 0.15f;

        // X축 임계값
        private const float XAxisThreshold = 0.4f;

        // Y축 임계값
        private const float YAxisThreshold = 0.75f;
        private const float HeightLimitAngle = 45f;

        #endregion

        #region Public Properties

        [field: Space, SerializeField, LabelText("컨트롤러 손")]
        public HandSide HandSide { get; private set; }

        public bool IsTeleporting
        {
            get => isTeleporting;
            private set
            {
                isTeleporting = value;
                if (value)
                {
                    Pointer.CanProcess = false;
                    PlayerController.ChangeHandState(HandSide, HandInteractState.Teleporting);
                }
                else
                {
                    Pointer.CanProcess = true;
                    PlayerController.ChangeHandState(HandSide, HandInteractState.None);
                }
            }
        }

        #endregion

        #region Private Properties

        private PXRPointerVR Pointer => HandSide == HandSide.Left ? PXRRig.LeftPointer : PXRRig.RightPointer;
        private PXRInputHandlerBase InputHandler => PXRInputBridge.GetXRController(HandSide);
        private PXRPlayerController PlayerController => PXRRig.PlayerController;
        private int TeleportLayer => PXRNameToLayer.TeleportSpace;

        #endregion

        #region Public Methods

        public void AddTeleportSpace(PXRTeleportSpaceBase teleportSpace)
        {
            if (!teleportSpaces.Contains(teleportSpace))
                teleportSpaces.Add(teleportSpace);
        }

        public void RemoveTeleportSpace(PXRTeleportSpaceBase teleportSpace)
        {
            if (teleportSpaces.Contains(teleportSpace))
                teleportSpaces.Remove(teleportSpace);
        }

        /// <summary>
        /// 강제로 텔레포트를 해야하는 상황에 이벤트를 발생시키기 위한 메소드
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        public void ForceTeleport(Vector3 position, Vector3 direction)
        {
            BeforeTeleportEvent?.Invoke();
            
            PXRRig.PlayerController.TurnToDirection(direction);
            PXRRig.PlayerController.MoveToFixedDestination(position);
            
            ExecuteTeleportEvent?.Invoke();
            AfterTeleportEvent?.Invoke();
        }

        
        public void CancelTeleport()
        {
            isTeleportCancel = true;
            IsTeleporting = false;
            tubeRenderer.Hide();

            if (isMoving)
            {
                isMoving = false;
                PlayerController.IsMoving = false;
                PXRScreenFade.StopFade();
                PXRScreenFade.ClearCameraImage();
            }

            InactivateTeleportSpace();

            ChangeTeleportVisual(TeleportLineState.Deactivate);
        }

        #endregion

        #region Private Methods

        private void Update()
        {
            if (!UseTeleport.Value)
                return;

            if (PXRRig.PlayerController.UseMovement && HandSide == HandSide.Left)
                return;
            
            var pointerEventData = Pointer.PointerEventData;
            var scrollDelta = Vector2.zero;
            var magnitude = 0f;
            
            if(pointerEventData != null)
                scrollDelta = pointerEventData.scrollDelta;
            
            magnitude = scrollDelta.magnitude;

            var isScrollView = magnitude > 0.01f;
            
            axisValue = InputHandler.GetAxisValue(controllerAxis);

            if (!isTeleportCancel && !isScrollView)
            {
                TryTeleport();
            }
            else
            {
                if (!isScrollView)
                {
                    var deadZone = InputHandler.StickDeadZone;
                    if (!(Mathf.Abs(axisValue.x) < deadZone.x) || !(Mathf.Abs(axisValue.y) < deadZone.y))
                        return;
                }

                isTeleportCancel = false;
                tubeRenderer.Hide();
                InactivateTeleportSpace();
            }
        }

        private void TryTeleport()
        {
            if (!isTeleporting)
            {
                if (!PossibleTeleport())
                    return;

                if (!(axisValue.y > YAxisThreshold) || !(Mathf.Abs(axisValue.x) < XAxisThreshold))
                    return;

                IsTeleporting = true;
                ActivateTeleportSpace();

                LineRendererController.SetOriginWidth();
            }
            else
            {
                if (!isMoving && !PlayerController.IsMoving)
                {
                    if (axisValue.sqrMagnitude > InputHandler.StickDeadZone.sqrMagnitude)
                    {
                        LineRendererController.EnableLineRenderer();
                        ExecuteRay();

                        if (!InputHandler.GetButtonState(Buttons.PrimaryAxis).isDown)
                            return;

                        CancelTeleport();
                        CancelTeleportEvent?.Invoke();
                    }
                    else
                    {
                        teleportLineState = TeleportLineState.Default;

                        tubeRenderer.Hide();
                        LineRendererController.DisableLineRenderer();
                        teleportMarker.Inactivate();
                        MoveTeleport();
                    }
                }
                else
                {
                    LineRendererController.DisableLineAndClearAllPosition(transform.position);
                }
            }
        }


        private void MoveTeleport()
        {
            InactivateTeleportSpace();

            if (targetHit == null)
            {
                IsTeleporting = false;
            }
            else
            {
                if (routineMoveDestination != null)
                {
                    StopCoroutine(routineMoveDestination);
                    routineMoveDestination = null;
                }

                routineMoveDestination = CoroutineMoveDestination();
                StartCoroutine(routineMoveDestination);
            }
        }

        private void ActivateTeleportSpace()
        {
            foreach (var space in teleportSpaces)
            {
                if (space == PXRRig.PlayerController.PrevTeleportSpace)
                    continue;

                space.ActiveSpace();
                space.AdditionalActive();
            }
        }

        private void InactivateTeleportSpace()
        {
            foreach (var space in teleportSpaces)
            {
                space.InactiveSpace();
                space.AdditionalInactive();
            }
        }

        /// <summary>
        /// 텔레포트 이동
        /// </summary>
        /// <param name="teleportSpace">이동하려는 TeleportSpace</param>
        /// <param name="isForced">강제 이동 체크</param>
        /// <returns></returns>
        private IEnumerator CoroutineMoveDestination(PXRTeleportSpaceBase teleportSpace = null, bool isForced = false)
        {
            isMoving = true;
            PlayerController.IsMoving = true;

            BeforeTeleportEvent?.Invoke();

            try
            {
                PXRScreenFade.StartCameraFade(1, FadeTime);
                while (PXRScreenFade.Instance.IsCameraFading)
                    yield return null;

                if (targetHit != null)
                    teleportSpace = teleportSpace != null ? teleportSpace : targetHit.Value.transform.GetComponent<ITeleportSpace>().GetOriginalTeleportSpace();
                else
                    yield break;

                if (teleportSpace.CanTeleport)
                {
                    var currentSpace = PXRRig.RigMovement.CurrentTeleportSpaceArea;

                    if (teleportSpace.SpaceType == SpaceType.Point)
                    {
                        var point = teleportSpace as PXRTeleportPoint;

                        if (point == null)
                        {
                            Debug.LogWarning("SpaceType와 클래스가 매칭되지 않습니다.");
                            yield break;
                        }

                        if (currentSpace == null)
                        {
                            currentSpace = point;
                            currentSpace.OnEnterPlayer();
                        }

                        if (point.GetTeleportSpace().GetHashCode() != currentSpace.GetHashCode())
                        {
                            currentSpace.OnExitPlayer();
                            teleportSpace = point.GetTeleportSpace();
                            PXRRig.RigMovement.SetTeleportSpaceArea(teleportSpace);
                            teleportSpace.OnEnterPlayer();
                        }
                    }
                    else
                    {
                        var area = teleportSpace as PXRTeleportArea;

                        if (currentSpace == null)
                        {
                            currentSpace = area;
                        }

                        if (area != null && currentSpace != null && area.GetHashCode() != currentSpace.GetHashCode())
                        {
                            currentSpace.OnExitPlayer();
                            teleportSpace = area;
                            PXRRig.RigMovement.SetTeleportSpaceArea(teleportSpace);
                            teleportSpace.OnEnterPlayer();
                        }
                    }

                    if (!teleportSpace.UseFixedDirection)
                    {
                        if (isForced)
                        {
                            var areaTransform = teleportSpace.transform;
                            var areaPosition = areaTransform.position;

                            PlayerController.TurnToDirection(areaTransform.forward);
                            PlayerController.MoveToFixedDestination(areaPosition);
                        }
                        else
                        {
                            var markerOffset = teleportMarker.transform.position;

                            PlayerController.TurnToDirection(teleportMarkerDirection.forward);
                            PlayerController.MoveToFixedDestination(markerOffset);
                        }
                    }
                    else
                    {
                        PlayerController.TurnToDirection(teleportSpace.UseForceTransform ? teleportSpace.ForceTransform.forward : teleportSpace.transform.forward);

                        if (teleportSpace.SpaceType == SpaceType.Point)
                        {
                            PlayerController.MoveToFixedDestination(teleportSpace.UseForceTransform ? teleportSpace.ForceTransform.position : teleportSpace.transform.position);
                        }
                        else
                        {
                            var markerOffset = teleportMarker.transform.position;
                            PlayerController.MoveToFixedDestination(markerOffset);
                        }
                    }

                    PlayerController.ChangePrevTeleportPoint(teleportSpace);

                    ExecuteTeleportEvent?.Invoke();
                }

                PXRScreenFade.StartCameraFade(0, FadeTime);
                while (PXRScreenFade.Instance.IsCameraFading)
                    yield return null;

                AfterTeleportEvent?.Invoke();
                PlayerController.OnAfterMovement();
            }
            finally
            {
                PXRScreenFade.ClearCameraImage();

                isMoving = false;
                PlayerController.IsMoving = false;

                IsTeleporting = false;
                destinationPointOverride = null;
                markerDirectionOverride = null;
                currentHitTeleportPoint = null;

                routineMoveDestination = null;
            }
        }

        private bool PossibleTeleport()
        {
            if (isMoving || PlayerController.IsMoving)
                return false;

            if (PlayerController.GetHandState(HandSide) != HandInteractState.None)
                return false;

            if (PlayerController.GetOtherHandState(HandSide) == HandInteractState.Teleporting)
                return false;

            if (PlayerController.GetHandState(HandSide) == HandInteractState.Clicking || PlayerController.GetOtherHandState(HandSide) == HandInteractState.Clicking)
                return false;

            return true;
        }

        private void RotateMarkerDirection()
        {
            teleportMarkerDirection.transform.GetChild(2).localScale = Vector3.one * 1f;
            teleportMarkerDirection.transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0.35f);

            if (markerDirectionOverride == null)
            {
                var normalVec = axisValue.normalized;
                var newRot = Vector3.up * (Mathf.Atan2(normalVec.x, normalVec.y) * Mathf.Rad2Deg);

                var forward = transform.forward;
                forward.y = 0f;

                var direction = Quaternion.Euler(newRot) * forward;
                direction.Normalize();

                teleportMarkerDirection.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                teleportMarkerDirection.rotation = Quaternion.LookRotation(markerDirectionOverride.Value);
            }
        }

        /// <summary>
        /// 텔레포트 마커를 켜고 끌지 결정
        /// </summary>
        /// <param name="lineState">라인 상태</param>
        private void ChangeTeleportVisual(TeleportLineState lineState)
        {
            if (teleportLineState == lineState)
                return;

            teleportLineState = lineState;

            switch (teleportLineState)
            {
                case TeleportLineState.Enable:
                    LineRendererController.ChangeEnableColor();

                    teleportMarker.Activate();
                    teleportMarker.ActivateEnableMarker();
                    break;

                case TeleportLineState.Disable:
                    LineRendererController.ChangeDisableColor();

                    teleportMarker.Activate();
                    teleportMarker.ActivateDisableMarker();
                    break;

                case TeleportLineState.MarkerOff:
                    LineRendererController.ChangeEnableColor();

                    teleportMarker.Inactivate();
                    break;

                case TeleportLineState.Deactivate:
                    LineRendererController.DisableLineRenderer();

                    teleportMarker.Inactivate();
                    break;
            }
        }

        #endregion
    }
}