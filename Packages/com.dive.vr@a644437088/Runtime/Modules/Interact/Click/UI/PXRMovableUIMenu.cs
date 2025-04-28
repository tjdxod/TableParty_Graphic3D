using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    public class PXRMovableUIMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Private Fields

        private RectTransform myRectTransform;
        private PXRMovableUI movableUI;        

        #endregion

        #region Private Properties

        public Vector3 Position => myRectTransform.position;        

        #endregion

        #region Public Methods

        public void Initialize(PXRMovableUI movable)
        {
            movableUI = movable;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            myRectTransform.DOScale(Vector3.one * 1.2f, 0.2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (movableUI.IsClicked)
                return;
            
            myRectTransform.DOScale(Vector3.one, 0.2f);
        }        

        #endregion
        
        #region Private Methods

        private void Awake()
        {
            myRectTransform = GetComponent<RectTransform>();
        }        

        #endregion
    }
}
