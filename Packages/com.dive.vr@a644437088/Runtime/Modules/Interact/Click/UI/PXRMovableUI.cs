using System;
using System.Threading;
using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dive.VRModule
{
    public partial class PXRMovableUI : PXRClickable
    {
        #region Public Methods

        public void SetStaticUI(PXRMovableStaticUI targetStaticUI, bool needReset = false)
        {
            if (needReset && staticUI != null)
            {
                if (moveSequence != null && moveSequence.IsActive())
                {
                    moveSequence.Kill();
                    moveSequence = null;
                }

                staticUI.DisableCollider();
            }
            
            staticUI = targetStaticUI;
        }

        #endregion
        
        #region Private Methods

        protected override void Awake()
        {
            base.Awake();
            billboards = GetComponentsInChildren<PXRUIBillboard>();
            leftController = PXRInputBridge.LeftController.Controller;
            rightController = PXRInputBridge.RightController.Controller;
            menu = GetComponentInChildren<PXRMovableUIMenu>();
            menu.Initialize(this);

            UpEvent += OnUpEvent;
            DownEvent += OnDownEvent;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyEvent();
            
            if (cts is {IsCancellationRequested: true})
                return;

            cts?.Cancel();
            cts?.Dispose();
        }

        private async void OnDownEvent(PXRClickable clickable, HandSide handSide)
        {
            if(moveSequence != null && moveSequence.IsActive())
            {
                moveSequence.Kill();
                moveSequence = null;
            }

            if (staticUI != null)
            {
                var lookAt = false;
                
                moveSequence = DOTween.Sequence();
                moveSequence.Append(transform.DODynamicLookAt(PXRRig.PlayerController.CenterEye.position, Time.deltaTime * 10)).OnComplete(() =>
                {
                    lookAt = true;
                });

                // await UniTask.WaitUntil(() => lookAt);

                foreach (var billboard in billboards)
                {
                    billboard.enabled = true;
                }
                
                staticUI.EnableCollider();
            }            
            
            originRootParent = transform.parent;
            originMoveParent = moveTarget.parent;
            originMovePosition = moveTarget.localPosition;

            moveTarget.SetParent(null);
            transform.SetParent(moveTarget);

            clicker = handSide == HandSide.Left ? PXRRig.LeftClicker : PXRRig.RightClicker;
            currentController = handSide == HandSide.Left ? leftController : rightController;

            startClickerPosition = clicker.LinePosition;
            startTargetPosition = moveTarget.position;

            cts = new CancellationTokenSource();
            
            tubeRenderer = clicker.TubeRendererController;
            tubeRenderer.SetRadius(lineRadius);
            // ExecuteMoveTask(cts.Token).Forget();
        }

        private void OnUpEvent(PXRClickable clickable, HandSide handSide)
        {
            if (!cts.IsCancellationRequested)
            {
                cts?.Cancel();
                cts?.Dispose();
            }
            
            tubeRenderer.HideTubeRenderer();
            menu.OnPointerExit(null);

            transform.SetParent(null);
            moveTarget.SetParent(originMoveParent);
            moveTarget.localPosition = originMovePosition;
            transform.SetParent(originRootParent);

            originRootParent = null;
            
            if (staticUI == null)
                return;
            
            if(moveSequence != null && moveSequence.IsActive())
            {
                moveSequence.Kill();
                moveSequence = null;
                
                foreach (var billboard in billboards)
                {
                    billboard.enabled = true;
                }
                
                staticUI.EnableCollider();
            }
            
            var colliders = Physics.OverlapSphere(transform.position, colliderSize, 1 << PXRNameToLayer.UILayer);

            if (colliders is not {Length: > 0})
            {
                staticUI.DisableCollider();
                return;
            }

            foreach (var coll in colliders)
            {
                var tmpUI = coll.GetComponent<PXRMovableStaticUI>();

                if (tmpUI.GetHashCode() != staticUI.GetHashCode()) 
                    continue;
                    
                foreach (var billboard in billboards)
                {
                    billboard.enabled = false;
                    billboard.SetOrigin();
                }
                
                moveSequence = DOTween.Sequence();
                moveSequence.Append(transform.DOMove(staticUI.CenterPosition, 0.5f));
                moveSequence.Join(transform.DORotateQuaternion(staticUI.LookRotation, 0.5f));

                moveSequence.Play();
                break;
            }

            staticUI.DisableCollider();
        }

        private void OnDestroyEvent()
        {
            if(tubeRenderer != null)
                tubeRenderer.HideTubeRenderer();
        }
        
        private async Task ExecuteMoveTask(CancellationToken token)
        {
            var maxDistance = PXRInputModule.MaxRaycastLength.Value;
            var curvedValue = maxDistance * 2;

            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                var position = moveTarget.position;
                var clickerPosition = clicker.LinePosition;

                var startDist = Vector3.Distance(clickerPosition, startClickerPosition) * speed;
                var dir = (clickerPosition - startClickerPosition).normalized;
                var targetPosition = startTargetPosition + dir * startDist;

                var targetDist = Vector3.Distance(clickerPosition, targetPosition);
                var preTargetDist = Vector3.Distance(clickerPosition, preTargetPosition);
                var curvedT = Mathf.Clamp01(targetDist / curvedValue) * 0.5f;

                tubeRenderer.SetStartFade(curvedT);
                
                var abs = Mathf.Abs(targetDist - preTargetDist);
                
                if(abs > 0.01f)
                {
                    preTargetPosition = moveTarget.position;
                }
                else
                {
                    abs = 0;
                }
                
                // abs 크기를 제한 0 < abs <  0.2
                abs = Mathf.Clamp(abs, 0, 0.2f);
                
                var middle = Vector3.Lerp(clickerPosition, position, curvedT) + clicker.LineUpDirection.normalized * abs;
                
                if (targetDist < maxDistance)
                {
                    // 8방향 상태를 확인
                    var angle = CalculateAngle(preTargetPosition, targetPosition);
                    
                    var (pointA, pointB, pointC, pointD) = GetRelatedAnglePoint(angle, preTargetDist - targetDist);

                    var length1 = Physics.OverlapBoxNonAlloc(pointA, Vector3.one * colliderSize, colliderInfos[0].Colliders, Quaternion.identity, ~ignoreLayer);
                    var length2 = Physics.OverlapBoxNonAlloc(pointB, Vector3.one * colliderSize, colliderInfos[1].Colliders, Quaternion.identity, ~ignoreLayer);
                    var length3 = Physics.OverlapBoxNonAlloc(pointC, Vector3.one * colliderSize, colliderInfos[2].Colliders, Quaternion.identity, ~ignoreLayer);
                    var length4 = Physics.OverlapBoxNonAlloc(pointD, Vector3.one * colliderSize, colliderInfos[3].Colliders, Quaternion.identity, ~ignoreLayer);
                    
                    if (length1 + length2 + length3 + length4 == 0)
                    {
                        moveTarget.position = Vector3.Lerp(moveTarget.position, targetPosition, 0.1f);
                    }
                }

                tubeRenderer.ShowTubeRenderer(clickerPosition, middle, menu.Position);

                // await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }

            // await UniTask.CompletedTask;
        }
        
        private static float CalculateAngle(Vector3 from, Vector3 to)
        {
            return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
        }

        private (Vector3, Vector3, Vector3, Vector3) GetRelatedAnglePoint(float angle, float dist)
        {
            // 8방향 체크
            var index = (int)angle / 45;

            switch (index)
            {
                case 0:
                    return dist > 0 ? (UpForward, Forward, RightUpForward, RightForward) : (UpBackward, Backward, RightUpBackward, RightBackward);
                case 1:
                    return dist > 0 ? (UpForward, RightUp, Right, RightForward) : (UpBackward, RightUp, Right, RightBackward);
                case 2:
                    return dist > 0 ? (RightForward, Right, RightDown, RightUpForward) : (RightBackward, Right, RightDown, RightUpBackward);
                case 3:
                    return dist > 0 ? (RightDownForward, RightDown, Down, DownForward) : (RightDownBackward, RightDown, Down, DownBackward);
                case 4:
                    return dist > 0 ? (DownForward, LeftDownForward, Down, LeftDown) : (DownBackward, LeftDownBackward, Down, LeftDown);
                case 5:
                    return dist > 0 ? (LeftDownForward, LeftForward, Left, LeftDown) : (LeftDownBackward, LeftBackward, Left, LeftDown);
                case 6:
                    return dist > 0 ? (LeftForward, LeftUpForward, Left, LeftUp) : (LeftBackward, LeftUpBackward, Left, LeftUp);
                case 7:
                    return dist > 0 ? (Up, UpForward, LeftUp, LeftUpForward) : (Up, UpBackward, LeftUp, LeftUpBackward);
                default:
                    return (Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!isShowGizmo)
                return;

            if (!IsClicked)
                return;

            if (currentController == null)
                return;

            Gizmos.color = Color.red;

            var clickerPosition = currentController.GetControllerPosition();
            var startDist = Vector3.Distance(clickerPosition, startClickerPosition) * speed;
            var dir = (clickerPosition - startClickerPosition).normalized;
            var targetPosition = startTargetPosition + dir * startDist;
            var targetDist = Vector3.Distance(clickerPosition, targetPosition);

            var preTargetDist = Vector3.Distance(clickerPosition, preTargetPosition);

            // 8방향 상태를 확인
            var angle = CalculateAngle(preTargetPosition, targetPosition);
            var (pointA, pointB, pointC, pointD) = GetRelatedAnglePoint(angle, targetDist - preTargetDist);

            Gizmos.DrawWireCube(pointA, Vector3.one * colliderSize);
            Gizmos.DrawWireCube(pointB, Vector3.one * colliderSize);
            Gizmos.DrawWireCube(pointC, Vector3.one * colliderSize);
            Gizmos.DrawWireCube(pointD, Vector3.one * colliderSize);

            Gizmos.DrawSphere(preTargetPosition, colliderSize / 4);
            Gizmos.DrawSphere(targetPosition, colliderSize / 4);
        }

        private void OnValidate()
        {
            if (colliderInfos == null || colliderInfos.Length == 0 || center == null)
                return;
            
            var leftUp = colliderInfos[0].Transform;
            var rightUp = colliderInfos[1].Transform;
            var leftDown = colliderInfos[2].Transform;
            var rightDown = colliderInfos[3].Transform;
            
            leftUp.position = center.position + uiWidth * 0.5f * center.right.normalized + uiHeight * 0.5f * center.up.normalized;
            rightUp.position = center.position + uiWidth * 0.5f * center.right.normalized - uiHeight * 0.5f * center.up.normalized;
            leftDown.position = center.position - uiWidth * 0.5f * center.right.normalized + uiHeight * 0.5f * center.up.normalized;
            rightDown.position = center.position - uiWidth * 0.5f * center.right.normalized - uiHeight * 0.5f * center.up.normalized;
        }

        private void OnDrawGizmosSelected()
        {
            if (!isShowGizmo)
                return;

            Gizmos.color = Color.green;

            var pointArray = new Vector3[28];

            // 직육면체로 그리기

            // 앞면
            pointArray[0] = colliderInfos[0].Position + colliderInfos[0].Forward * uiDepth;
            pointArray[1] = colliderInfos[1].Position + colliderInfos[1].Forward * uiDepth;
            pointArray[2] = colliderInfos[1].Position + colliderInfos[1].Backward * uiDepth;
            pointArray[3] = colliderInfos[3].Position + colliderInfos[3].Backward * uiDepth;
            pointArray[4] = colliderInfos[3].Position + colliderInfos[3].Forward * uiDepth;
            pointArray[5] = colliderInfos[2].Position + colliderInfos[2].Forward * uiDepth;
            pointArray[6] = colliderInfos[2].Position + colliderInfos[2].Backward * uiDepth;
            pointArray[7] = colliderInfos[0].Position + colliderInfos[0].Backward * uiDepth;

            // 뒷면
            pointArray[8] = colliderInfos[0].Position - colliderInfos[0].Forward * uiDepth;
            pointArray[9] = colliderInfos[1].Position - colliderInfos[1].Forward * uiDepth;
            pointArray[10] = colliderInfos[1].Position - colliderInfos[1].Backward * uiDepth;
            pointArray[11] = colliderInfos[3].Position - colliderInfos[3].Backward * uiDepth;
            pointArray[12] = colliderInfos[3].Position - colliderInfos[3].Forward * uiDepth;
            pointArray[13] = colliderInfos[2].Position - colliderInfos[2].Forward * uiDepth;
            pointArray[14] = colliderInfos[2].Position - colliderInfos[2].Backward * uiDepth;
            pointArray[15] = colliderInfos[0].Position - colliderInfos[0].Backward * uiDepth;

            // 옆면
            pointArray[16] = pointArray[0];
            pointArray[17] = pointArray[8];
            pointArray[18] = pointArray[9];
            pointArray[19] = pointArray[1];
            pointArray[20] = pointArray[2];
            pointArray[21] = pointArray[10];
            pointArray[22] = pointArray[11];
            pointArray[23] = pointArray[3];
            pointArray[24] = pointArray[4];
            pointArray[25] = pointArray[12];
            pointArray[26] = pointArray[13];
            pointArray[27] = pointArray[5];

            Gizmos.DrawLineList(pointArray);
        }

#endif        
        
        #endregion
    }
}