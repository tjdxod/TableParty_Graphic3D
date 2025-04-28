using UnityEngine;

namespace Dive.VRModule
{
    public class PXRUIBillboard : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Transform additionalTarget;
        
        [SerializeField]
        private float additionalStandard = -30f;

        [SerializeField]
        private bool additionalReverse = false;
        
        [SerializeField]
        private Vector2 minMaxAngle = new Vector2(-50, 20);

        [SerializeField]
        private bool is180Rotate = false;

        private Quaternion originLocalRotation;
        
        private Transform target;        

        #endregion

        #region Public Methods

        public void SetOrigin()
        {
            transform.localRotation = originLocalRotation;
        }

        #endregion
        
        #region Private Methods

        private void Awake()
        {
            if (Camera.main != null)
                target = Camera.main.transform;
            
            originLocalRotation = transform.localRotation;
        }
        
        private void Update()
        {
            if (target == null)
                return;

            var lookDir = is180Rotate ? (target.position - transform.position).normalized : (transform.position - target.position).normalized;
            var lookRotation = Quaternion.LookRotation(lookDir);
            
            var euler = lookRotation.eulerAngles;
            
            euler.x = euler.x > 180 ? euler.x - 360 : euler.x;
            euler.x = Mathf.Clamp(euler.x, minMaxAngle.x, minMaxAngle.y);
            
            lookRotation = Quaternion.Euler(euler);
            
            var toLocalRotation = transform.parent.InverseTransformDirection(lookRotation.eulerAngles);

            toLocalRotation.y = toLocalRotation.x > 0 ? 179.9f : 0.01f;
            toLocalRotation.z = toLocalRotation.x > 0 ? 0.01f : 179.9f;
            
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(toLocalRotation), Time.deltaTime * 10f);

            if (euler.x < additionalStandard)
            {
                var compare = additionalStandard - euler.x;
                
                if(compare > Mathf.Abs(15))
                    compare = Mathf.Abs(15);
                
                var newEuler = new Vector3(additionalReverse ? -15 + compare : 15 - compare, 0, 0);
                additionalTarget.localEulerAngles = Vector3.Lerp(additionalTarget.localEulerAngles, newEuler, Time.deltaTime * 10f);
            }
            else
            {
                var newEuler = new Vector3(additionalReverse ? -15 : 15, 0, 0);
                additionalTarget.localEulerAngles = Vector3.Lerp(additionalTarget.localEulerAngles, newEuler, Time.deltaTime * 10f);
            }
        }        

        #endregion
    }
}