using System;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public class PXRTurnableValve : PXRDirectableBase
    {
        #region Public Fields

#pragma warning disable CS0067 // Event is never used
        // ReSharper disable once EventNeverSubscribedTo.Global
        public event Action CompletedClosedEvent;

        // ReSharper disable once EventNeverSubscribedTo.Global
        public event Action CompletedOpenedEvent;
#pragma warning restore CS0067 // Event is never used
        public event Action<float> ProgressEvent;

        #endregion

        #region Private Fields

        [SerializeField, ReadOnly, Space, LabelText("돌려진 정도 (0~1)")]
        // ReSharper disable once NotAccessedField.Local
        private float ratio = 0;

        [SerializeField, LabelText("밸브 최소 각도")]
        private float minAngle = 0;

        [SerializeField, LabelText("밸브 최대 각도")]
        private float maxAngle = 360 * 3;

        [SerializeField]
        private float rotationSpeed = 360;

        [SerializeField]
        private PXRValveVariable internalVariable;

        private readonly Dictionary<int, PXRValveGrabbable> valveGrabbables = new Dictionary<int, PXRValveGrabbable>();

        private readonly Dictionary<HandSide, PXRValveGrabbable> grabbedValveGrabbables = new Dictionary<HandSide, PXRValveGrabbable>();

        private Vector3 rotatePosition;
        // ReSharper disable once NotAccessedField.Local
        private float previousTargetAngle;
        private float smoothedAngle;
        private float targetAngle;

        #endregion

        #region Public Properties

        public PXRValveVariable InternalVariable => internalVariable;
        public Vector3 PreviousLeftPosition { get; set; }
        public Vector3 PreviousRightPosition { get; set; }
        public int GrabbedCount => grabbedValveGrabbables.Count;
        public float Angle => Mathf.Clamp(smoothedAngle, minAngle, maxAngle);

        public Transform FakeHandParent => internalVariable.FakeHandParent;
        public GameObject LeftFakeHand => internalVariable.LeftFakeHand;
        public GameObject RightFakeHand => internalVariable.RightFakeHand;
        public Transform HeadTransform => internalVariable.HeadTransform;

        #endregion

        #region Public Methods

        public PXRValveGrabbable GetValveGrabbable(int index)
        {
            return valveGrabbables.TryGetValue(index, out var grabbable) ? grabbable : null;
        }

        public PXRValveGrabbable GetValveGrabbable(PXRValveGrabbable grabbable)
        {
            var index = grabbable.Index;
            return valveGrabbables.TryGetValue(index, out var valveGrabbable) ? valveGrabbable : null;
        }

        public PXRValveGrabbable GetOtherValveGrabbable(int index)
        {
            foreach (var grabbable in valveGrabbables.Values)
            {
                var otherIndex = grabbable.Index;
                var compare = Mathf.Abs(otherIndex - index);

                if (compare == 3)
                    return grabbable;
            }

            return null;
        }

        public PXRValveGrabbable GetOtherValveGrabbable(PXRValveGrabbable grabbable)
        {
            var index = grabbable.Index;
            foreach (var valveGrabbable in valveGrabbables.Values)
            {
                var otherIndex = valveGrabbable.Index;
                var compare = Mathf.Abs(otherIndex - index);

                if (compare == 3)
                    return valveGrabbable;
            }

            return null;
        }

        public void SetCanInteract(int index)
        {
            for (var i = 1; i <= 6; i++)
                valveGrabbables[i].TransferState = TransferState.None;

            switch (index)
            {
                case 1:
                    valveGrabbables[4].TransferState = TransferState.Both;
                    break;
                case 4:
                    valveGrabbables[1].TransferState = TransferState.Both;
                    break;
                case 2:
                    valveGrabbables[5].TransferState = TransferState.Both;
                    break;
                case 5:
                    valveGrabbables[2].TransferState = TransferState.Both;
                    break;
                case 3:
                    valveGrabbables[6].TransferState = TransferState.Both;
                    break;
                case 6:
                    valveGrabbables[3].TransferState = TransferState.Both;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
        }

        public void ResetCanInteract()
        {
            for (var i = 1; i <= 6; i++)
                valveGrabbables[i].TransferState = TransferState.Both;
        }

        public void ReturnToOrigin(PXRValveGrabbable grabbable)
        {
            var tr = grabbable.transform;
            tr.localPosition = grabbable.OriginLocalPosition;
            tr.localRotation = grabbable.OriginLocalRotation;
        }

        public void SetHandAngle(int index)
        {
            switch (index)
            {
                case 1:
                case 4:
                    FakeHandParent.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 2:
                case 5:
                    FakeHandParent.localRotation = Quaternion.Euler(0, 60, 0);
                    break;
                case 3:
                case 6:
                    FakeHandParent.localRotation = Quaternion.Euler(0, -60, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
        }

        public bool AddGrabbable(PXRGrabber grabber, int index)
        {
            var handSide = grabber.HandSide;

            if (grabbedValveGrabbables.ContainsKey(handSide))
                return false;

            if (!valveGrabbables.ContainsKey(index))
                return false;

            var grabbable = GetValveGrabbable(index);
            grabbedValveGrabbables.Add(handSide, grabbable);
            return true;
        }

        public bool RemoveGrabbable(PXRGrabber grabber, int index)
        {
            var handSide = grabber.HandSide;

            if (!grabbedValveGrabbables.ContainsKey(handSide))
                return false;

            grabbedValveGrabbables.Remove(handSide);
            return true;
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

            var grabbables = GetComponentsInChildren<PXRValveGrabbable>();

            foreach (var grabbable in grabbables)
            {
                if (valveGrabbables.ContainsKey(grabbable.Index))
                    continue;

                valveGrabbables.Add(grabbable.Index, grabbable);
            }
        }

        /// <summary>
        /// 정해진 시간마다 밸브의 위치를 갱신
        /// </summary>
        /// <example>
        /// 상속하여 사용할때는 다음과 같이 override하여 변경하여야 함.
        /// smoothedAngle를 동기화
        /// <code>
        /// protected override void FixedUpdate()
        /// {
        ///     if (!IsEnableTurn())
        ///         return;
        /// 
        ///     if (photonView.IsMine)
        ///     {
        ///         UpdateAngleCalculate();
        ///     }
        ///
        ///     OnApply(Angle);
        ///
        ///     ratio = SetRatio();
        ///     OnProgress(axisValue);
        ///
        ///     previousTargetAngle = targetAngle;
        /// }
        /// </code>
        /// </example>
        protected virtual void FixedUpdate()
        {
            if (!IsEnableTurn())
                return;

            UpdateAngleCalculate();
            
            OnApply(Angle);

            ratio = SetRatio();
            OnProgress(ratio);

            previousTargetAngle = targetAngle;
        }

        private void OnApply(float angle)
        {
            HeadTransform.localRotation = Quaternion.Lerp(HeadTransform.localRotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * 10f);
        }

        private float GetRelativeAngle(Vector3 position1, Vector3 position2)
        {
            if (Vector3.Cross(position1, position2).y < 0)
            {
                return -Vector3.Angle(position1, position2);
            }

            return Vector3.Angle(position1, position2);
        }

        private void UpdateAngleCalculate()
        {
            var angleAdjustment = 0f;

            var leftGrabbable = grabbedValveGrabbables[HandSide.Left];
            var rightGrabbable = grabbedValveGrabbables[HandSide.Right];

            rotatePosition = WorldToLocalPosition(leftGrabbable.transform.position, HeadTransform);
            rotatePosition.y = 0;

            var angle = GetRelativeAngle(rotatePosition, PreviousLeftPosition);

            angleAdjustment += angle;
            PreviousLeftPosition = rotatePosition;

            rotatePosition = WorldToLocalPosition(rightGrabbable.transform.position, HeadTransform);
            rotatePosition.y = 0;

            angle = GetRelativeAngle(rotatePosition, PreviousRightPosition);

            angleAdjustment += angle;
            PreviousRightPosition = rotatePosition;

            angleAdjustment *= 0.5f;

            targetAngle -= angleAdjustment;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (rotationSpeed == 0)
                smoothedAngle = targetAngle;
            else
                smoothedAngle = Mathf.Lerp(smoothedAngle, targetAngle, Time.deltaTime * rotationSpeed);

            if (minAngle == 0 || maxAngle == 0) 
                return;
            
            targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);
            smoothedAngle = Mathf.Clamp(smoothedAngle, minAngle, maxAngle);
        }

        private float SetRatio()
        {
            // minAngle ~ maxAngle와 Angle 사이의 비율을 구한다.
            var distance = Mathf.Abs(maxAngle - minAngle);
            var angleRatio = Mathf.Abs(Angle - minAngle) / distance;

            return angleRatio;
        }

        private void OnProgress(float r)
        {
            ProgressEvent?.Invoke(r);
        }

        private bool IsEnableTurn()
        {
            if (!CanInteract)
                return false;

            if (valveGrabbables.Count != 6)
                return false;

            if (grabbedValveGrabbables is not {Count: 2})
                return false;

            return true;
        }

        #endregion
    }
}