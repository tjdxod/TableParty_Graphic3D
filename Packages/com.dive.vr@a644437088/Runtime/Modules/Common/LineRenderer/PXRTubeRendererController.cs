using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    public class PXRTubeRendererController : MonoBehaviour
    {
        #region Private Fields

        private PXRTubeRenderer tubeRenderer;

        private TubePoint[] tubePoints;
        private readonly List<Vector3> calculatedPoints = new List<Vector3>();
        private const int PointCount = 50;

        #endregion

        #region Public Properties

        /// <summary>
        /// 현재 컨트롤러의 PXRTubeRenderer 반환
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public PXRTubeRenderer TubeRenderer
        {
            get
            {
                if (!tubeRenderer)
                    tubeRenderer = GetComponent<PXRTubeRenderer>();

                return tubeRenderer;
            }
        }

        #endregion

        #region Public Methods

        public void ShowTubeRenderer(Vector3 start, Vector3 middle, Vector3 end)
        {
            PointRay(start, middle, end);
            TubeRenderer.RenderTube(tubePoints, Space.World);
        }

        public void HideTubeRenderer()
        {
            TubeRenderer.Hide();
        }

        public void SetRadius(float radius)
        {
            TubeRenderer.Radius = radius;
        }
        
        public void SetStartFade(float startFade)
        {
            var fade = Mathf.Clamp01(startFade);
            
            TubeRenderer.StartFadeThreshold = fade;
        }
        
        public void SetEndFade(float endFade)
        {
            var fade = Mathf.Clamp01(endFade);
            
            TubeRenderer.EndFadeThreshold = fade;
        }
        
        #endregion
        
        #region Private Methods

        private void PointRay(Vector3 start, Vector3 middle, Vector3 end)
        {
            var tmpPoints = new Vector3[PointCount];

            for (var i = 0; i < PointCount; i++)
            {
                var t = i / (PointCount - 1f);
                var point = EvaluateBezier(start, middle, end, t);
                tmpPoints[i] = point;
            }

            if (tubePoints == null || tubePoints.Length < tmpPoints.Length)
            {
                tubePoints = new TubePoint[tmpPoints.Length];
            }

            var totalLength = 0f;
            for (var i = 1; i < tmpPoints.Length; i++)
            {
                totalLength += (tmpPoints[i] - tmpPoints[i - 1]).magnitude;
            }

            for (var i = 0; i < tmpPoints.Length; i++)
            {
                var difference = i == 0 ? tmpPoints[i + 1] - tmpPoints[i] : tmpPoints[i] - tmpPoints[i - 1];
                tubePoints[i].position = tmpPoints[i];
                tubePoints[i].rotation = Quaternion.LookRotation(difference);
                tubePoints[i].relativeLength = i == 0 ? 0f : tubePoints[i - 1].relativeLength + (difference.magnitude / totalLength);
            }
        }

        private Vector3 EvaluateBezier(Vector3 start, Vector3 middle, Vector3 end, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return (oneMinusT * oneMinusT * start)
                   + (2f * oneMinusT * t * middle)
                   + (t * t * end);
        }

        /// <summary>
        /// 베지어 곡선을 그리기 위한 지점들 계산
        /// </summary>
        /// <param name="pointsCount">지점 갯수</param>
        /// <param name="controlPoints">계산하는 지점들</param>
        /// <returns></returns>
        private List<Vector3> GenerateBezierPoints(int pointsCount, List<Vector3> controlPoints)
        {
            calculatedPoints.Clear();
            var stepSize = pointsCount != 1 ? 1f / (pointsCount - 1) : pointsCount;

            for (var i = 0; i < pointsCount; i++)
            {
                var calculatedPoint = GenerateBezierPoint(controlPoints, i * stepSize);
                calculatedPoints.Add(calculatedPoint);
            }

            return calculatedPoints;
        }

        /// <summary>
        /// 베지어 곡선을 그리기 위한 지점 계산
        /// </summary>
        /// <param name="controlPoints">계산하는 지점들</param>
        /// <param name="pointLocation">지점의 순번</param>
        /// <returns></returns>
        private Vector3 GenerateBezierPoint(List<Vector3> controlPoints, float pointLocation)
        {
            int index;
            if (pointLocation >= 1f)
            {
                pointLocation = 1f;
                index = controlPoints.Count - 4;
            }
            else
            {
                pointLocation = Mathf.Clamp01(pointLocation) * ((controlPoints.Count - 1) / 3f);
                index = (int)pointLocation;
                pointLocation -= index;
                index *= 3;
            }

            var normalizedPointLocation = Mathf.Clamp01(pointLocation);
            var oneMinusT = 1f - normalizedPointLocation;
            return oneMinusT * oneMinusT * oneMinusT * controlPoints[index] + 3f * oneMinusT * oneMinusT * normalizedPointLocation * controlPoints[index + 1] +
                   3f * oneMinusT * normalizedPointLocation * normalizedPointLocation * controlPoints[index + 2] +
                   normalizedPointLocation * normalizedPointLocation * normalizedPointLocation * controlPoints[index + 3];
        }

        #endregion
    }
}