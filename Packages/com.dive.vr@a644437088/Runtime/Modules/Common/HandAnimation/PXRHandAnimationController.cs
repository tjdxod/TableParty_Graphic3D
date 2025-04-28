using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    /// <summary>
    /// 손 애니메이션을 컨트롤
    /// </summary>
    public class PXRHandAnimationController : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// 현재 컨트롤러의 손 방향
        /// </summary>
        public HandSide controller;

        #endregion

        #region Private Fields

        private Animator animator;

        [SerializeField]
        private MeshFilter handSleeveMeshFilter;

        [SerializeField]
        private MeshRenderer handSleeveRenderer;
        
        [SerializeField]
        private Renderer handRenderer;
        
        [SerializeField, ReadOnly]
        private bool isThumbTouch;
        
        [SerializeField, ReadOnly]
        private bool isIndexTouch;

        [SerializeField, ReadOnly]
        private float flexParam = 0f;

        [SerializeField, ReadOnly]
        private float pinchParam = 0f;

        private bool isActiveController;

        private float thumbWeight = 1f;
        private float pointWeight = 1f;

        private int thumbAnimLayer = -1;
        // ReSharper disable once NotAccessedField.Local
        private int grabThumbAnimLayer = -1;
        private int grabIndexAnimLayer = -1;
        private int grabMiddleAnimLayer = -1;

        // ReSharper disable once NotAccessedField.Local
        private int animParamIndexPose = -1;

        private const float InputRate = 20.0f;

        private const string AnimLayerNameThumb = "Thumb Layer";
        private const string AnimLayerNameGrabThumb = "Grab Thumb Layer";
        private const string AnimLayerNameGrabIndex = "Grab Index Layer";
        private const string AnimLayerNameGrabMiddle = "Grab Middle Layer";
        private const string AnimParamNamePose = "Pose";

        private static readonly int HashFlex = Animator.StringToHash("Flex");
        private static readonly int Pinch = Animator.StringToHash("Pinch");
        private static readonly int IndexTouch = Animator.StringToHash("IndexTouch");

        #endregion

        #region Public Properties
        
        public MeshFilter HandSleeveMeshFilter => handSleeveMeshFilter;
        public MeshRenderer HandSleeveRenderer => handSleeveRenderer;
        public Renderer HandRenderer => handRenderer;
        public float ThumbLayerWeight => animator.GetLayerWeight(thumbAnimLayer);
        public float IndexLayerWeight => animator.GetLayerWeight(grabIndexAnimLayer);
        public float FlexParam => flexParam;
        public float PinchParam => pinchParam;
        public bool IsIndexTouch => isIndexTouch;
        public bool IsThumbTouch => isThumbTouch;
        
        #endregion
        
        #region Private Methods
        
        private void Start()
        {
            animator = GetComponent<Animator>();

            if (!animator)
                return;

            thumbAnimLayer = animator.GetLayerIndex(AnimLayerNameThumb);
            grabThumbAnimLayer = animator.GetLayerIndex(AnimLayerNameGrabThumb);
            grabIndexAnimLayer = animator.GetLayerIndex(AnimLayerNameGrabIndex);
            grabMiddleAnimLayer = animator.GetLayerIndex(AnimLayerNameGrabMiddle);

            animParamIndexPose = Animator.StringToHash(AnimParamNamePose);
        }


        private void Update()
        {
            if (!animator)
                return;

            UpdateTouchState();

            // 검지
            pinchParam = (float)(PXRInputBridge.GetXRController(controller).GetButtonState(Buttons.Trigger).value);
            animator.SetFloat(Pinch, pinchParam);

            //그랩 버튼
            flexParam = (float)(PXRInputBridge.GetXRController(controller).GetButtonState(Buttons.Grip).value);
            animator.SetFloat(HashFlex, flexParam);

            if (!isActiveController)
                UpdateAnimatorParameters();
            else
                UpdateGrabbingAnimatorParameters();
        }

        private void UpdateTouchState()
        {
            // 엄지 터치
            isThumbTouch = PXRInputBridge.GetXRController(controller).GetButtonState(Buttons.Primary).isTouch;
            isThumbTouch |= PXRInputBridge.GetXRController(controller).GetButtonState(Buttons.Secondary).isTouch;
            isThumbTouch |= PXRInputBridge.GetXRController(controller).GetButtonState(Buttons.PrimaryAxis).isTouch;
            isThumbTouch |= PXRInputBridge.GetXRController(controller).GetButtonState(Buttons.SecondaryAxis).isTouch;
            // 검지 터치
            isIndexTouch = PXRInputBridge.GetXRController(controller).GetButtonState(Buttons.Trigger).isTouch;
        }

        /// <summary>
        /// 컨트롤러 쥐고 있지 않을 때 
        /// </summary>
        private void UpdateAnimatorParameters()
        {
            // 엄지
            thumbWeight = InputValueRateChange(isThumbTouch, false, thumbWeight);
            animator.SetLayerWeight(thumbAnimLayer, thumbWeight);

            //검지
            pointWeight = InputValueRateChange(isIndexTouch, false, pointWeight);
            animator.SetLayerWeight(grabIndexAnimLayer, pointWeight);
        }

        /// <summary>
        /// 컨트롤러 쥐고 있을 때
        /// </summary>
        private void UpdateGrabbingAnimatorParameters()
        {
            animator.SetBool(IndexTouch, isIndexTouch);

            if (!isIndexTouch)
            {
                animator.SetLayerWeight(grabIndexAnimLayer, 1);
            }
            else
            {
                pointWeight = InputValueRateChange(isIndexTouch, true, pointWeight);
                pointWeight = Mathf.Clamp(pointWeight, 0, 0.1f);

                animator.SetLayerWeight(grabIndexAnimLayer, pointWeight);
            }

            var middleWeight = flexParam * 0.15f;
            animator.SetLayerWeight(grabMiddleAnimLayer, middleWeight);
        }

        /// <summary>
        /// 누르는 세기에 따른 수치를 반환
        /// </summary>
        /// <param name="isDown">누른 상태</param>
        /// <param name="isReverse">값 반전</param>
        /// <param name="value">세기</param>
        /// <returns>수치</returns>
        private float InputValueRateChange(bool isDown, bool isReverse, float value)
        {
            var rateDelta = Time.deltaTime * InputRate;

            var sign = 0f;
            if (!isReverse)
                sign = isDown ? -1.0f : 1.0f;
            else
                sign = isDown ? 1.0f : -1.0f;

            return Mathf.Clamp01(value + rateDelta * sign);
        }
        
        #endregion
    }
}