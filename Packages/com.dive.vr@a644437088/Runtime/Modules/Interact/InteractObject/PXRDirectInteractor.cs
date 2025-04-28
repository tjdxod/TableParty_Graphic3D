using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 직접 상호작용을 감지하여 동작하는 클래스
    /// </summary>
    public class PXRDirectInteractor : MonoBehaviour
    {
        #region Private Fields
        
        /// <summary>
        /// 왼손 / 오른손을 구분하는 열거형 변수
        /// </summary>
        [Tooltip("왼손 / 오른손을 구분하는 열거형 변수")]
        [SerializeField]
        private HandSide handSide;

        [Tooltip("왼손 / 오른손의 Grabber")]
        [SerializeField]
        private PXRGrabber grabber;
        
        private PXRDirectInteractableBase currentInteractable;
        private readonly List<int> interactableHash = new List<int>();

        private IEnumerator routineDelay;
        
        #endregion
        
        #region Public Methods

        /// <summary>
        /// 리스트 초기화
        /// </summary>
        public void ClearHashList()
        {
            interactableHash?.Clear();
        }

        /// <summary>
        /// 상호작용 오브젝트 클래스를 설정
        /// </summary>
        /// <param name="interactableBase">상호작용 오브젝트 베이스 클래스</param>
        public void SetInteractable(PXRDirectInteractableBase interactableBase = null)
        {
            currentInteractable = interactableBase;
        }
        
        /// <summary>
        /// 현재 상호작용중인 손을 설정
        /// </summary>
        /// <param name="targetHandSide">손의 방향</param>
        public void SetHandSide(HandSide targetHandSide)
        {
            handSide = targetHandSide;
        }        
        
        #endregion
        
        #region Private Methods
        
        private void OnTriggerEnter(Collider other)
        {
            if (grabber == null || grabber.IsGrabbing)
                return;
            
            var interactable = other.GetComponent<PXRDirectInteractableBase>();
            if (!interactable) 
                return;
            
            if (!interactable.CanInteract || currentInteractable)
                return;
            
            if (interactable.UseDelay)
            {
                if (interactableHash.Contains(interactable.GetHashCode()))
                    return;

                interactableHash.Add(interactable.GetHashCode());
            }
            
            currentInteractable = interactable;
            currentInteractable.Hover(this);
            currentInteractable.SetInteractHand(handSide);
        }


        private void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<PXRDirectInteractableBase>();
            if (!interactable) 
                return;

            if (interactable != currentInteractable) 
                return;
            
            currentInteractable.UnHover();
            currentInteractable = null;

            if (!interactable.UseDelay) 
                return;
            
            if (routineDelay != null)
                return;
                
            routineDelay = CoroutineDelay(interactable);
            StartCoroutine(routineDelay);
        }
        
        private IEnumerator CoroutineDelay(PXRDirectInteractableBase interactableBase)
        {
            var hash = interactableBase.GetHashCode();
            var delay = interactableBase.DelayTime;
            
            yield return YieldInstructionCache.WaitForSeconds(delay);
            
            if(interactableHash.Contains(hash))
                interactableHash.Remove(hash);
            
            routineDelay = null;
        }        
        #endregion
    }
}
