using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public partial class PXRGrabbable
    {
        #region Private Fields

        /// <summary>
        /// 자신과 자식들의 MeshRenderer 컴포넌트
        /// </summary>
        protected MeshRenderer[] meshes;

        #endregion

        #region Public Properties

        [field: Tooltip("실험적인 기능을 사용할 수 있는 기능 - 현재 DistanceGrab + AttachGrabbableState None 상태에서만 지원")]
        [field: Space, SerializeField, ShowIf(nameof(IsMoveGrabbable), true), LabelText("추가 기능")]
        public bool ExperimentFeature { get; private set; } = false;

        /// <summary>
        /// 오브젝트가 손으로 바로 부착되지 않을 경우에
        /// Axis 컨트롤러로 오브젝트 이동이 가능한 경우 true, 그렇지 않은 경우 false
        /// </summary>
        [field: Tooltip("오브젝트가 손으로 바로 부착되지 않을 경우에 Axis 컨트롤러로 오브젝트 이동이 가능한 경우 true, 그렇지 않은 경우 false")]
        [field: SerializeField, ShowIf(nameof(UseMoveGrabbable), true), LabelText("오브젝트 앞뒤 이동")]
        public bool CanMoving { get; private set; } = false;

        /// <summary>
        /// 오브젝트가 손으로 바로 부착되지 않을 경우에
        /// Axis 컨트롤러로 오브젝트 이동이 가능한 경우
        /// PC Rig를 사용하여 오브젝트 이동을 지원하는 경우 true, 그렇지 않은 경우 false
        /// </summary>
        [field: Tooltip("오브젝트가 손으로 바로 부착되지 않을 경우에 PC Rig를 사용하여 오브젝트 이동을 지원하는 경우 true, 그렇지 않은 경우 false")]
        [field: SerializeField, ShowIf(nameof(UsePCMoveGrabbable), true), LabelText("오브젝트 앞뒤 이동 [PC]")]
        public bool CanPCMoving { get; private set; } = false;

        /// <summary>
        /// Axis 컨트롤러로 오브젝트 회전이 가능한 경우 true, 그렇지 않은 경우 false
        /// </summary>
        [field: Tooltip("오브젝트가 손으로 바로 부착되지 않을 경우에 Axis 컨트롤러로 오브젝트 회전이 가능한 경우 true, 그렇지 않은 경우 false")]
        [field: SerializeField, ShowIf(nameof(UseMoveGrabbable), true), LabelText("오브젝트 좌우 회전")]
        public bool CanRotating { get; private set; } = false;

        /// <summary>
        /// 오브젝트 이동 및 회전 기능 활성화 플래그
        /// </summary>
        public bool IsMoveGrabbable => IsEnableDistanceGrab;

        /// <summary>
        /// 오브젝트 이동 및 회전 [PC] 기능 활성화 플래그
        /// </summary>
        public bool IsPCMoveGrabbable => IsMoveGrabbable && !PXRRig.IsVRPlay;

        /// <summary>
        /// 실험적 기능 + 오브젝트 이동 및 회전 활성화 플래그
        /// </summary>
        public bool UseMoveGrabbable => ExperimentFeature && IsMoveGrabbable;

        /// <summary>
        /// 실험적 기능 + 오브젝트 이동 및 회전 [PC] 활성화 플래그
        /// </summary>
        public bool UsePCMoveGrabbable => ExperimentFeature && IsPCMoveGrabbable;

        #endregion

        #region Public Methods

        /// <summary>
        /// 자식 오브젝트의 메쉬들을 모두 합쳐서 bound값을 구함
        /// enter는 Local 포지션임
        /// </summary>
        /// <returns></returns>
        public Bounds CalculateLocalBounds()
        {
            Bounds mergedBounds = new Bounds(transform.position, Vector3.zero);

            foreach (MeshRenderer mr in meshes)
            {
                mergedBounds.Encapsulate(mr.bounds);
            }

            return mergedBounds;
        }

        #endregion
    }
}