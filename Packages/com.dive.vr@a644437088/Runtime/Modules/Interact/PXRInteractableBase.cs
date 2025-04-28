using System;
// ReSharper disable once RedundantUsingDirective
using UnityEngine;
using UnityEngine.EventSystems;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    /// <summary>
    /// 상호작용이 가능한 오브젝트의 베이스
    /// </summary>
    public partial class PXRInteractableBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region Private Fields

        /// <summary>
        /// 상호작용 가능한 상태를 나타냄 (현재는 미사용)
        /// </summary>
        [Obsolete]
        [SerializeField, HideInInspector]
        protected bool canInteract = true;

        /// <summary>
        /// 상호작용 상태를 나타냄
        /// </summary>
        [Tooltip("현재 오브젝트의 상호작용 상태를 나타냄")]
        [SerializeField, Header("상호작용"), LabelText("잡을 수 있는 컨트롤러")]
        protected TransferState transferState = TransferState.Both;
        
        /// <summary>
        /// 상호작용 상태를 나타냄 [Obsolete]
        /// </summary>
        [Obsolete]
        public bool CanInteract
        {
            get { return transferState != TransferState.None; }
            set { TransferState = value ? TransferState.Both : TransferState.None; }
        }

        #endregion

        #region Private Fields

        protected Vector3 originPosition;
        protected Quaternion originRotation;

        protected PXRPointerVR leftPointer;
        protected PXRPointerVR rightPointer;
        
        #endregion
        
        #region Public Properties

        /// <summary>
        /// 상호작용 상태를 나타냄
        /// 상태에 따라 아웃라인 색상 변경
        /// </summary>
        public TransferState TransferState
        {
            get => transferState;
            set
            {
                transferState = value;

                if (luxOutline)
                {
                    if (value != TransferState.None)
                    {
                        luxOutline.OutlineColor = GetCanInteractColor();
                    }
                    else
                    {
                        luxOutline.OutlineColor = GetCanNotInteractColor();
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 레이어 초기화
        /// </summary>
        protected virtual void Awake()
        {
            gameObject.layer = PXRNameToLayer.Interactable;

            leftPointer = PXRRig.LeftPointer;
            rightPointer = PXRRig.RightPointer;
            
            originPosition = transform.position;
            // ReSharper disable once Unity.InefficientPropertyAccess
            originRotation = transform.rotation;
        }

        /// <summary>
        /// 아웃라인 초기화
        /// </summary>
        protected virtual void Start()
        {
            SetOutline();
        }

        #endregion
    }
}