using UnityEngine;

namespace Dive.VRModule
{
    public class PXRPressableButtonChild : MonoBehaviour, IDirectable
    {
        #region Private Fields

        private PXRDirectableBase parentDirectableBase;

        #endregion

        #region Private Properties

        private PXRDirectableBase ParentDirectableBase
        {
            get
            {
                if (parentDirectableBase == null)
                    parentDirectableBase = GetComponentInParent<PXRDirectableBase>();

                return parentDirectableBase;
            }
        }

        #endregion

        #region Public Methods

        public PXRDirectableBase GetInteractableBase()
        {
            return ParentDirectableBase;
        }

        public void ForceRelease(bool useEvent = true)
        {
            GetInteractableBase().ForceRelease(useEvent);
        }

        public void ForcePress(bool useEvent = true)
        {
            GetInteractableBase().ForceRelease(useEvent);
        }

        public IAdditionalPressableButton GetAdditionalPressableButton()
        {
            return ParentDirectableBase.GetComponent<IAdditionalPressableButton>();
        }
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            gameObject.layer = PXRNameToLayer.Directable;
        }

        #endregion
    }
}