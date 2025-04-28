using System.Collections;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// Grabbable 회전 방향
    /// </summary>
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    /// <summary>
    /// 회전 가능한 Grabbable에 나타나는 회전 가이드 링
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class PXRGrabbableRing : MonoBehaviour
    {
        #region Private Fields

        [Tooltip("점 갯수")]
        [SerializeField, Range(0, 50)]
        private int segments = 50;

        private PXRGrabber grabber;

        private IEnumerator routineFollowGrabbable;

        private float originStartWidth = 0f;
        private float originEndWidth = 0f;
        private readonly float radiusExtendValue = 1.2f;

        private LineRenderer lineRenderer;

        #endregion

        #region Private Properties

        private LineRenderer LineRenderer
        {
            get
            {
                if (!lineRenderer)
                {
                    lineRenderer = GetComponent<LineRenderer>();
                    lineRenderer.positionCount = segments + 1;
                    lineRenderer.useWorldSpace = false;

                    originStartWidth = lineRenderer.startWidth;
                    originEndWidth = lineRenderer.endWidth;
                }

                return lineRenderer;
            }
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// rotation의 값을 초기 값으로 변경
        /// </summary>
        public void ResetRotation()
        {
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 가이드 링 활성화
        /// </summary>
        public void ActivateRing()
        {
            if (!grabber)
                grabber = GetComponentInParent<PXRGrabber>();

            if (grabber.GrabbedGrabbable == null)
                return;

            gameObject.SetActive(true);

            LineRenderer.startWidth = originStartWidth;
            LineRenderer.endWidth = originEndWidth;

            var grabbable = grabber.GrabbedGrabbable;
            var bounds = grabbable.CalculateLocalBounds();

            var diff = grabbable.transform.position - bounds.center;
            transform.position = bounds.center + diff;

            var radiusExtension = diff.magnitude;
            var radius = GetRadius(bounds) + (radiusExtension * 0.5f);
            radius *= 1 / grabbable.transform.localScale.x;

            CreatePoints(radius);

            if (routineFollowGrabbable != null)
            {
                StopCoroutine(routineFollowGrabbable);
                routineFollowGrabbable = null;
            }

            routineFollowGrabbable = CoroutineFollowGrabbable();
            StartCoroutine(routineFollowGrabbable);
        }

        /// <summary>
        /// 가이드 링 비활성화
        /// </summary>
        public void DeactivateRing()
        {
            if (routineFollowGrabbable != null)
            {
                StopCoroutine(routineFollowGrabbable);
                routineFollowGrabbable = null;
            }

            gameObject.SetActive(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Grabbable의 바운드 크기로 오브젝트의 크기 계산 
        /// </summary>
        /// <param name="bounds">Grabbable의 바운드</param>
        /// <returns>오브젝트의 반지름 길이</returns>
        private float GetRadius(Bounds bounds)
        {
            var posArray = new Vector3[8];

            posArray[0] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
            posArray[1] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);

            posArray[2] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            posArray[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);

            posArray[4] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            posArray[5] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);

            posArray[6] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            posArray[7] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            
            var max = 0f;

            foreach (var pos in posArray)
            {
                var dist = Vector3.Magnitude(bounds.center - pos);

                if (dist > max)
                {
                    max = dist;
                }
            }

            return max;
        }
        
        /// <summary>
        /// 곡선 형태의 라인 렌더러 생성
        /// </summary>
        /// <param name="radius">반지름의 길이</param>
        private void CreatePoints(float radius)
        {
            var angle = 20f;

            for (var i = 0; i < (segments + 1); i++)
            {
                var x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius * radiusExtendValue;
                var y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius * radiusExtendValue;

                LineRenderer.SetPosition(i, new Vector3(x, 0, y));

                angle += (360f / segments);
            }
        }
        
        /// <summary>
        /// 가이드 링이 잡고있는 Grabbable를 추적하는 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoroutineFollowGrabbable()
        {
            while (true)
            {
                if (!grabber.GrabbedGrabbable)
                {
                    gameObject.SetActive(false);
                    yield break;
                }

                transform.position = grabber.GrabbedGrabbable.transform.position;
                yield return null;
            }
        }

        #endregion
    }
}