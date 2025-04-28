using UnityEngine;
using UnityEngine.InputSystem;

namespace Dive.VRModule
{
    public class PXRPointerMouse : PXRPointerBase
    {
        #region Public Methods

        public override void Process(out bool isValid)
        {
            isValid = canProcess;

            if (!canProcess) 
                return;
            
            PointerEventData.Reset();
            PointerEventData.position = Mouse.current.position.ReadValue();
            PointerEventData.scrollDelta = Vector2.zero;
        }        
        
        #endregion
    }
}

