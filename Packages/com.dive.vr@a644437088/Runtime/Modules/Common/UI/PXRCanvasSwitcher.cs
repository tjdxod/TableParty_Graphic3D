using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// Canvas 컴포넌트의 활성화 비활성화 관리
    /// </summary>
    public class PXRCanvasSwitcher : MonoBehaviour
    {
        #region Private Fields
        
        private Canvas targetCanvas;
        
        #endregion
        
        #region Private Methods
        
        private void Awake()
        {
            targetCanvas = GetComponent<Canvas>();
        }

        private void OnEnable()
        {
            targetCanvas.enabled = true;
        }

        private void OnDisable()
        {
            targetCanvas.enabled = false;
        }
        
        #endregion
    }
}
