using System;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public enum SpaceType
    {
        Area,
        Point
    }

    public abstract class PXRTeleportSpaceBase : MonoBehaviour, ITeleportSpace
    {
        #region Public Fields

        public event Action EnterPlayerEvent;
        public event Action ExitPlayerEvent;
        
        #endregion

        #region Private Fields

        private Collider spaceCollider;
        private float cachedMinY = 100;
        private float cachedMaxY = -100;
        
        #endregion
        
        #region Public Properties

        [field: Tooltip("텔레포트 공간의 타입"), Header("텔레포트 상태")]
        [field: LabelText("텔레포트 공간 타입"), SerializeField]
        public SpaceType SpaceType { get; protected set; } = SpaceType.Area;

        [field: Tooltip("텔레포트를 할 때만 활성화 할 것인지")]
        [field: LabelText("텔레포트 시 활성화"), SerializeField]
        public bool UseActiveOnlyTeleporting { get; private set; } = true;

        [field: Tooltip("텔레포트가 가능한 경우 true, 불가능한 경우 false")]
        [field: LabelText("텔레포트 가능 상태"), SerializeField]
        public bool CanTeleport { get; private set; } = true;

        [field: Tooltip("텔레포트 공간의 렌더러를 활성화하는 경우 true, 비활성화하는 경우 false")]
        [field: LabelText("렌더러 활성화 상태"), SerializeField]
        public bool CanVisible { get; protected set; } = false;

        [field: Header("고정")]
        [field: LabelText("텔레포트 시 고정된 방향을 바라보는지"), SerializeField]
        public bool UseFixedDirection { get; private set; }

        [field: LabelText("텔레포트 시 고정된 높이를 유지하는지"), SerializeField]
        public bool UseFixedHeight { get; private set; } = false;

        [field: Header("플레이어")]
        [field: LabelText("현재 플레이어 입장 상태"), SerializeField]
        public bool IsEnteredPlayer { get; protected set; } = false;

        [field: LabelText("현재 공간에 있는 플레이어 수"), SerializeField, ReadOnly, ShowIf(nameof(SpaceType), SpaceType.Area)]
        public int CurrentInsidePlayerCount { get; private set; } = 0;

        [field: Space, SerializeField, LabelText("입장 시 다른 위치로 강제 이동 시키는 경우")]
        public bool UseForceTransform { get; private set; } = false;

        [field: LabelText("강제 이동 시킬 Transform"), SerializeField, ShowIf(nameof(UseForceTransform), true)]
        public Transform ForceTransform { get; private set; } = null;

        #endregion

        #region Private Properties

        private Collider SpaceCollider
        {
            get
            {
                if (spaceCollider == null)
                    spaceCollider = GetComponent<Collider>();

                return spaceCollider;
            }
        }

        #endregion
        
        #region Public Methods

        public virtual void OnEnterPlayer()
        {
            EnterPlayerEvent?.Invoke();
            IncreaseInsidePlayerCount();            
            
            if(SpaceType == SpaceType.Point)
                IsEnteredPlayer = true;
            else
                IsEnteredPlayer = CurrentInsidePlayerCount != 0;
            
            EnteredPlayerToSpace();
            PXRRig.Current.OwnerArea = this;
            
            // TODO: 미사용 기능
            if(UseFixedHeight)
                PXRRig.PlayerController.ForceChangeEyeHeight();
        }

        public virtual void OnExitPlayer()
        {
            ExitPlayerEvent?.Invoke();
            DecreaseInsidePlayerCount();           
            
            if(SpaceType == SpaceType.Point)
                IsEnteredPlayer = false;
            else
                IsEnteredPlayer = CurrentInsidePlayerCount != 0;
            
            ExitedPlayerToSpace();
            PXRRig.Current.OwnerArea = null;
        }

        public virtual void ActiveSpace()
        {
            if(this == null)
                return;
            
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);
        }

        public virtual void InactiveSpace()
        {
            if (this == null)
                return;
            
            if (gameObject.activeInHierarchy)
                gameObject.SetActive(false);
        }
        
        public virtual void EnableCanTeleport()
        {
            CanTeleport = true;
        }

        public virtual void DisableCanTeleport()
        {
            CanTeleport = false;
        }

        public virtual void EnableCanVisible()
        {
            CanVisible = true;
        }
        
        public virtual void DisableCanVisible()
        {
            CanVisible = false;
        }

        public abstract PXRTeleportSpaceBase GetTeleportSpace();
        public abstract PXRTeleportSpaceBase GetOriginalTeleportSpace();

        public float GetSpaceMaxY()
        {
            if (cachedMaxY < -50)
                cachedMaxY = SpaceCollider.bounds.max.y;

            return cachedMaxY;
        }

        public float GetSpaceMinY()
        {
            if (cachedMinY > 50)
                cachedMinY = SpaceCollider.bounds.min.y;

            return cachedMinY;
        }


        public void IncreaseInsidePlayerCount()
        {
            CurrentInsidePlayerCount++;
        }

        public void DecreaseInsidePlayerCount()
        {
            if (CurrentInsidePlayerCount > 0)
                CurrentInsidePlayerCount--;
        }

        public abstract void AdditionalActive();
        public abstract void AdditionalInactive();

        public abstract void EnteredPlayerToSpace();

        public abstract void ExitedPlayerToSpace();        
        
        #endregion

        #region Private Methods

        protected virtual void Awake()
        {
            gameObject.layer = PXRNameToLayer.TeleportSpace;
        }

        private void Reset()
        {
            gameObject.layer = PXRNameToLayer.TeleportSpace;
        }
        
        protected virtual void RegisterActiveOnlyTeleport()
        {
            if (!UseActiveOnlyTeleporting)
                return;

            var leftTeleporter = PXRRig.LeftTeleporter;
            var rightTeleporter = PXRRig.RightTeleporter;

            if (leftTeleporter != null)
                leftTeleporter.AddTeleportSpace(this);

            if (rightTeleporter != null)
                rightTeleporter.AddTeleportSpace(this);

            InactiveSpace();
        }

        protected virtual void UnregisterActiveOnlyTeleport()
        {
            var leftTeleporter = PXRRig.LeftTeleporter;
            var rightTeleporter = PXRRig.RightTeleporter;

            if (leftTeleporter != null)
                leftTeleporter.RemoveTeleportSpace(this);

            if (rightTeleporter != null)
                rightTeleporter.RemoveTeleportSpace(this);
        }

        #endregion
    }
}