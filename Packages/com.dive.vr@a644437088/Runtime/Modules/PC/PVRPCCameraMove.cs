using UnityEngine;

namespace Dive.VRModule
{
    // ReSharper disable once IdentifierTypo
    public class PVRPCCameraMove : MonoBehaviour
    {
        #region Private Fields
        
        [SerializeField]
        private float moveSpeed = 2f;

        [SerializeField]
        private float currentHeight = 0f;

        private bool isBreath = false;

        private const float MaxHeight = 0.1f;

        #endregion
        
        #region Private Methods

        private void Update()
        {
            var localPosition = transform.localPosition;
            
            if (isBreath)
            {
                currentHeight = Mathf.Lerp(currentHeight, MaxHeight, Time.deltaTime * moveSpeed);
                // ReSharper disable once Unity.InefficientPropertyAccess
                transform.localPosition = new Vector3(transform.localPosition.x, currentHeight, localPosition.z);
                if (currentHeight > MaxHeight - 0.01f)
                    isBreath = false;
            }
            else
            {
                currentHeight = Mathf.Lerp(currentHeight, 0f, Time.deltaTime * moveSpeed);
                // ReSharper disable once Unity.InefficientPropertyAccess
                transform.localPosition = new Vector3(transform.localPosition.x, currentHeight, localPosition.z);
                if (currentHeight < 0.01f)
                    isBreath = true;
            }
        }        
        
        #endregion
    }
}