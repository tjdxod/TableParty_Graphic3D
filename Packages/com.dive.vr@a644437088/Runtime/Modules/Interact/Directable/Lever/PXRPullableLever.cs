using System;
using System.Collections;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

// ReSharper disable once CheckNamespace
namespace Dive.VRModule
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class PXRPullableLever : PXRDirectableBase
    {
        #region Public Fields

        public event Action OnEvent;
        public event Action OffEvent;
        public event Action ReturnEvent;
        public event Action<float> ProgressEvent;

        #endregion

        #region Private Fields

        [SerializeField, ShowIf(nameof(IsTwoDirection), true), LabelText("상태 변경 시 강제로 핸드 릴리즈 여부")]
        private bool useForceRelease = true;

        [SerializeField, LabelText("내부 변수")]
        private PXRPullableLeverVariable internalVariable;

        [SerializeField, ReadOnly, LabelText("현재 레버의 방향")]
        private Vector2 axisValue;

        // [SerializeField, LabelText("위아래 방향 반전")]
        // private bool isReverse;
        
        [SerializeField, ReadOnly]
        private float ratio = 1;

        private PXRLeverGrabbable leverGrabbable;

        private IEnumerator routineOnOffMove;
        private IEnumerator routineCenterMove;
        private bool isDeadZoneX;
        private bool isDeadZoneZ;

        private bool isStateChanged;
        private bool isCompletedX;
        private bool isCompletedZ;

        [SerializeField]
        private Vector3 maxLocalPosition;

        [SerializeField]
        private Vector3 minLocalPosition;

        #endregion

        #region Public Properties

        [field: SerializeField, LabelText("레버의 타입"), Space]
        public LeverType LeverType { get; private set; } = LeverType.None;

        [field: SerializeField, ShowIf(nameof(IsTwoDirection), true), LabelText("현재 레버의 상태")]
        public LeverState LeverState { get; private set; } = LeverState.Off;

        public bool IsTwoDirection => LeverType == LeverType.TwoDirection;

        #endregion

        #region Private Properties

        private Vector3 CenterLeverLocalPosition => StaticCenterTransform.localPosition + Vector3.up * LeverLength;
        private Vector3 CenterLeverWorldPosition => LocalToWorldPosition(CenterLeverLocalPosition, transform);
        private Vector3 GrabbableReturnPosition => LocalToWorldPosition(CenterLeverLocalPosition, JointTransform);
        private Transform JointTransform => internalVariable.JointTransform;
        private Transform AxisTransform => internalVariable.AxisTransform;
        private Transform StaticCenterTransform => internalVariable.StaticCenterTransform;
        private Vector2 FrameSize => internalVariable.FrameSize;
        private float LeverLength => internalVariable.LeverLength;
        private float StateChangeThreshold => internalVariable.StateChangeThreshold;
        private float DeadZoneRatio => internalVariable.DeadZoneRatio;
        private int Angle => internalVariable.Angle;

        private float PreGrabAxis
        {
            get => internalVariable.PreGrabAxis;
            set => internalVariable.PreGrabAxis = value;
        }

        #endregion

        #region Public Methods

        public void ReturnToOrigin(bool useEvent = true)
        {
            if (IsTwoDirection)
            {
                if (useForceRelease)
                    routineOnOffMove = CoroutineOnOffMove(PreGrabAxis > 0 ? 1 : -1, useEvent);
                else
                    routineOnOffMove = isStateChanged ? CoroutineOnOffMove(PreGrabAxis > 0 ? -1 : 1, useEvent) : CoroutineOnOffMove(PreGrabAxis < 0 ? -1 : 1, useEvent);

                StartCoroutine(routineOnOffMove);
            }
            else
            {
                routineCenterMove = CoroutineCenterMove();
                StartCoroutine(routineCenterMove);
            }
        }

        public void ForceStateChange(LeverState state)
        {
            if (!IsTwoDirection)
                return;

            if (state == LeverState)
                return;

            LeverState = state;
            routineOnOffMove = CoroutineOnOffMove(state == LeverState.On ? 1 : -1, false, false);
            StartCoroutine(routineOnOffMove);
        }

        public override void ForceRelease(bool useEvent = true)
        {
        }

        public override void ForcePress(bool useEvent = true)
        {
        }

        #endregion

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();

            leverGrabbable = GetComponentInChildren<PXRLeverGrabbable>(true);

            var max = RotateAround(StaticCenterTransform.position, CenterLeverWorldPosition, StaticCenterTransform.rotation, JointTransform.forward, Angle);
            var min = RotateAround(StaticCenterTransform.position, CenterLeverWorldPosition, StaticCenterTransform.rotation, JointTransform.forward, -Angle);
            
            // world to local
            maxLocalPosition = transform.InverseTransformPoint(max.Item1); 
            minLocalPosition = transform.InverseTransformPoint(min.Item1);
            
            if (!IsTwoDirection)
                return;

            PreGrabAxis = LeverState == LeverState.On ? 1 - StateChangeThreshold : -1 + StateChangeThreshold;
            
            routineOnOffMove = CoroutineOnOffMove(LeverState == LeverState.On ? 1 : -1, true);
            StartCoroutine(routineOnOffMove);
        }

        /// <summary>
        /// 정해진 시간마다 레버의 위치를 갱신
        /// </summary>
        /// <example>
        /// 상속하여 사용할때는 다음과 같이 override하여 변경하여야 함.
        /// <code>
        /// protected override void FixedUpdate()
        /// {
        ///     if (!IsEnableLever())
        ///         return;
        /// 
        ///     if (photonView.IsMine)
        ///     {
        ///         axisValue = GetAxisValue();
        ///     }
        ///     else
        ///     {
        ///         axisValue = axisValue;
        ///     }
        ///
        ///     ratio = axisValue.x;
        ///     OnProgress(axisValue);
        /// }
        /// </code>
        /// </example>
        protected virtual void FixedUpdate()
        {
            if (!IsEnableLever())
                return;

            axisValue = GetAxisValue();
            ratio = axisValue.x;
            OnProgress(axisValue);
        }

        protected void OnProgress(Vector2 axis)
        {
            if (routineOnOffMove != null || routineCenterMove != null)
                return;
            
            var targetRotation = Quaternion.identity;

            switch (LeverType)
            {
                case LeverType.TwoDirection:

                    if (useForceRelease)
                    {
                        var stateChange = PreGrabAxis >= 0 ? PreGrabAxis * -1 > axis.x : PreGrabAxis * -1 < axis.x;
                        var targetAxis = axis.x;

                        if ((isCompletedX || isCompletedZ) && stateChange)
                        {
                            isStateChanged = true;
                            
                            LeverState = targetAxis > 0 ? LeverState.On : LeverState.Off;
                            
                            PreGrabAxis = LeverState == LeverState.On ? 1 - StateChangeThreshold : -1 + StateChangeThreshold;

                            leverGrabbable.Grabber?.ForceRelease();
                            return;
                        }
                    }
                    else
                    {
                        if (isCompletedX)
                            isStateChanged = axis.x * PreGrabAxis < 0;
                        else
                            isStateChanged = false;
                    }

                    var angle = Mathf.Lerp(-Angle, Angle, (axis.x + 1) / 2);
                    
                    targetRotation = transform.rotation * Quaternion.Euler(0, 0, angle);
                    
                    break;
                case LeverType.FourDirection:
                    // if (isDeadZoneX)
                    //     ratioPosition = isDeadZoneY ? Vector3.zero : movePositionZ;
                    // else
                    //     ratioPosition = isDeadZoneY ? movePositionX : movePositionX + movePositionZ;
                    //
                    // localToWorld = LocalToWorldPosition(CenterLeverLocalPosition + ratioPosition, transform);
                    // targetRotation = Quaternion.FromToRotation(Vector3.up, localToWorld - centerPosition);

                    break;
                case LeverType.None:
                default:
                    return;
            }

            JointTransform.rotation = Quaternion.Lerp(JointTransform.rotation, targetRotation, Time.deltaTime * 10f);

            if (!leverGrabbable.IsGrabbed)
                leverGrabbable.transform.position = GrabbableReturnPosition;
                
            ProgressEvent?.Invoke(ratio);
        }

        private IEnumerator CoroutineOnOffMove(float axis, bool isAwake = false, bool useEvent = true)
        {
            if (useEvent)
            {
                if (useForceRelease)
                {
                    if (!isAwake)
                    {
                        if (isStateChanged)
                        {
                            isStateChanged = false;

                            if (LeverState == LeverState.On)
                                OnEvent?.Invoke();
                            else
                                OffEvent?.Invoke();
                        }
                        else
                        {
                            ReturnEvent?.Invoke();
                        }
                    }
                }
                else
                {
                    if (!isAwake)
                    {
                        if (isStateChanged)
                        {
                            isStateChanged = false;

                            LeverState = LeverState == LeverState.On ? LeverState.Off : LeverState.On;
                            PreGrabAxis *= -1;

                            if (LeverState == LeverState.On)
                                OnEvent?.Invoke();
                            else
                                OffEvent?.Invoke();
                        }
                        else
                        {
                            ReturnEvent?.Invoke();
                        }
                    }
                }
            }


            var time = 0.5f;

            var startRotation = JointTransform.rotation;
            var lookRotation = transform.rotation * Quaternion.Euler(0, 0, LeverState == LeverState.On ? Angle : -Angle);
            
            while (time > 0)
            {
                time -= Time.deltaTime;
                JointTransform.rotation = Quaternion.Lerp(startRotation, lookRotation, 1 - (time / 0.5f));
                yield return null;
            }

            JointTransform.rotation = lookRotation;
            leverGrabbable.transform.position = GrabbableReturnPosition;

            routineOnOffMove = null;
        }

        private IEnumerator CoroutineCenterMove()
        {
            ReturnEvent?.Invoke();

            var time = 0.5f;

            while (time > 0)
            {
                time -= Time.deltaTime;
                var local = LocalToWorldPosition(CenterLeverLocalPosition, transform);
                var fromToRotation = Quaternion.FromToRotation(Vector3.up, local - StaticCenterTransform.position);
                JointTransform.rotation = Quaternion.Lerp(JointTransform.rotation, fromToRotation, 1 - (time / 0.5f));
                yield return null;
            }

            {
                var local = LocalToWorldPosition(CenterLeverLocalPosition, transform);
                var fromToRotation = Quaternion.FromToRotation(Vector3.up, local - StaticCenterTransform.position);
                JointTransform.rotation = fromToRotation;
            }

            leverGrabbable.transform.position = GrabbableReturnPosition;

            routineCenterMove = null;
        }

        protected virtual bool IsEnableLever()
        {
            if (!CanInteract)
                return false;

            if (LeverType == LeverType.None)
                return false;

            return true;
        }
        
        protected Vector2 GetAxisValue()
        {
            if (!CheckFrontAndBack())
                return axisValue;

            var aTob = CenterLeverWorldPosition - StaticCenterTransform.position;
            var bToD = leverGrabbable.transform.position - CenterLeverWorldPosition;
            
            AxisTransform.position = CenterLeverWorldPosition + Vector3.Project(bToD, Vector3.ProjectOnPlane(bToD, aTob));
            
            var standardMaxX = StaticCenterTransform.localPosition.x - FrameSize.x;
            var standardMinX = StaticCenterTransform.localPosition.x + FrameSize.x;
            var standardMaxZ = StaticCenterTransform.localPosition.z + FrameSize.y;
            var standardMinZ = StaticCenterTransform.localPosition.z - FrameSize.y;
            
            var ratioX = (AxisTransform.localPosition.x - standardMinX) / (standardMaxX - standardMinX) * 2 - 1;
            var ratioZ = (AxisTransform.localPosition.z - standardMinZ) / (standardMaxZ - standardMinZ) * 2 - 1;
            
            isDeadZoneX = Mathf.Abs(ratioX) < DeadZoneRatio;
            isDeadZoneZ = Mathf.Abs(ratioZ) < DeadZoneRatio;

            isCompletedX = Mathf.Abs(ratioX) > StateChangeThreshold;
            isCompletedZ = Mathf.Abs(ratioZ) > StateChangeThreshold;
            
            if (isDeadZoneX && isDeadZoneZ)
            {
                ratioX = 0;
                ratioZ = 0;
            }
            else
            {
                if (Mathf.Abs(ratioX) < 0.001f)
                    ratioX = 0;

                if (Mathf.Abs(ratioZ) < 0.001f)
                    ratioZ = 0;
            }

            ratioX = Mathf.Clamp(ratioX, -1, 1);
            ratioZ = Mathf.Clamp(ratioZ, -1, 1);
            
            return new Vector2(ratioX, ratioZ);
        }

        private bool CheckFrontAndBack()
        {
            var nearA = LocalToWorldPosition(StaticCenterTransform.localPosition + Vector3.up * 0.01f, transform);

            var aToNearA = nearA - StaticCenterTransform.position;
            var bTod = leverGrabbable.transform.position - nearA;

            var dot = Vector3.Dot(aToNearA, bTod);

            return dot > 0;
        }

        public static (Vector3, Quaternion) RotateAround(Vector3 point, Vector3 startPos, Quaternion startRot, Vector3 axis, float angle)
        {
            var rot = Quaternion.AngleAxis(angle, axis);
            var dir = startPos - point;
            dir = rot * dir;

            (Vector3, Quaternion) posRot;
            posRot.Item1 = point + dir;
            posRot.Item2 = startRot * Quaternion.Inverse(startRot) * rot * startRot;

            return posRot;
        }

        #endregion
    }
}