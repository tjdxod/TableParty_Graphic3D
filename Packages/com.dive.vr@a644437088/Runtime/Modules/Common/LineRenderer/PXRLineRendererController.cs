using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 컨트롤러의 LineRenderer 매니저
    /// </summary>
    public class PXRLineRendererController : MonoBehaviour
    {
        #region Private Fields

        private LineRenderer lineRenderer;

        private readonly Gradient defaultGradient = new Gradient();
        private readonly Gradient moveGradient = new Gradient();
        
        #endregion

        #region Public Properties

        /// <summary>
        /// 시작 위치 너비
        /// </summary>
        public float OriginStartWidth { get; private set; }

        /// <summary>
        /// 마지막 위치 너비
        /// </summary>
        public float OriginEndWidth { get; private set; }

        /// <summary>
        /// 현재 컨트롤러의 LineRenderer 반환
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public LineRenderer LineRenderer
        {
            get
            {
                if (!lineRenderer)
                    lineRenderer = GetComponent<LineRenderer>();

                return lineRenderer;
            }
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// LineRenderer 너비 수치 초기화
        /// </summary>
        protected void Awake()
        {
            OriginStartWidth = LineRenderer.startWidth;
            OriginEndWidth = LineRenderer.endWidth;

            moveGradient.mode = GradientMode.Blend;
            moveGradient.SetKeys(new[]
            {
                new GradientColorKey(new Color(0.15f, 0.85f, 0.93f), 0.0f),
                new GradientColorKey(new Color(0.15f, 0.85f, 0.93f), 0.2f),
                new GradientColorKey(new Color(0.15f, 0.85f, 0.93f), 1.0f)
            }, new[]
            {
                new GradientAlphaKey(0f, 0.0f),
                new GradientAlphaKey(0.4f, 0.2f),
                new GradientAlphaKey(0.0f, 1.0f)
            });
            
            defaultGradient.mode = GradientMode.Blend;
            defaultGradient.SetKeys(new []
            {
                new GradientColorKey(new Color(0.15f, 0.85f, 0.93f), 0.0f),
                new GradientColorKey(new Color(0.15f, 0.85f, 0.93f), 1.0f)
            }, new[]
            {
                new GradientAlphaKey(0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            });
        }

        /// <summary>
        /// LineRenderer 활성화
        /// </summary>
        public void EnableLineRenderer()
        {
            if (!LineRenderer.enabled)
                LineRenderer.enabled = true;
        }

        /// <summary>
        /// LineRenderer 비활성화
        /// </summary>
        public void DisableLineRenderer()
        {
            if (LineRenderer.enabled)
                LineRenderer.enabled = false;
        }

        /// <summary>
        /// LineRenderer 비활성화 및 위치 초기화
        /// </summary>
        /// <param name="originPosition">초기 위치</param>
        public void DisableLineAndClearAllPosition(Vector3 originPosition)
        {
            if (LineRenderer.enabled)
            {
                ClearAllPosition(originPosition);
                LineRenderer.enabled = false;
            }
        }

        /// <summary>
        /// LineRenderer 너비 세팅
        /// </summary>
        /// <param name="startWidth"></param>
        /// <param name="endWidth"></param>
        public void SetWidth(float startWidth, float endWidth)
        {
            LineRenderer.startWidth = startWidth;
            LineRenderer.endWidth = endWidth;
        }

        /// <summary>
        /// LineRenderer 너비를 초기 너비로 세팅
        /// </summary>
        public void SetOriginWidth()
        {
            LineRenderer.startWidth = OriginStartWidth;
            LineRenderer.endWidth = OriginEndWidth;
        }

        /// <summary>
        /// LineRenderer의 두 포인트를 세팅
        /// </summary>
        /// <param name="start">시작 위치</param>
        /// <param name="end">마지막 위치</param>
        public void SetTwoPoints(Vector3 start, Vector3 end)
        {
            LineRenderer.SetPosition(0, start);
            LineRenderer.SetPosition(1, (start + end) / 2);
            LineRenderer.SetPosition(2, end);
        }

        /// <summary>
        /// LineRenderer의 위치를 세팅
        /// </summary>
        /// <param name="positions">세팅 위치 배열</param>
        public void SetPositions(Vector3[] positions)
        {
            LineRenderer.SetPositions(positions);
        }

        /// <summary>
        /// 특정 인덱스의 위치를 지정
        /// </summary>
        /// <param name="index">특정 인덱스</param>
        /// <param name="position">위치</param>
        public void SetPosition(int index, Vector3 position)
        {
            LineRenderer.SetPosition(index, position);
        }
        
        /// <summary>
        /// LineRenderer의 포지션 카운트 설정
        /// </summary>
        /// <param name="count">포지션 카운트</param>
        public void SetPositionCount(int count)
        {
            LineRenderer.positionCount = count;
        }

        /// <summary>
        /// LineRenderer의 모든 포지션을 초기화
        /// </summary>
        /// <param name="originPosition">초기 위치</param>
        public void ClearAllPosition(Vector3 originPosition)
        {
            var count = LineRenderer.positionCount;

            for (var i = 0; i < count; i++)
            {
                LineRenderer.SetPosition(i, originPosition);
            }
        }
        
        /// <summary>
        /// LineRenderer의 그라데이션을 초기화
        /// </summary>
        public void SetDefaultGradient()
        {
            LineRenderer.colorGradient = defaultGradient;
        }
        
        /// <summary>
        /// LineRenderer의 그라데이션을 이동 상태로 변경
        /// </summary>
        public void SetMoveGradient()
        {
            LineRenderer.colorGradient = moveGradient;
        }

        #endregion
    }
}