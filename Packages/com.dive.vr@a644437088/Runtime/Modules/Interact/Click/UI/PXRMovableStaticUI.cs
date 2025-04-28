using UnityEngine;

namespace Dive.VRModule
{
    [RequireComponent(typeof(BoxCollider))]
    public class PXRMovableStaticUI : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Vector3 staticSize;

        [SerializeField]
        private Vector3 staticCenter;

        [SerializeField]
        private Vector3 staticDirection;
        
        [SerializeField]
        private bool isShowGizmo = false;

        private BoxCollider boxCollider;
        
        #endregion

        #region Public Properties

        public Quaternion LookRotation => Quaternion.LookRotation(ForwardDirection, UpDirection);
        public Vector3 CenterPosition => transform.position + transform.up * staticCenter.y + transform.right * staticCenter.x + transform.forward * staticCenter.z;

        #endregion

        #region Private Properties

        private Vector3 ForwardDirection => (CenterPosition + staticDirection) - CenterPosition;
        private Vector3 UpDirection => Vector3.Cross(ForwardDirection, transform.right);
        
        #endregion
        
        #region Public Methods

        public void EnableCollider()
        {
            boxCollider.enabled = true;
        }

        public void DisableCollider()
        {
            boxCollider.enabled = false;
        }

        #endregion
        
        #region Private Methods

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }
        
#if UNITY_EDITOR
        
        private void OnValidate()
        {
            boxCollider = GetComponent<BoxCollider>();

            if (boxCollider == null)
                return;

            boxCollider.enabled = false;
            boxCollider.isTrigger = true;
            boxCollider.size = staticSize;
            boxCollider.center = staticCenter;
        }

        private void OnDrawGizmos()
        {
            if (isShowGizmo)
            {
                Gizmos.color = Color.red;
                
                Gizmos.DrawRay(CenterPosition, ForwardDirection.normalized * 0.05f);
                Gizmos.DrawRay(CenterPosition, UpDirection.normalized * 0.05f);
            }
        }

#endif        

        #endregion
    }
}