using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using InputDevice = UnityEngine.XR.InputDevice;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    /// <summary>
    /// PlugVR XR 베이스 컨트롤러
    /// </summary>
    public class PXRBaseController : XRBaseController
    {
        #region Private Fields

        [Header("위치"), SerializeField]
        private InputActionProperty positionReference;

        [SerializeField]
        private InputActionProperty rotationReference;

        [SerializeField]
        private InputActionProperty stateReference;

        [SerializeField]
        private Transform trackingSpace;

        [SerializeField]
        private HandSide handSide;

        [SerializeField]
        private bool isDebugConnectedDevice = false;

        [SerializeField]
        private PXRBaseControllerScriptableObject controllerScriptableObject;

        [SerializeField]
        private Vector3 positionOffset = Vector3.zero;

        [SerializeField]
        private Vector3 rotationOffset = new Vector3(40, 0, 0);

        private Vector3 originalPositionOffset = Vector3.zero;
        private Vector3 originalRotationOffset = Vector3.zero;

        private OpenXRRuntimeType currentRuntime = OpenXRRuntimeType.None;
        private SupportOffsetDevice currentDevice = SupportOffsetDevice.Unknown;

        private Vector3 prePosition;
        private Quaternion preRotation;
        
        #endregion

        #region Public Properties

        public OpenXRRuntimeType CurrentRuntime => currentRuntime;
        public SupportOffsetDevice CurrentDevice => currentDevice;

        public HandSide HandSide => handSide;

        public Transform Transform => transform;

        #endregion

        #region Public Methods

        /// <summary>
        /// 컨트롤러 진동 실행
        /// </summary>
        /// <param name="amplitude">진동 세기</param>
        /// <param name="duration">진동 시간</param>
        /// <returns></returns>
        public override bool SendHapticImpulse(float amplitude, float duration)
        {
            var inputDevice = InputDevices.GetDeviceAtXRNode(handSide == HandSide.Left ? XRNode.LeftHand : XRNode.RightHand);

            if (inputDevice.name == null)
                return false;

            // PC Rig의 경우 진동 X
            if (!PXRRig.IsVRPlay)
                return false;

            var isCapability = inputDevice.TryGetHapticCapabilities(out var capabilities);
            var supportsImpulse = false;

            if (isCapability)
                supportsImpulse = capabilities.supportsImpulse;

            if (isCapability && supportsImpulse)
            {
                return inputDevice.SendHapticImpulse(0u, amplitude, duration);
            }

            return false;
        }

        public Vector3 GetControllerPosition()
        {
            var posAction = positionReference.action;
            var pos = posAction.ReadValue<Vector3>();
            var worldPosition = trackingSpace.TransformPoint(pos);

            return worldPosition;
        }

        #endregion

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();

            originalPositionOffset = positionOffset;
            originalRotationOffset = rotationOffset;

#if DIVE_PLATFORM_STEAM

            InputDevices.deviceConnected += OnDeviceConnected;

#endif
        }

        protected void OnDestroy()
        {
#if DIVE_PLATFORM_STEAM

            InputDevices.deviceConnected -= OnDeviceConnected;

#endif
        }

        private void OnDeviceConnected(InputDevice device)
        {
#if DIVE_PLATFORM_STEAM

            var openXRRuntime = UnityEngine.XR.OpenXR.OpenXRRuntime.name;
            var steamDeviceName = device.name;

            if (openXRRuntime.Contains("SteamVR"))
            {
                currentRuntime = OpenXRRuntimeType.SteamVR;

                if (steamDeviceName.Contains("Oculus"))
                {
                    currentDevice = SupportOffsetDevice.Quest2;
                }
                else if (steamDeviceName.Contains("Meta") && steamDeviceName.Contains("Pro"))
                {
                    currentDevice = SupportOffsetDevice.QuestPro;
                }
                else if (steamDeviceName.Contains("Meta") && steamDeviceName.Contains("Plus"))
                {
                    currentDevice = SupportOffsetDevice.Quest3;
                }
                else if (steamDeviceName.Contains("Vive"))
                {
                    currentDevice = SupportOffsetDevice.Vive;
                }
                else if (steamDeviceName.Contains("Index"))
                {
                    currentDevice = SupportOffsetDevice.Index;
                }
                else if (steamDeviceName.Contains("Pico"))
                {
                    currentDevice = SupportOffsetDevice.Pico;
                }
                else if (steamDeviceName.Contains("MR") || steamDeviceName.Contains("Reverb"))
                {
                    currentDevice = SupportOffsetDevice.MixedReality;
                }
                else if (steamDeviceName.Contains("OpenXR"))
                {
                    currentDevice = SupportOffsetDevice.OpenXR;
                }
                else
                {
                    currentDevice = SupportOffsetDevice.Unknown;
                }

                positionOffset = controllerScriptableObject.GetPositionOffset(currentRuntime, currentDevice);
                rotationOffset = controllerScriptableObject.GetRotationOffset(currentRuntime, currentDevice);
            }
            else if (openXRRuntime.Contains("Oculus"))
            {
                currentRuntime = OpenXRRuntimeType.Oculus;

                if (steamDeviceName.Contains("Oculus"))
                {
                    currentDevice = SupportOffsetDevice.Quest2;
                }
                else if (steamDeviceName.Contains("Meta") && steamDeviceName.Contains("Pro"))
                {
                    currentDevice = SupportOffsetDevice.QuestPro;
                }
                else if (steamDeviceName.Contains("Meta") && steamDeviceName.Contains("Plus"))
                {
                    currentDevice = SupportOffsetDevice.Quest3;
                }

                positionOffset = controllerScriptableObject.GetPositionOffset(OpenXRRuntimeType.Oculus, currentDevice);
                rotationOffset = controllerScriptableObject.GetRotationOffset(OpenXRRuntimeType.Oculus, currentDevice);
            }
            else
            {
                currentRuntime = OpenXRRuntimeType.Other;

                positionOffset = controllerScriptableObject.GetPositionOffset(OpenXRRuntimeType.Oculus, currentDevice);
                rotationOffset = controllerScriptableObject.GetRotationOffset(OpenXRRuntimeType.Oculus, currentDevice);
            }

            if (isDebugConnectedDevice)
            {
                Debug.Log($"Current Device : {steamDeviceName}\nPosition Offset : {positionOffset}\nRotation Offset : {rotationOffset}");
            }

#elif DIVE_PLATFORM_META
            var oculusDeviceName = device.name;
            if (oculusDeviceName.Contains("Oculus"))
            {
                currentDevice = SupportOffsetDevice.Quest2;
            }
            else if (oculusDeviceName.Contains("Meta") && oculusDeviceName.Contains("Pro"))
            {
                currentDevice = SupportOffsetDevice.QuestPro;
            }
            else if (oculusDeviceName.Contains("Meta") && oculusDeviceName.Contains("Plus"))
            {
                currentDevice = SupportOffsetDevice.Quest3;
            }
            else
            {
                currentDevice = SupportOffsetDevice.Unknown;
            }

            positionOffset = controllerScriptableObject.GetPositionOffset(OpenXRRuntimeType.Oculus, currentDevice);
            rotationOffset = controllerScriptableObject.GetRotationOffset(OpenXRRuntimeType.Oculus, currentDevice);

            if (isDebugConnectedDevice)
            {
                Debug.Log($"Current Device : {oculusDeviceName}\nPosition Offset : {positionOffset}\nRotation Offset : {rotationOffset}");
            }
#else
            var etcDeviceName = device.name;
            if (etcDeviceName.Contains("Oculus"))
            {
                currentDevice = SupportOffsetDevice.Quest2;
            }
            else if (etcDeviceName.Contains("Meta") && etcDeviceName.Contains("Pro"))
            {
                currentDevice = SupportOffsetDevice.QuestPro;
            }
            else if (etcDeviceName.Contains("Meta") && etcDeviceName.Contains("Plus"))
            {
                currentDevice = SupportOffsetDevice.Quest3;
            }
            else if (etcDeviceName.Contains("Vive"))
            {
                currentDevice = SupportOffsetDevice.Vive;
            }
            else if (etcDeviceName.Contains("Index"))
            {
                currentDevice = SupportOffsetDevice.Index;
            }
            else if (etcDeviceName.Contains("Pico"))
            {
                currentDevice = SupportOffsetDevice.Pico;
            }
            else if (etcDeviceName.Contains("MR"))
            {
                currentDevice = SupportOffsetDevice.MixedReality;
            }
            else if (etcDeviceName.Contains("OpenXR"))
            {
                currentDevice = SupportOffsetDevice.OpenXR;
            }
            else
            {
                currentDevice = SupportOffsetDevice.Unknown;
            }

            positionOffset = controllerScriptableObject.GetPositionOffset(OpenXRRuntimeType.Other, currentDevice);
            rotationOffset = controllerScriptableObject.GetRotationOffset(OpenXRRuntimeType.Other, currentDevice);

            if (isDebugConnectedDevice)
            {
                Debug.Log($"Current Device : {etcDeviceName}\nPosition Offset : {positionOffset}\nRotation Offset : {rotationOffset}");
            }
#endif
        }

        /// <summary>
        /// 컨트롤러 연결이 끊겨도 이전 위치로 이동
        /// </summary>
        /// <param name="updatePhase"></param>
        /// <param name="controllerState"></param>
        protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
        {
            base.ApplyControllerState(updatePhase, controllerState);
            
            if (controllerState == null)
                return;
            
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic ||
                updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender ||
                updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                transform.localPosition = controllerState.position;
                transform.localRotation = controllerState.rotation;
            }
        }
        
        /// <summary>
        /// 컨트롤러 트래킹 업데이트
        /// </summary>
        /// <param name="controllerState">현재 컨트롤러 상태</param>
        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);
            if (controllerState == null)
                return;

            var posAction = positionReference.action;
            var rotAction = rotationReference.action;
            var hasPositionAction = posAction != null;
            var hasRotationAction = rotAction != null;

            // Update inputTrackingState
            controllerState.inputTrackingState = InputTrackingState.None;
            var inputTrackingStateAction = stateReference.action;

            // Actions without bindings are considered empty and will fallback
            if (inputTrackingStateAction is {bindings: {Count: > 0}})
            {
                controllerState.inputTrackingState = (InputTrackingState)inputTrackingStateAction.ReadValue<int>();
            }
            else
            {
                // Fallback to the device trackingState if m_TrackingStateAction is not valid
                var positionTrackedDevice = hasPositionAction ? posAction.activeControl?.device as TrackedDevice : null;
                var rotationTrackedDevice = hasRotationAction ? rotAction.activeControl?.device as TrackedDevice : null;
                var positionTrackingState = InputTrackingState.None;

                if (positionTrackedDevice != null)
                    positionTrackingState = (InputTrackingState)positionTrackedDevice.trackingState.ReadValue();

                // If the tracking devices are different only the InputTrackingState.Position and InputTrackingState.Position flags will be considered
                if (positionTrackedDevice != rotationTrackedDevice)
                {
                    var rotationTrackingState = InputTrackingState.None;
                    if (rotationTrackedDevice != null)
                        rotationTrackingState = (InputTrackingState)rotationTrackedDevice.trackingState.ReadValue();

                    positionTrackingState &= InputTrackingState.Position;
                    rotationTrackingState &= InputTrackingState.Rotation;
                    controllerState.inputTrackingState = positionTrackingState | rotationTrackingState;
                }
                else
                {
                    controllerState.inputTrackingState = positionTrackingState;
                }
            }

