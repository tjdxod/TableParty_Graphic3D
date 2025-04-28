using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    /// <summary>
    /// 텔레포트 LineRenderer 상태 enum
    /// </summary>
    public enum TeleportLineState
    {
        Default,
        Enable,
        Disable,
        MarkerOff,
        Deactivate
    }

    public partial class PXRTeleporter
    {
        #region Private Fields

        [Header("텔레포트")]
        [SerializeField, LabelText("Ray의 최소거리")]
        private float minTeleportRayLength = 6f;

        [SerializeField, LabelText("Ray의 최대거리")]
        private float maxTeleportRayLength = 15f;

        [Header("포인트 텔레포트")]
        [SerializeField, LabelText("Tube 렌더러")]
        private PXRTubeRenderer tubeRenderer;

        [SerializeField]
        private PXRTeleporterLineRendererController lineRendererController;
        private IEnumerator routineScrollTexture;

        private PXRTeleportSpaceBase currentHitTeleportArea;
        private PXRTeleportSpaceBase currentHitTeleportPoint;

        // ReSharper disable once NotAccessedField.Local
        private Vector3 currentHitTeleportPointPosition;

        private List<Vector3> curvePoints = new List<Vector3>();
        private readonly List<Vector3> linePoints = new List<Vector3>();
        private readonly List<Vector3> calculatedPoints = new List<Vector3>();

        private TubePoint[] tubePoints;

        private TeleportLineState teleportLineState;

        private RaycastHit? targetHit;
        private Vector3? markerDirectionOverride;
        private Vector3? destinationPointOverride;

        // 곡선 충돌 체크할 때 사용하는 값
        private int collisionCheckFrequency = 10;

        // 베지어 곡선을 그리기 위한 점의 개수
        private const int SegmentCount = 50;
        private const int PointCount = 50;
        private const float CurveOffset = 0.1f;

        // 최종 텔레포트 지점에서 뒤로가는 거리
        private const float AdjustmentOffset = 0.3f;

        #endregion

        #region Private Properties

        private PXRTeleporterLineRendererController LineRendererController
        {
            get
            {
                if (lineRendererController != null)
                    return lineRendererController;

                lineRendererController = GetComponent<PXRTeleporterLineRendererController>();

                if (lineRendererController == null)
                    Debug.LogWarning("LineRendererController is null");

                return lineRendererController;
            }
        }

        #endregion

        #region Private Methods

        private void Awake()
        {
            if (teleporterOrigin == null)
                teleporterOrigin = transform;

            LineRendererController.SetPositionCount(SegmentCount);
        }

        private void ExecuteRay()
        {
            var start = teleporterOrigin.position;
            var direction = teleporterOrigin.forward;
            var forward = ProjectForward();
            var down = ProjectDown(forward);
            var end = currentHitTeleportPoint == null ? down : currentHitTeleportPoint.transform.position;
            var middle = start + direction * (Vector3.Distance(start, end) * 0.5f);
            
            if (currentHitTeleportPoint != null)
            {
                if (!currentHitTeleportPoint.CanTeleport)
                {
                    tubeRenderer.Hide();
                    tubeRenderer.Tint = LineRendererController.ColorDisableTeleport;

                    LineRendererController.EnableLineRenderer();
                    down.y = currentHitTeleportPoint.GetSpaceMinY();
                    GeneratePoints(forward, down);
                    ChangeTeleportVisual(TeleportLineState.Disable);
                }
                else
                {
                    LineRendererController.DisableLineRenderer();

                    PointRay(start, middle, end);
                    tubeRenderer.Tint = LineRendererController.ColorEnableTeleport;
                    tubeRenderer.RenderTube(tubePoints, Space.World);

                    ChangeTeleportVisual(TeleportLineState.MarkerOff);
                    teleportMarker.transform.position = end;
                    RotateMarkerDirection();
                    teleportMarker.Activate();
                }
            }
            else
            {
                if (currentHitTeleportArea != null)
                {
                    if (!currentHitTeleportArea.CanTeleport)
                    {
                        tubeRenderer.Hide();
                        tubeRenderer.Tint = LineRendererController.ColorDisableTeleport;
                        LineRendererController.EnableLineRenderer();
                        GeneratePointsIncludingSegments(forward, end);
                        RotateMarkerDirection();
                        teleportMarker.Inactivate();
                    }
                    else
                    {
                        tubeRenderer.Hide();
                        tubeRenderer.Tint = LineRendererController.ColorDisableTeleport;

                        LineRendererController.EnableLineRenderer();
                        GeneratePointsIncludingSegments(forward, end);
                        RotateMarkerDirection();
                        ChangeTeleportVisual(TeleportLineState.Enable);
                        teleportMarker.Activate();
                    }
                }
                else
                {
                    tubeRenderer.Hide();
                    tubeRenderer.Tint = LineRendererController.ColorDisableTeleport;

                    LineRendererController.EnableLineRenderer();
                    GeneratePointsIncludingSegments(forward, end);
                    RotateMarkerDirection();
                    teleportMarker.Activate();
                }
            }
        }

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

        /// <summary>
        /// 직선 레이캐스트 계산
        /// </summary>
        /// <returns></returns>
        private Vector3 ProjectForward()
        {
            var length = minTeleportRayLength;
            var rotation = Vector3.Dot(Vector3.up, teleporterOrigin.forward.normalized);

            if (rotation * 100f > HeightLimitAngle)
            {
                var controllerRotationOffset = 1f - (rotation - HeightLimitAngle / 100f);
                length = minTeleportRayLength * controllerRotationOffset * controllerRotationOffset;
            }

            var ray = new Ray(teleporterOrigin.position, teleporterOrigin.forward);
            var hasCollided = Physics.Raycast(ray, out RaycastHit hitData, length, 1 << TeleportLayer);
            var anythingCollided = Physics.Raycast(ray, out RaycastHit anything, maxTeleportRayLength);
            
            if (anythingCollided && !hasCollided)
            {
                targetHit = null;

                destinationPointOverride = null;
                markerDirectionOverride = null;
                currentHitTeleportArea = null;
                currentHitTeleportPoint = null;
                currentHitTeleportPointPosition = Vector3.zero;
                
                return ray.GetPoint(Vector3.Distance(ray.origin, anything.point) - AdjustmentOffset);
            }
            
            if (hasCollided)
            {
                var teleportSpace = hitData.transform.GetComponent<ITeleportSpace>();

                if (teleportSpace == null)
                    return ray.GetPoint(length) + (Vector3.up * AdjustmentOffset);

                targetHit = hitData;

                if (teleportSpace.GetOriginalTeleportSpace().SpaceType == SpaceType.Point)
                {
                    currentHitTeleportArea = null;
                    
                    if (currentHitTeleportPoint == null)
                    {
                        currentHitTeleportPoint = teleportSpace.GetOriginalTeleportSpace();
                        currentHitTeleportPointPosition = hitData.point;
                    }

                    if (teleportSpace.GetOriginalTeleportSpace().UseFixedDirection)
                    {
                        destinationPointOverride = teleportSpace.GetOriginalTeleportSpace().UseForceTransform ? 
                            teleportSpace.GetOriginalTeleportSpace().ForceTransform.position : teleportSpace.GetOriginalTeleportSpace().transform.position;
                        markerDirectionOverride = teleportSpace.GetOriginalTeleportSpace().UseForceTransform ? 
                            teleportSpace.GetOriginalTeleportSpace().ForceTransform.forward : teleportSpace.GetOriginalTeleportSpace().transform.forward;

                        return ray.GetPoint(Vector3.Distance(ray.origin, hitData.point)) + Vector3.up * AdjustmentOffset;
                    }

                    destinationPointOverride = teleportSpace.GetOriginalTeleportSpace().UseForceTransform ? teleportSpace.GetOriginalTeleportSpace().ForceTransform.position : null;
                    return ray.GetPoint(Vector3.Distance(ray.origin, hitData.point)) + Vector3.up * AdjustmentOffset;
                }

                if (!(hitData.distance < length))
                {
                    return ray.GetPoint(length) + (Vector3.up * AdjustmentOffset);
                }

                currentHitTeleportArea = teleportSpace.GetTeleportSpace();
                
                length = hitData.distance;

                destinationPointOverride = null;
                markerDirectionOverride = null;
                currentHitTeleportPoint = null;
                currentHitTeleportPointPosition = Vector3.zero;
            }
            else
            {
                destinationPointOverride = null;
                markerDirectionOverride = null;
                currentHitTeleportArea = null;
                currentHitTeleportPoint = null;
                currentHitTeleportPointPosition = Vector3.zero;
            }

            return ray.GetPoint(length) + (Vector3.up * AdjustmentOffset);
        }


        /// <summary>
        /// Forward에서 아래방향 레이캐스트
        /// </summary>
        /// <param name="downwardOrigin">아래 방향</param>
        /// <returns></returns>
        private Vector3 ProjectDown(Vector3 downwardOrigin)
        {
            if (destinationPointOverride != null)
                return Vector3.zero;

            var ray = new Ray(downwardOrigin, Vector3.down);

            var downRayHit = Physics.Raycast(ray, out var hitData, maxTeleportRayLength, 1 << TeleportLayer);
            var anythingCollided = Physics.Raycast(ray, out RaycastHit anything, maxTeleportRayLength * 2);
            
            if (anythingCollided && !downRayHit)
            {
                targetHit = null;
                return ray.GetPoint(Vector3.Distance(ray.origin, anything.point));
            }

            if (anythingCollided && downRayHit)
            {
                targetHit = null;
            }
            
            if (!downRayHit)
            {
                targetHit = null;
                return ray.GetPoint(0f);
            }

            var teleportSpace = hitData.transform.GetComponent<ITeleportSpace>();

            if (teleportSpace == null)
            {
                targetHit = null;
                return ray.GetPoint(hitData.distance);
            }

            if (currentHitTeleportPoint == null)
                targetHit = hitData;

            if (teleportSpace.SpaceType != SpaceType.Point)
                return ray.GetPoint(hitData.distance);

            if (!teleportSpace.UseFixedDirection)
                return ray.GetPoint(Vector3.Distance(ray.origin, hitData.point));

            destinationPointOverride = teleportSpace.UseForceTransform ? teleportSpace.ForceTransform.position : teleportSpace.GetTeleportSpace().transform.position;
            markerDirectionOverride = teleportSpace.UseForceTransform ? teleportSpace.ForceTransform.forward : teleportSpace.GetTeleportSpace().transform.forward;

            return ray.GetPoint(Vector3.Distance(ray.origin, hitData.point));
        }

        private void GeneratePointsIncludingSegments(Vector3 forward, Vector3 down)
        {
            GeneratePoints(forward, down);

            if (destinationPointOverride != null)
            {
                ChangeTeleportVisual(TeleportLineState.MarkerOff);
                return;
            }

            markerDirectionOverride = null;

            // 직선 레이캐스트가 아닌 포물선에 오브젝트가 닿았을 때
            collisionCheckFrequency = Mathf.Clamp(collisionCheckFrequency, 0, SegmentCount);
            var step = SegmentCount / (collisionCheckFrequency > 0 ? collisionCheckFrequency : 1);

            for (var i = 0; i < SegmentCount - step; i += step)
            {
                var currentPoint = linePoints[i];
                var nextPoint = i + step < linePoints.Count ? linePoints[i + step] : linePoints[^1];
                var nextPointDirection = (nextPoint - currentPoint).normalized;
                var nextPointDistance = Vector3.Distance(currentPoint, nextPoint);

                var pointsRay = new Ray(currentPoint, nextPointDirection);

                // 점과 점 사이에 레이캐스트를 쏴서 오브젝트가 있는지 판단
                if (!Physics.Raycast(pointsRay, out var forwardHitData, nextPointDistance, 1 << TeleportLayer))
                    continue;

                var teleportSpace = forwardHitData.transform.GetComponent<ITeleportSpace>();

                if (teleportSpace == null)
                    continue;

                var collisionPoint = pointsRay.GetPoint(forwardHitData.distance);
                var downwardRay = new Ray(collisionPoint + Vector3.up * 0.01f, Vector3.down);

                if (teleportSpace.GetTeleportSpace().SpaceType == SpaceType.Point)
                {
                    if (!teleportSpace.GetTeleportSpace().CanTeleport)
                        return;

                    if (currentHitTeleportPoint == null)
                    {
                        currentHitTeleportPoint = teleportSpace.GetTeleportSpace();
                    }
                    
                    if (teleportSpace.GetTeleportSpace().UseFixedDirection)
                    {
                        destinationPointOverride = teleportSpace.GetTeleportSpace().UseForceTransform ? teleportSpace.GetTeleportSpace().ForceTransform.position : teleportSpace.GetTeleportSpace().transform.position;
                        markerDirectionOverride = teleportSpace.GetTeleportSpace().UseForceTransform ? teleportSpace.GetTeleportSpace().ForceTransform.forward : teleportSpace.GetTeleportSpace().transform.forward;

                        GeneratePoints(forward, down);
                        ChangeTeleportVisual(TeleportLineState.MarkerOff);
                        return;
                    }

                    destinationPointOverride = teleportSpace.GetTeleportSpace().UseForceTransform ? teleportSpace.GetTeleportSpace().ForceTransform.position : null;

                    GeneratePoints(forward, down);
                    ChangeTeleportVisual(TeleportLineState.MarkerOff);
                    return;
                }

                // 아래방향으로 레이캐스트를 쏴서 바닥과 닿는지 판단
                if (!Physics.Raycast(downwardRay, out var downwardHitData, float.PositiveInfinity, 1 << TeleportLayer))
                {
                    targetHit = null;
                    continue;
                }

                targetHit = downwardHitData;

                var newDownPosition = downwardRay.GetPoint(downwardHitData.distance);
                var newJointPosition = newDownPosition.y < forward.y ? new Vector3(newDownPosition.x, forward.y, newDownPosition.z) : forward;

                GeneratePoints(newJointPosition, newDownPosition);

                break;
            }

            if (targetHit != null && targetHit.Value.transform.gameObject.layer != TeleportLayer)
            {
                ChangeTeleportVisual(TeleportLineState.Disable);
            }
            else if (targetHit == null)
            {
                ChangeTeleportVisual(TeleportLineState.Disable);
            }
            else if (targetHit != null)
            {
                var tp = targetHit.Value.transform.GetComponent<ITeleportSpace>();

                if (tp != null && tp.GetTeleportSpace().SpaceType == SpaceType.Area )
                {
                    if (!tp.GetTeleportSpace().CanTeleport)
                    {
                        ChangeTeleportVisual(TeleportLineState.Disable);
                    }
                    else
                    {
                        ChangeTeleportVisual(TeleportLineState.Enable);
                    }
                }
                else
                {
                    ChangeTeleportVisual(TeleportLineState.Enable);
                }
            }
        }

        private void GeneratePoints(Vector3 forward, Vector3 down)
        {
            forward = destinationPointOverride ?? forward;
            down = destinationPointOverride ?? down;

            if (curvePoints == null || curvePoints.Count != 4)
            {
                curvePoints = new List<Vector3>(4);
                curvePoints.AddRange(new Vector3[4]);
            }

            curvePoints[0] = teleporterOrigin.transform.position;
            curvePoints[1] = forward + Vector3.up * CurveOffset;
            curvePoints[2] = down;
            curvePoints[3] = down;

            linePoints.Clear();
            foreach (Vector3 generatedPoint in GenerateBezierPoints(SegmentCount, curvePoints))
            {
                linePoints.Add(generatedPoint);
            }

            lineRendererController.SetPositions(linePoints.ToArray());
            teleportMarker.transform.position = down;
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