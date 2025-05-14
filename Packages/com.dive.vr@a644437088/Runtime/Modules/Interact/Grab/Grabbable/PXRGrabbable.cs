using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    /// <summary>
    /// 그랩이 가능한 오브젝트
    /// </summary>
    public partial class PXRGrabbable : PXRInteractableBase, IGrabbable
    {
        #region Private Fields

        /// <summary>
        /// Rigidbody의 한 프레임 전 Position 
        /// </summary>
        protected Vector3 preFramePosition = Vector3.zero;

        /// <summary>
        /// Transform의 한 프레임 전 Rotation
        /// </summary>
        protected Quaternion preFrameRotation = Quaternion.identity;

        /// <summary>
        /// 자신과 자식 오브젝트들의 IGrabbable 리스트
        /// </summary>
        protected readonly List<IGrabbable> grabbableList = new List<IGrabbable>();

        /// <summary>
        /// 오브젝트를 잡고 있는 손의 방향
        /// </summary>
        protected HandSide currentHandSide;

        #endregion

        #region Public Properties

        [field: Tooltip("Release 이벤트 실행 후 초기 위치로 복귀 시 true, 그렇지 않은 경우 false")]
        [field: SerializeField, Header("복귀 이벤트"), LabelText("초기 위치로 복귀")]
        public bool UseReturnToOrigin { get; private set; } = false;

        [field: Tooltip("움직임이 없는 경우에 복귀 시간을 계산하는 경우 true, 그렇지 않은 경우 false")]
        [field: SerializeField, ShowIf(nameof(UseReturnToOrigin), true), LabelText("움직임이 없을 때부터 시간 계산")]
        public bool IsCalculateTimeFromNoMovement { get; private set; } = false;

        [field: Tooltip("복귀 시간")]
        [field: SerializeField, ShowIf(nameof(UseReturnToOrigin), true), LabelText("복귀 시간")]
        public float ReturnToOriginTime { get; private set; } = 0.5f;

        [field: Tooltip("Y값에 따른 강제 복귀 실행")]
        [field: SerializeField, ShowIf(nameof(UseReturnToOrigin), true), LabelText("Y값에 따른 강제 복귀 실행")]
        public bool IsReturnToOriginY { get; private set; } = false;

        [field: Tooltip("강제 복귀를 위한 Y값 범위")]
        [field: SerializeField, ShowIf(nameof(IsEnableForceReturn), true), LabelText("강제 복귀를 위한 Y값 범위")]
        public Vector2 ReturnToOriginYRange { get; private set; } = new Vector2(-0.5f, 50f);

        /// <summary>
        /// 그랩이 가능한 조작 방법
        /// </summary>
        [field: Tooltip("그랩이 가능한 조작 방법을 선택 - Collider는 Collider를 통해 그랩, Distance는 거리를 통해 그랩, Both는 둘 다 가능")]
        [field: SerializeField, Header("그랩"), LabelText("그랩 가능 상태")]
        public EnableGrabState EnableGrabState { get; private set; } = EnableGrabState.Collider;

        /// <summary>
        /// Release 시 오브젝트의 Parent 설정 방법
        /// </summary>
        [field: Tooltip("Release 시 오브젝트가 복귀하는 Parent 오브젝트를 설정")]
        [field: SerializeField, ShowIf(nameof(IsEnableGrab), true), LabelText("부모 오브젝트 설정")]
        public OverrideParentState OverrideParentState { get; private set; } = OverrideParentState.Origin;

        /// <summary>
        ///  Release 시 Parent로 하고자하는 오브젝트를 할당
        /// </summary>
        [Tooltip("Release 시 Parent로 하고자하는 오브젝트를 할당")]
        [SerializeField, ShowIf(nameof(OverrideParentState), OverrideParentState.Other), LabelText("Parent 오브젝트")]
        protected Transform parentOverride;

        /// <summary>
        /// 그랩 후 부착 상태를 선택
        /// </summary>
        [field: Tooltip("그랩 후 부착 상태를 선택 - None은 부착하지 않음, Attached는 그랩 후 부착, AttachedWithPose는 그랩 후 부착하고 지정된 위치와 회전으로 변경")]
        [field: SerializeField, LabelText("그랩 부착 상태"), ShowIf(nameof(IsEnableGrab), true), Space]
        public AttachGrabbableState AttachGrabbableState { get; private set; } = AttachGrabbableState.None;

        /// <summary>
        /// 그랩 후 부착 상태가 None이 아닌 경우 지정된 위치와 회전으로 변경
        /// </summary>
        [field: Tooltip("그랩 후 부착 상태가 None이 아닌 경우 지정된 위치와 회전으로 변경")]
        [field: SerializeField, LabelText("지정 위치 이동 및 회전"), ShowIf(nameof(IsAttachGrabbable), true)]
        public OverrideTransformState OverrideTransformState { get; private set; } = OverrideTransformState.None;

        /// <summary>
        /// 오브젝트를 잡았을 때 정해진 위치로 이동하는 경우에 지정된 이동 위치
        /// </summary>
        [Tooltip("오브젝트를 잡았을 때 정해진 위치로 이동하는 경우에 지정된 이동 위치(Native)")]
        [field: SerializeField, ShowIf(nameof(IsPositionOverride), true), LabelText("이동 위치 [Native]")]
        public Vector3 OverrideLocalPosition { get; set; } = Vector3.zero;

        /// <summary>
        /// 오브젝트를 잡았을 때 정해진 회전값으로 변경하는 경우에 지정된 변경 회전 값
        /// </summary>
        [Tooltip("오브젝트를 잡았을 때 정해진 회전값으로 변경하는 경우에 지정된 변경 회전 값 (Native)")]
        [field: SerializeField, ShowIf(nameof(IsRotationOverride), true), LabelText("회전 값 [Native]")]
        public Vector3 OverrideLocalRotation { get; set; } = Vector3.zero;

        [Tooltip("오브젝트를 잡았을 때 정해진 위치로 이동하는 경우에 지정된 이동 위치 (Meta)")]
        [field: SerializeField, ShowIf(nameof(IsPositionOverride), true), LabelText("이동 위치 [메타]")]
        public Vector3 MetaOverrideLocalPosition { get; set; } = Vector3.zero;

        [Tooltip("오브젝트를 잡았을 때 정해진 회전값으로 변경하는 경우에 지정된 변경 회전 값 (Meta)")]
        [field: SerializeField, ShowIf(nameof(IsRotationOverride), true), LabelText("회전 값 [메타]")]
        public Vector3 MetaOverrideLocalRotation { get; set; } = Vector3.zero;

        [Tooltip("오브젝트를 잡았을 때 정해진 위치로 이동하는 경우에 지정된 이동 위치 (Pico)")]
        [field: SerializeField, ShowIf(nameof(IsPositionOverride), true), LabelText("이동 위치 [피코]")]
        public Vector3 PicoOverrideLocalPosition { get; set; } = Vector3.zero;

        [Tooltip("오브젝트를 잡았을 때 정해진 회전값으로 변경하는 경우에 지정된 변경 회전 값 (Pico)")]
        [field: SerializeField, ShowIf(nameof(IsRotationOverride), true), LabelText("회전 값 [피코]")]
        public Vector3 PicoOverrideLocalRotation { get; set; } = Vector3.zero;


        /// <summary>
        /// 물건을 잡았을 때 메타 / 피코 손 모델의 포즈
        /// </summary>
        [field: SerializeField, ShowIf(nameof(IsEnableGrab), true), LabelText("손 모델의 포즈"), Space]
        public PXRPose GrabbedPXRPose { get; set; }

        /// <summary>
        /// 물건을 잡았을 때 손 모델의 포즈
        /// </summary>
        [field: SerializeField, ShowIf(nameof(IsEnableGrab), true), LabelText("손 모델의 포즈"), Space]
        public BNG.HandPose GrabbedHandPose { get; private set; }

        [field: SerializeField, LabelText("페이크 핸드 사용 유무")]
        public bool UseFakeHand { get; private set; } = false;

        [field: SerializeField, ShowIf(nameof(UseFakeHand), true), LabelText("페이크 핸드")]
        public GameObject FakeHand { get; set; }

        /// <summary>
        /// 물건을 잡고있는 손 방향의 그랩 관리 클래스
        /// </summary>
        public PXRGrabber Grabber { get; private set; } = null;

        /// <summary>
        /// 기존에 위치하던 오브젝트의 Parent 자식이 아닌 다른 Parent의 자식으로 변경하고자 할때 할당
        /// </summary>
        public Transform ParentOverride
        {
            get
            {
                if (OverrideParentState == OverrideParentState.Null)
                    return null;

                return !parentOverride ? transform.parent : parentOverride;
            }
            set => parentOverride = OverrideParentState == OverrideParentState.Null ? null : value;
        }

        /// <summary>
        /// Transform의 한 프레임 전 Position
        /// </summary>
        public Vector3 PreFramePosition => preFramePosition;

        /// <summary>
        /// Transform의 한 프레임 전 Rotation
        /// </summary>
        public Quaternion PreFrameRotation => preFrameRotation;

        /// <summary>
        /// 오브젝트를 잡고 있는 손의 방향
        /// </summary>
        public HandSide CurrentHandSide => currentHandSide;

        /// <summary>
        /// 오브젝트가 잡힌 상태인 경우 true, 그렇지 않은 경우 false
        /// </summary>
        // ReSharper disable once RedundantDefaultMemberInitializer
        public bool IsGrabbed { get; private set; } = false;

        /// <summary>
        /// Collider 그랩 가능 여부
        /// </summary>
        public bool IsEnableColliderGrab => EnableGrabState is EnableGrabState.Collider or EnableGrabState.Both;

        /// <summary>
        /// Distance 그랩 가능 여부
        /// </summary>
        public bool IsEnableDistanceGrab => EnableGrabState is EnableGrabState.Distance or EnableGrabState.Both;

        /// <summary>
        /// AttachGrabbable 가능 여부
        /// </summary>
        public bool IsAttachGrabbable => IsEnableGrab && AttachGrabbableState is AttachGrabbableState.Attached or AttachGrabbableState.AttachedWithPose;

        /// <summary>
        /// 위치 값 덮어쓰기 가능 여부
        /// </summary>
        public bool IsPositionOverride => IsAttachGrabbable && OverrideTransformState is OverrideTransformState.Both or OverrideTransformState.Position;

        /// <summary>
        /// 회전 값 덮어쓰기 가능 여부
        /// </summary>
        public bool IsRotationOverride => IsAttachGrabbable && OverrideTransformState is OverrideTransformState.Both or OverrideTransformState.Rotation;

        /// <summary>
        /// Y값 범위에 따른 강제 복귀 여부
        /// </summary>
        public bool IsEnableForceReturn => UseReturnToOrigin && IsReturnToOriginY;

        public bool IsHandPose => GrabbedHandPose != null && GrabbedPXRPose != null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Grabbable의 위치와 회전을 변경
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ChangePosAndRot()
        {
            var parentHandSide = transform.parent.GetComponent<PXRGrabber>().HandSide;

            if (OverrideTransformState is OverrideTransformState.Position or OverrideTransformState.Both)
            {
                ChangeLocalPosition(parentHandSide);
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }

            if (!PXRRig.IsVRPlay)
                return;

            if (OverrideTransformState is OverrideTransformState.Rotation or OverrideTransformState.Both)
            {
                ChangeLocalRotation(parentHandSide);
            }
        }

        /// <summary>
        /// Grabbable을 반환
        /// </summary>
        /// <returns>Grabbable 클래스</returns>
        public PXRGrabbable GetGrabbable()
        {
            return this;
        }

        /// <summary>
        /// 이전 프레임과 거리를 비교하여 반환
        /// </summary>
        /// <returns>거리</returns>
        public float GetDistanceAtFrame()
        {
            return Vector3.Distance(preFramePosition, transform.position);
        }

        /// <summary>
        /// 이전 프레임과의 각도 반환
        /// </summary>
        /// <returns>각도</returns>
        public float GetAngleAtFrame()
        {
            return Quaternion.Angle(preFrameRotation, transform.rotation);
        }

        /// <summary>
        /// 이전 프레임과의 방향 반환
        /// </summary>
        /// <returns>방향</returns>
        public Vector3 GetDirectionAtFrame()
        {
            return transform.position - preFramePosition;
        }

        /// <summary>
        /// GrabbableChild 리스트에 추가
        /// </summary>
        /// <param name="grabbable"></param>
        public void AddChildGrabbable(IGrabbable grabbable)
        {
            if (!grabbableList.Contains(grabbable))
            {
                grabbableList.Add(grabbable);
            }
        }

        /// <summary>
        /// AttachGrabbableState를 None으로 변경
        /// </summary>
        public void DisableCanImmediatelyAttached()
        {
            AttachGrabbableState = AttachGrabbableState.None;
        }

        public void ForceSetPXRPose(PXRPose pose)
        {
            GrabbedPXRPose = pose;
        }
        
        public void ForceSetHandPose(BNG.HandPose pose)
        {
            GrabbedHandPose = pose;
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// 변수 초기화
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Rigid = GetComponent<Rigidbody>();
            colliderArray = GetComponentsInChildren<Collider>();
            notTriggerList = GetComponentsInChildren<Collider>().Where(x => x.isTrigger == false).ToList();
            meshes = GetComponentsInChildren<MeshRenderer>();
        }

        /// <summary>
        /// 변수 초기화
        /// </summary>
        protected override void Start()
        {
            base.Start();

            grabbableList.Add(this);
        }

        /// <summary>
        /// 컨트롤러에 맞게 Local위치값 변경
        /// </summary>
        /// <param name="handSide">컨트롤러</param>
        private void ChangeLocalPosition(HandSide handSide)
        {
            var targetOverrideLocalPosition = Vector3.zero;

#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

            targetOverrideLocalPosition = MetaOverrideLocalPosition;

#elif DIVE_PLATFORM_PICO
            targetOverrideLocalPosition = PicoOverrideLocalPosition;

#else
            targetOverrideLocalPosition = OverrideLocalPosition;

#endif

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (handSide == HandSide.Right)
            {
                if (Grabber != null && Grabber.IsNeedChangeTweezerPosition)
                {
                    var origin = PXRGrabber.GetGrabberPosition(Grabber.HandSide);
                    var tweezer = PXRGrabber.GetGrabberTweezerPosition(Grabber.HandSide);

                    var compareX = origin.x - tweezer.x;
                    var compareY = origin.y - tweezer.y;
                    var compareZ = origin.z - tweezer.z;

                    transform.localPosition = targetOverrideLocalPosition - new Vector3(compareX, compareY, compareZ);
                }
                else
                {
                    transform.localPosition = targetOverrideLocalPosition;
                }
            }
            else
            {
                var targetPosition = new Vector3(-targetOverrideLocalPosition.x, targetOverrideLocalPosition.y, targetOverrideLocalPosition.z);

                if (Grabber != null && Grabber.IsNeedChangeTweezerPosition)
                {
                    var origin = PXRGrabber.GetGrabberPosition(Grabber.HandSide);
                    var tweezer = PXRGrabber.GetGrabberTweezerPosition(Grabber.HandSide);

                    var compareX = origin.x - tweezer.x;
                    var compareY = origin.y - tweezer.y;
                    var compareZ = origin.z - tweezer.z;

                    transform.localPosition = targetPosition - new Vector3(compareX, compareY, compareZ);
                }
                else
                {
                    transform.localPosition = targetPosition;
                }
            }
        }

        /// <summary>
        /// 컨트롤러에 맞게 Local회전값 변경
        /// </summary>
        /// <param name="handSide">컨트롤러</param>
        private void ChangeLocalRotation(HandSide handSide)
        {
            var targetOverrideLocalRotation = Vector3.zero;

#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

            targetOverrideLocalRotation = MetaOverrideLocalRotation;

#elif DIVE_PLATFORM_PICO
            targetOverrideLocalRotation = PicoOverrideLocalRotation;

#else
            targetOverrideLocalRotation = OverrideLocalRotation;

#endif

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (handSide == HandSide.Right)
            {
                transform.localRotation = Quaternion.Euler(targetOverrideLocalRotation);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(new Vector3(targetOverrideLocalRotation.x, -targetOverrideLocalRotation.y, -targetOverrideLocalRotation.z));
            }
        }

        #endregion
    }
}