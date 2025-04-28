using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 포인터 이벤트가 캔버스의 UI를 인식할 수 있도록 적용하는 클래스
    /// </summary>
    public class PXRInputModuleCanvasAdder : MonoBehaviour
    {
        #region Private Fields

        private PXRInputModule inputModule;
        private Canvas targetCanvas;

        private bool isCanvasEnabled;        
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            inputModule = FindObjectOfType<PXRInputModule>();
            targetCanvas = GetComponent<Canvas>();
        }

        private void OnEnable()
        {
            targetCanvas.enabled = true;
            isCanvasEnabled = targetCanvas.enabled;

            if (inputModule == null)
            {
                inputModule = FindObjectOfType<PXRInputModule>();
            }
            
            if (inputModule != null)
                inputModule.AddCanvas(targetCanvas);
        }

        private void OnDisable()
        {
            targetCanvas.enabled = false;
            isCanvasEnabled = targetCanvas.enabled;

            if (inputModule != null)
                inputModule.RemoveCanvas(targetCanvas);
        }        
        
        private void Update()
        {
            if (targetCanvas.enabled != isCanvasEnabled)
            {
                isCanvasEnabled = !isCanvasEnabled;
                if (isCanvasEnabled)
                {
                    if (inputModule != null)
                        inputModule.AddCanvas(targetCanvas);
                }
                else
                {
                    if (inputModule != null)
                        inputModule.RemoveCanvas(targetCanvas);
                }
            }
        }        
        
        #endregion
    }
}