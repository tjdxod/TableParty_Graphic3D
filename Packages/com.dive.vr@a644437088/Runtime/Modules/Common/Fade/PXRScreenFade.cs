using System.Collections;
using Dive.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Dive.VRModule
{
    /// <summary>
    /// 카메라의 Fade in / out을 관리
    /// </summary>
    public class PXRScreenFade : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// 싱글톤
        /// </summary>
        public static PXRScreenFade Instance
        {
            get
            {
                if (instance != null)
                    return instance.Value;

                var fade = FindObjectOfType<PXRScreenFade>();

                if (fade == null)
                    return null;

                instance = new StaticVar<PXRScreenFade>(fade);

                return instance.Value;
            }
        }

        #endregion

        #region Private Fields

        private static StaticVar<PXRScreenFade> instance;

        /// <summary>
        /// 제일 앞쪽 Fade (Scene 이동용)
        /// </summary>
        [SerializeField, Tooltip("제일 앞의 Fade (씬 이동용)")]
        private Image imgSceneFade;

        /// <summary>
        /// 카메라에 붙어 있는 Fade
        /// </summary>
        [SerializeField, Tooltip("카메라에 붙어있는 FadeImage")]
        private Image imgCameraFade;

        /// <summary>
        /// 스턴 시 카메라에 보이는 이미지
        /// </summary>
        [SerializeField, Tooltip("스턴 이미지")]
        private RectTransform rectTransformStun;

        /// <summary>
        /// 시작 시 설정되는 alpha 값
        /// </summary>
        [SerializeField, Tooltip("시작할 때 Fader의 알파 값")]
        private float initAlpha;

        private IEnumerator routineDoFade;
        private IEnumerator routineDoFrontFade;
        private IEnumerator routineStun;

        #endregion

        #region Public Properties

        /// <summary>
        /// 기절상태인경우 true, 그렇지 않은 경우 false
        /// </summary>
        public bool IsStunning { get; private set; }

        /// <summary>
        /// 페이드 진행중인 경우 true, 그렇지 않은 경우 false
        /// </summary>
        public bool IsCameraFading { get; private set; } // Fade를 하고 있는지 나타내는 변수

        /// <summary>
        /// Scene 이동 중에 페이드 진행중인 경우 true, 그렇지 않은 경우 false
        /// </summary>
        public bool IsSceneFading { get; private set; }

        #endregion

        #region Public Methods

        public static void ResetFadeValues()
        {
            if (Instance == null)
                return;

            Instance.IsCameraFading = false;
            Instance.IsSceneFading = false;
            Instance.IsStunning = false;
        }
        
        /// <summary>
        /// 수치에 따라 Fade in / out 일반 페이드 실행
        /// </summary>
        /// <param name="endAlpha">0 ~ 1까지의 알파값</param>
        /// <param name="fadeTime">Fade가 끝날때까지의 시간</param>
        public static void StartCameraFade(float endAlpha, float fadeTime)
        {
            if (Instance == null)
                return;

            if (Instance.IsCameraFading)
                return;

            StopFade();
            Instance.routineDoFade = Instance.CoroutineDoFade(endAlpha, fadeTime);
            Instance.StartCoroutine(Instance.routineDoFade);
        }

        /// <summary>
        /// 일반 페이드 정지
        /// </summary>
        public static void StopFade()
        {
            if (Instance == null)
                return;
            
            if (Instance.routineDoFade == null)
                return;

            Instance.StopCoroutine(Instance.routineDoFade);
            Instance.routineDoFade = null;
            Instance.IsCameraFading = false;
        }

        /// <summary>
        /// 수치에 따라 Fade in / out Scene 페이드 실행
        /// </summary>
        /// <param name="endAlpha">0 ~ 1까지의 알파값</param>
        /// <param name="fadeTime">Fade가 끝날때까지의 시간</param>
        public static void StartSceneFade(float endAlpha, float fadeTime)
        {
            StopFrontFade();

            if (Instance == null)
                return;
            
            if (!Mathf.Approximately(Instance.imgCameraFade.color.a, 0))
                Instance.imgCameraFade.ClearAlpha();

            Instance.routineDoFrontFade = Instance.CoroutineDoFrontFade(endAlpha, fadeTime);
            Instance.StartCoroutine(Instance.routineDoFrontFade);
        }

        /// <summary>
        /// Scene 페이드 정지
        /// </summary>
        public static void StopFrontFade()
        {
            if (Instance == null)
                return;
            
            if (Instance.routineDoFrontFade == null)
                return;

            Instance.StopCoroutine(Instance.routineDoFrontFade);
            Instance.routineDoFrontFade = null;
            Instance.IsSceneFading = false;
        }

        /// <summary>
        /// 사용자 아바타 기절
        /// </summary>
        /// <param name="size">이미지 사이즈</param>
        /// <param name="fadeTime">Fade가 끝날때까지의 시간</param>
        public static void StartStun(float size, float fadeTime)
        {
            if (Instance == null)
                return;
            
            if (Instance.routineStun != null)
            {
                Instance.StopCoroutine(Instance.routineStun);
                Instance.routineStun = null;
            }

            Instance.routineStun = CoroutineStun(size, fadeTime);
            Instance.StartCoroutine(Instance.routineStun);
        }

        /// <summary>
        /// 스턴 이미지 비활성화
        /// </summary>
        public static void DeactivateStunImage()
        {
            if (Instance == null)
                return;
            
            Instance.rectTransformStun.gameObject.SetActive(false);
        }

        /// <summary>
        /// 카메라 페이드 이미지 알파 값 Clear
        /// </summary>
        public static void ClearCameraImage()
        {
            if (Instance == null)
                return;
            
            var color = Instance.imgCameraFade.color;
            color.a = 0f;
            Instance.imgCameraFade.color = color;
        }

        /// <summary>
        /// 씬 페이드 이미지 알파 값 Clear
        /// </summary>
        public static void ClearSceneImage()
        {
            if (Instance == null)
                return;
            
            var color = Instance.imgSceneFade.color;
            color.a = 0f;
            Instance.imgSceneFade.color = color;
        }

        #endregion

        #region Private Methods

        private void Awake()
        {
            var initColor = imgCameraFade.color;
            initColor.a = initAlpha;

            imgCameraFade.color = initColor;
        }

        /// <summary>
        /// endAlpha에 따라 Fade In / Out 실행 할 수 있음 (0이면 FadeIn, 1이면 FadeOut)
        /// </summary>
        /// <param name="endAlpha">0 ~ 1까지의 알파값</param>
        /// <param name="fadeTime">Fade가 끝날때까지의 시간</param>
        /// <returns></returns>
        private IEnumerator CoroutineDoFade(float endAlpha, float fadeTime)
        {
            endAlpha = Mathf.Clamp01(endAlpha);

            IsCameraFading = true;

            float elapsedTime = 0f;
            float inverseFadeTime = 1 / fadeTime;
            float startAlpha = imgCameraFade.color.a;

            imgCameraFade.sprite = null;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;

                Color color = imgCameraFade.color;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime * inverseFadeTime);
                imgCameraFade.color = color;

                yield return null;
            }

            Color finalColor = imgCameraFade.color;
            finalColor.a = endAlpha;
            imgCameraFade.color = finalColor;

            IsCameraFading = false;
        }

        /// <summary>
        /// Scene 이동 Fade endAlpha에 따라 Fade In / Out 실행 할 수 있음 (0이면 FadeIn, 1이면 FadeOut)
        /// </summary>
        /// <param name="endAlpha">0 ~ 1까지의 알파값</param>
        /// <param name="fadeTime">Fade가 끝날때까지의 시간</param>
        /// <returns></returns>
        private IEnumerator CoroutineDoFrontFade(float endAlpha, float fadeTime)
        {
            endAlpha = Mathf.Clamp01(endAlpha);

            IsSceneFading = true;

            float elapsedTime = 0f;
            float inverseFadeTime = 1 / fadeTime;
            float startAlpha = imgSceneFade.color.a;

            imgSceneFade.sprite = null;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;

                Color color = imgSceneFade.color;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime * inverseFadeTime);
                imgSceneFade.color = color;

                yield return null;
            }

            Color finalColor = imgSceneFade.color;
            finalColor.a = endAlpha;
            imgSceneFade.color = finalColor;

            IsSceneFading = false;
        }

        /// <summary>
        /// 스턴이 걸린 상황의 Fade 동작
        /// </summary>
        /// <param name="size">이미지 사이즈</param>
        /// <param name="fadeTime">Fade가 끝날때까지의 시간</param>
        /// <returns></returns>
        private static IEnumerator CoroutineStun(float size, float fadeTime)
        {
            Vector2 targetSize = Vector2.one * size;

            Instance.rectTransformStun.gameObject.SetActive(true);

            Instance.IsStunning = true;

            float elapsedTime = 0f;
            float inverseFadeTime = 1f / fadeTime;

            var originSize = Instance.rectTransformStun.sizeDelta;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;

                Instance.rectTransformStun.sizeDelta = Vector2.Lerp(originSize, targetSize, elapsedTime * inverseFadeTime);

                yield return null;
            }

            Instance.IsStunning = false;
            Instance.rectTransformStun.sizeDelta = targetSize;
        }

        #endregion
    }
}