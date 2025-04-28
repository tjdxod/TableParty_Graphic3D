using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// HMD의 움직임에 따라 아바타의 머리가 회전
    /// </summary>
    public class PXRRigHeadRotator : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private float headXClampAngle = 50f;

        [SerializeField]
        private float headZClampAngle = 25f;

        // [SerializeField]
        // private bool isRotator = false;

        private Transform head = null;
        private Transform body = null;
        private Transform camTr = null;

        private Vector3 prePosition = Vector3.zero;

        #endregion

        #region Public Properties

        public Vector3 PrePosition => prePosition;

        /// <summary>
        /// 머리의 Rotation.x 제한 각도
        /// </summary>
        public float HeadXClampAngle => headXClampAngle;
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// 머리와 몸 트랜스폼을 갱신
        /// </summary>
        /// <param name="changeHead">머리 Transform</param>
        /// <param name="changeBody">몸 Transform</param>
        public void SetHeadBody(Transform changeHead, Transform changeBody)
        {
            head = changeHead;
            body = changeBody;

            if (changeHead == null || changeBody == null)
                Debug.LogError($"{nameof(SetHeadBody)} 에러");
        }
        
        #endregion

        #region Private Methods
        
        private void Awake()
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("MainCamera가 없습니다.");
                return;
            }

            camTr = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (camTr == null)
                return;

            prePosition = transform.position;

            if (head != null)
            {
                Quaternion tmpRot = camTr.localRotation;
                tmpRot.y = 0.01f;
                head.localRotation = ClampRotation(tmpRot, new Vector3(headXClampAngle, 0.01f, headZClampAngle));
            }

            if (body != null)
            {
                Quaternion tmpRot2 = camTr.rotation;
                tmpRot2.x = 0;
                tmpRot2.z = 0;
                body.localRotation = tmpRot2;
            }
        }
        
        /// <summary>
        /// 제한된 범위 내의 Quaternion을 반환
        /// </summary>
        /// <param name="quaternion">각을 제한하고 싶은 Quaternion</param>
        /// <param name="bounds">각 축의 제한 각도</param>
        /// <returns></returns>
        private Quaternion ClampRotation(Quaternion quaternion, Vector3 bounds)
        {
            quaternion.x /= quaternion.w;
            quaternion.y /= quaternion.w;
            quaternion.z /= quaternion.w;
            quaternion.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(quaternion.x);
            angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
            quaternion.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(quaternion.y);
            angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
            quaternion.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

            float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(quaternion.z);
            angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
            quaternion.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

            //NaN 검사
            if (float.IsNaN(quaternion.x)) quaternion.x = 0f;
            if (float.IsNaN(quaternion.y)) quaternion.y = 0f;
            if (float.IsNaN(quaternion.z)) quaternion.z = 0f;

            return quaternion.normalized;
        }

        /// <summary>
        /// 제한된 범위 내의 EulerAngle을 반환
        /// </summary>
        /// <param name="euler">각을 제한하고 싶은 EulerAngle</param>
        /// <param name="bounds">각 축의 제한 각도</param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        private Quaternion ClampRotation(Vector3 euler, Vector3 bounds)
        {
            var angleX = euler.x;
            var angleY = euler.y;
            var angleZ = euler.z;

            if (Mathf.Abs(Mathf.Abs(angleX) - 0) < 0.01f)
                angleX = 0.01f;

            if (Mathf.Abs(Mathf.Abs(angleY) - 0) < 0.01f)
                angleY = 0.01f;

            if (Mathf.Abs(Mathf.Abs(angleZ) - 0) < 0.01f)
                angleZ = 0.01f;

            if (Mathf.Abs(Mathf.Abs(angleX) - 180) < 0.01f)
                angleX = 179.99f;

            if (Mathf.Abs(Mathf.Abs(angleY) - 180) < 0.01f)
                angleY = 179.99f;

            if (Mathf.Abs(Mathf.Abs(angleZ) - 180) < 0.01f)
                angleZ = 179.99f;

            if (angleX > 180)
                angleX -= 360;

            if (angleY > 180)
                angleY -= 360;

            if (angleZ > 180)
                angleZ -= 360;

            angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
            angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
            angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);

            return Quaternion.Euler(new Vector3(angleX, angleY, angleZ));
        }    
        
        #endregion
    }
}