#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

            // Update position
            if (hasPositionAction)
            {
                if ((controllerState.inputTrackingState & InputTrackingState.Position) != 0)
                {
                    var pos = posAction.ReadValue<Vector3>();
                    controllerState.position = pos + positionOffset;

                    prePosition = controllerState.position;
                }
                else
                {
                    controllerState.position = prePosition;
                }
            }

            // Update rotation
            if (hasRotationAction)
            {
                if ((controllerState.inputTrackingState & InputTrackingState.Rotation) != 0)
                {
                    var rot = rotAction.ReadValue<Quaternion>();
                    controllerState.rotation = rot * Quaternion.Euler(rotationOffset);

                    preRotation = controllerState.rotation;
                }
                else
                {
                    controllerState.rotation = preRotation;
                }
            }

#elif DIVE_PLATFORM_PICO
            
            // Update position
            if (hasPositionAction && (controllerState.inputTrackingState & InputTrackingState.Position) != 0)
            {
                var pos = posAction.ReadValue<Vector3>();

                if (pos == Vector3.zero)
                {
                    controllerState.position = prePosition;
                }
                else
                {
                    controllerState.position = pos + positionOffset;
                    prePosition = controllerState.position;
                }
            }

            // Update rotation
            if (hasRotationAction && (controllerState.inputTrackingState & InputTrackingState.Rotation) != 0)
            {
                var rot = rotAction.ReadValue<Quaternion>();
                    
                if(rot == Quaternion.identity)
                {
                    controllerState.rotation = preRotation;
                }
                else
                {
                    controllerState.rotation = rot * Quaternion.Euler(rotationOffset);
                    preRotation = controllerState.rotation;
                }
            }

#endif
        }

        #endregion
    }
}