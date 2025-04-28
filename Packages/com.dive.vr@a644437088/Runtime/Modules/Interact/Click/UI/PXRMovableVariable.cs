using System;
using System.Threading;
using DG.Tweening;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    [Serializable]
    public class ColliderInfo
    {
        [SerializeField]
        private Transform trans;

        public Collider[] Colliders;

        public ColliderInfo(Transform transform)
        {
            trans = transform;
            Colliders = new Collider[1];
        }

        public Transform Transform => trans;
        
        public Vector3 Position => trans.position;

        public Vector3 Forward => trans.forward;

        public Vector3 Backward => trans.forward * -1;
    }

    public partial class PXRMovableUI
    {
        [Header("Target"), SerializeField]
        private PXRMovableStaticUI staticUI;
        
        [SerializeField]
        private Transform childCanvas;

        [SerializeField]
        private Transform childTransform;
        
        [SerializeField]
        private Transform moveTarget;
        
        [SerializeField]
        private Transform center;
        
        [Header("UI"), SerializeField]
        private float uiWidth;

        [SerializeField]
        private float uiHeight;

        [SerializeField]
        private float uiDepth = 0.1f;

        [SerializeField]
        private float speed = 1.0f;

        [SerializeField]
        private float lineRadius = 0.015f;

        [SerializeField]
        private float colliderSize = 0.1f;

        [SerializeField]
        private LayerMask ignoreLayer;

        [Header("Editor용 충돌 디버그"), SerializeField]
        private bool isShowGizmo = false;

        [SerializeField, ShowIf(nameof(isShowGizmo), true)]
        private ColliderInfo[] colliderInfos = new ColliderInfo[4];

        private PXRMovableUIMenu menu;
        private PXRTubeRendererController tubeRenderer;
        private PXRClicker clicker;

        private float amassDist;

        private Vector3 startClickerPosition;
        private Vector3 startTargetPosition;
        private Vector3 originMovePosition;
        private Vector3 preTargetPosition;

        private PXRUIBillboard[] billboards;
        private PXRBaseController leftController;
        private PXRBaseController rightController;
        private PXRBaseController currentController;

        private Transform originMoveParent;
        private Transform originRootParent;
        private Sequence moveSequence;
        
        private CancellationTokenSource cts;

        // Left
        private Vector3 LeftUp => colliderInfos[0].Position;
        private Vector3 LeftDown => colliderInfos[2].Position;
        private Vector3 Left => (colliderInfos[0].Position + colliderInfos[2].Position) * 0.5f;
        private Vector3 LeftUpForward => colliderInfos[0].Position + colliderInfos[0].Forward * uiDepth;
        private Vector3 LeftUpBackward => colliderInfos[0].Position - colliderInfos[0].Forward * uiDepth;

        private Vector3 LeftDownForward => colliderInfos[2].Position + colliderInfos[2].Forward * uiDepth;
        private Vector3 LeftDownBackward => colliderInfos[2].Position - colliderInfos[2].Forward * uiDepth;

        private Vector3 LeftForward => (colliderInfos[0].Position + colliderInfos[2].Position) * 0.5f + colliderInfos[0].Forward * uiDepth;
        private Vector3 LeftBackward => (colliderInfos[0].Position + colliderInfos[2].Position) * 0.5f - colliderInfos[0].Forward * uiDepth;

        private Vector3 RightUp => colliderInfos[1].Position;
        private Vector3 RightDown => colliderInfos[3].Position;
        private Vector3 Right => (colliderInfos[1].Position + colliderInfos[3].Position) * 0.5f;
        private Vector3 RightUpForward => colliderInfos[1].Position + colliderInfos[1].Forward * uiDepth;
        private Vector3 RightUpBackward => colliderInfos[1].Position - colliderInfos[1].Forward * uiDepth;

        private Vector3 RightDownForward => colliderInfos[3].Position + colliderInfos[3].Forward * uiDepth;
        private Vector3 RightDownBackward => colliderInfos[3].Position - colliderInfos[3].Forward * uiDepth;

        private Vector3 RightForward => (colliderInfos[1].Position + colliderInfos[3].Position) * 0.5f + colliderInfos[1].Forward * uiDepth;
        private Vector3 RightBackward => (colliderInfos[1].Position + colliderInfos[3].Position) * 0.5f - colliderInfos[1].Forward * uiDepth;

        private Vector3 Down => (colliderInfos[2].Position + colliderInfos[3].Position) * 0.5f;
        private Vector3 DownForward => (colliderInfos[2].Position + colliderInfos[3].Position) * 0.5f + colliderInfos[2].Forward * uiDepth;
        private Vector3 DownBackward => (colliderInfos[2].Position + colliderInfos[3].Position) * 0.5f - colliderInfos[2].Forward * uiDepth;

        private Vector3 Up => (colliderInfos[0].Position + colliderInfos[1].Position) * 0.5f;
        private Vector3 UpForward => (colliderInfos[0].Position + colliderInfos[1].Position) * 0.5f + colliderInfos[0].Forward * uiDepth;
        private Vector3 UpBackward => (colliderInfos[0].Position + colliderInfos[1].Position) * 0.5f - colliderInfos[0].Forward * uiDepth;

        private Vector3 Forward => center.position + center.forward * uiDepth;
        private Vector3 Backward => center.position - center.forward * uiDepth;
    }
}