using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 자식 오브젝트가 그랩이 가능한 경우 자식에 추가하는 클래스
    /// 자식이 그랩되더라도 부모의 Grabbable 실행
    /// </summary>
    public class PXRGrabbableChild : MonoBehaviour, IGrabbable
    {
        #region Public Properties

        /// <summary>
        /// 부모의 Grabbable
        /// </summary>
        public PXRGrabbable ParentGrabbable { get; private set; }
        
        #endregion
        
        #region Public Methods

        /// <summary>
        /// Grabbable을 반환
        /// </summary>
        /// <returns>Grabbable 클래스</returns>
        public PXRGrabbable GetGrabbable()
        {
            return ParentGrabbable;
        }        
        
        #endregion

        #region Private Methods

        private void Start()
        {
            if (!ParentGrabbable)
                ParentGrabbable = GetComponentInParent<PXRGrabbable>();

            ParentGrabbable.AddChildGrabbable(this);
            gameObject.layer = PXRNameToLayer.Interactable;
        }        
        
        #endregion
    }
}
