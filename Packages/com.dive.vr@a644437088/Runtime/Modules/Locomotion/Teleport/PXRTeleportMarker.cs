using UnityEngine;

namespace Dive.VRModule.Locomotion
{
    public class PXRTeleportMarker : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private GameObject enableMarker;

        [SerializeField]
        private GameObject disableMarker;

        [SerializeField]
        private GameObject directionMarker;        
        
        #endregion
        
        #region Public Methods

        public void Activate()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void Inactivate()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
        
        public void ActivateEnableMarker()
        {
            if(!enableMarker.activeSelf)
                enableMarker.SetActive(true);
            
            if(disableMarker.activeSelf)
                disableMarker.SetActive(false);

            if(!directionMarker.activeSelf)
                directionMarker.SetActive(true);
        }
        
        public void ActivateDisableMarker()
        {
            if(enableMarker.activeSelf)
                enableMarker.SetActive(false);
            
            if(!disableMarker.activeSelf)
                disableMarker.SetActive(true);

            if(directionMarker.activeSelf)
                directionMarker.SetActive(false);
        }

        public void DeactivateAllMarker()
        {
            if(enableMarker.activeSelf)
                enableMarker.SetActive(false);
            
            if(disableMarker.activeSelf)
                disableMarker.SetActive(false);
        }        
        
        #endregion

        #region Private Methods

        private void OnDisable()
        {
            DeactivateAllMarker();
        }        
        
        #endregion
    }

}

