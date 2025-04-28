using UnityEngine;

namespace Dive.VRModule.TestBed
{
    public class Billboard : MonoBehaviour
    {
        private Transform target;

        [SerializeField]
        private bool xLock = false;

        [SerializeField]
        private bool yLock = false;

        [SerializeField]
        private bool zLock = false;

        [SerializeField]
        private float lerpSpeed = 10f;

        [SerializeField]
        private bool is180Rotate = false;

        private float originLerpSpeed;

        private void Awake()
        {
            if (Camera.main != null)
                target = Camera.main.transform;
            originLerpSpeed = lerpSpeed;
        }

        private void OnEnable()
        {
            lerpSpeed = 1 / Time.deltaTime;

            DoBillboard();

            lerpSpeed = originLerpSpeed;
        }

        private void Update()
        {
            DoBillboard();
        }

        private void DoBillboard()
        {
            if (target == null)
                return;
            
            var dir = is180Rotate
                ? (target.position - transform.position).normalized
                : (transform.position - target.position).normalized;

            if (xLock)
                dir.x = 0f;

            if (yLock)
                dir.y = 0f;

            if (zLock)
                dir.z = 0f;

            if (dir == Vector3.zero)
                return;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), lerpSpeed * Time.deltaTime);
        }
    }
}

