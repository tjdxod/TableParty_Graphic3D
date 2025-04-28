using UnityEngine;

namespace Dive.VRModule
{
    [RequireComponent(typeof(Canvas))]
    public class PXRCanvasWorldToOverlay : MonoBehaviour
    {
        #region Private Fields

        private Canvas targetCanvas;
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            targetCanvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            if (!PXRRig.IsVRPlay)
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }        
        
        #endregion
    }
}
