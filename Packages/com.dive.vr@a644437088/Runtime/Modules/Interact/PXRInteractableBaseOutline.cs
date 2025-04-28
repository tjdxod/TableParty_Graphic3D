#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#else

using EasyButtons;

#endif

using UnityEngine;

namespace Dive.VRModule
{
    public partial class PXRInteractableBase
    {
        #region Private Fields

        /// <summary>
        /// 오브젝트의 아웃라인
        /// </summary>
        [SerializeField, Header("아웃라인"), LabelText("Lux 아웃라인"), Tooltip("Interactable 아웃라인, 존재하지 않는 경우 null로 두기")]
        protected LuxOutline luxOutline;

        [SerializeField, ReadOnly, LabelText("아웃라인 고정 색상 모드")]
        protected bool onlyOutlineColor = false;        
        
        /// <summary>
        /// 이전에 사용한 Outline값을 저장하는 변수
        /// </summary>
        protected LuxOutline savedLuxOutline;
        
        private bool hasCustomInteractOutline = false;
        private bool hasCustomNonInteractOutline = false;
        
        private Color onlyColor = Color.white;
        private Color interactColor = Color.white;
        private Color nonInteractColor = Color.white;
        
        #endregion

        #region Public Properties

        /// <summary>
        /// 상호작용 가능한 상태를 나타냄 (현재는 미사용)
        /// </summary>
        public bool IsLuxOutline => luxOutline;

        #endregion

        #region Public Methods

        /// <summary>
        /// 아웃라인 컴포넌트 활성화
        /// </summary>
        public void ActivateOutline()
        {
            if (luxOutline == null)
                return;

            if (luxOutline.enabled)
                return;
            
            luxOutline.OutlineColor = TransferState != TransferState.None ? GetCanInteractColor() : GetCanNotInteractColor();
            luxOutline.enabled = true;
        }

        /// <summary>
        /// 아웃라인 컴포넌트 비활성화
        /// </summary>
        public void DeactivateOutline()
        {
            if (luxOutline == null)
                return;

            if (luxOutline.enabled)
                luxOutline.enabled = false;
        }

        public void SetOutlineColor(Color color)
        {
            if (luxOutline == null)
                return;

            luxOutline.OutlineColor = color;
        }
        
        /// <summary>
        /// 아웃라인을 컴포넌트를 Null로 바꿈
        /// </summary>
        public void ConvertOutlineToNull()
        {
            DeactivateOutline();
            savedLuxOutline = luxOutline;

            luxOutline = null;
        }
        
        /// <summary>
        /// Null로 바뀐 아웃라인을 다시 되돌림
        /// </summary>
        public void RestoreOutline()
        {
            luxOutline = savedLuxOutline;
        }

        public void SetInteractColor(Color color)
        {
            hasCustomInteractOutline = true;
            interactColor = color;
        }
        
        public void SetNonInteractColor(Color color)
        {
            hasCustomNonInteractOutline = true;
            nonInteractColor = color;
        }

        public void SetOnlyColor(bool isOnly)
        {
            onlyOutlineColor = isOnly;
        }
        
        public void SetOnlyOutlineColor(Color color)
        {
            if (luxOutline == null)
                return;

            onlyColor = color;
        }
        
        #endregion
        
        #region Private Methods

#if UNITY_EDITOR
        
        /// <summary>
        /// 아웃라인 컴포넌트 추가 (에디터에서만 실행하기)
        /// </summary>
        [Button]
        private void AddOutline()
        {
            if (GetComponent<LuxOutline>() != null)
                return;

            var newLuxOutline = GetComponent<LuxOutline>();

            luxOutline = !newLuxOutline ? gameObject.AddComponent<LuxOutline>() : newLuxOutline;

            luxOutline.enabled = false;
            luxOutline.OutlineMode = LuxOutline.Mode.OutlineVisible;
            luxOutline.precomputeOutline = true;
            luxOutline.OutlineWidth = 2f;
        }

#endif

        /// <summary>
        /// 아웃라인 이벤트 추가
        /// </summary>
        private void SetOutline()
        {
            if (!luxOutline) 
                return;
            
            PointerEnterEvent += ActivateOutline;
            PointerExitEvent += DeactivateOutline;

            luxOutline.OutlineColor = TransferState != TransferState.None ? GetCanInteractColor() : GetCanNotInteractColor();
        }

        /// <summary>
        /// 상호작용 가능한 오브젝트의 색 반환
        /// </summary>
        /// <returns></returns>
        protected Color GetCanInteractColor()
        {
            if (PXRRig.Current == null)
            {
                return new Color(0.19f, 0.95f, 0.31f, 1f);
            }

            if (onlyOutlineColor)
            {
                return onlyColor;
            }
            
            return hasCustomInteractOutline ? interactColor : PXRRig.Current.CanInteractColor;
        }

        /// <summary>
        /// 상호작용 불가능한 오브젝트의 색 반환
        /// </summary>
        /// <returns></returns>
        protected Color GetCanNotInteractColor()
        {
            if (PXRRig.Current == null)
            {
                return new Color(0.89f, 0.28f, 0.28f, 1f);
            }
            
            if (onlyOutlineColor)
            {
                return onlyColor;
            }
            
            return hasCustomNonInteractOutline ? nonInteractColor : PXRRig.Current.CanNotInteractColor;
        }


        #endregion
    }
}