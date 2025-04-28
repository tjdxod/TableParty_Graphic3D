using Dive.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dive.VRModule
{
#pragma warning disable 0168  // variable declared but not used.
#pragma warning disable 0219  // variable assigned but not used.
#pragma warning disable 0414  // private field assigned but not used.
    // ReSharper disable once IdentifierTypo
    public abstract class PXRPCControllerBase : MonoBehaviour
    {
        #region Public Fields

        public static StaticVar<PXRPCControllerBase> Instance;        
        
        #endregion
        
        #region Private Fields

        [SerializeField]
        protected Canvas crossHairCanvas;

        [SerializeField]
        protected InputAction rightClick;

        private PXRPCPlayerMovement playerMovement;
        private PXRInputModule inputModule;

        private bool isCursorLock;        
        
        #endregion

        #region Private Methods

        protected virtual void Awake()
        {
            playerMovement = GetComponent<PXRPCPlayerMovement>();
            inputModule = FindObjectOfType<PXRInputModule>();

            Cursor.lockState = CursorLockMode.Locked;
            
            AddMenuEvent();
        }

        protected void CursorLock()
        {
            if (PXRRig.IsVRPlay)
                return;

            isCursorLock = true;

            crossHairCanvas.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            playerMovement.EnableMoveAndRotate();
            inputModule.EnableVRPointer();
        }
        
        protected virtual void CursorNone()
        {
            if (PXRRig.IsVRPlay)
                return;

            isCursorLock = false;

            crossHairCanvas.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            playerMovement.DisableMoveAndRotate();
            inputModule.DisableVRPointer();
        }
        
        protected abstract void AddMenuEvent();        
        
        #endregion
    }
}
