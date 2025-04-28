using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    public class PXRDirect3DButton : PXRDirectInteractableBase, IPointerClickHandler
    {
        private enum Direction
        {
            X,
            Y,
            Z
        }

        #region Public Fields

        /// <summary>
        /// 버튼이 완벽하게 눌린 경우 실행되는 이벤트
        /// </summary>
        public event Action CompletedPressEvent;

        /// <summary>
        /// 버튼을 포인터 클릭할 시 실행되는 이벤트
        /// </summary>
        public event Action PointerClickEvent;

        #endregion

        #region Private Fields

        [Tooltip("버튼의 방향을 설정합니다.")]
        [SerializeField]
        private Direction localDirection = Direction.Y;

        [Tooltip("버튼이 눌릴 최소 위치를 설정합니다.")]
        [SerializeField]
        private float minLocalPos;

        [Tooltip("버튼이 눌릴 최대 위치를 설정합니다.")]
        [SerializeField]
        private float maxLocalPos;

        [Tooltip("버튼이 눌릴 때 속도를 설정합니다.")]
        [SerializeField]
        private float buttonSpeed = 10f;

        [Tooltip("버튼이 눌릴 때 눌린 위치로 이동할 때의 오프셋을 설정합니다.")]
        [SerializeField]
        private float pressedOffset = 0.003f;

        [Tooltip("트리거 버튼을 눌러 버튼을 누르는지 설정합니다.")]
        [SerializeField]
        private bool useTriggerClick = true;

        private IEnumerator routineMoveButtonDown;
        private IEnumerator routineMoveButtonUp;
        private IEnumerator routineAutoMoveDownAndUp;
        private IEnumerator routineDelay;

        private Transform childButtonMesh;

        private Vector3 buttonDownPosition;
        private Vector3 buttonUpPosition;

        #endregion

        #region Public Methods

        public void OnHover()
        {
            StopCoroutines();
            if (gameObject.activeInHierarchy)
            {
                routineMoveButtonDown = MoveButtonDown();
                StartCoroutine(routineMoveButtonDown);
            }
        }

        public void OnUnHover()
        {
            StopCoroutines();
            if (gameObject.activeInHierarchy)
            {
                routineMoveButtonUp = MoveButtonUp();
                StartCoroutine(routineMoveButtonUp);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClickEvent?.Invoke();
        }

        public void ImmediatePositionUp()
        {
            childButtonMesh.localPosition = buttonUpPosition;
        }

        public void ImmediatePositionDown()
        {
            childButtonMesh.localPosition = buttonDownPosition;
        }

        public void ClearButtonEvent()
        {
            CompletedPressEvent = null;
        }

        #endregion

        #region Private Methods

        protected void Awake()
        {
            childButtonMesh = GetComponentInChildren<MeshRenderer>().transform;

            var buttonLocalPosition = childButtonMesh.localPosition;
            
            switch (localDirection)
            {
                case Direction.X:
                    buttonDownPosition = new Vector3(minLocalPos, buttonLocalPosition.y, buttonLocalPosition.z);
                    buttonUpPosition = new Vector3(maxLocalPos, buttonLocalPosition.y, buttonLocalPosition.z);
                    break;
                case Direction.Y:
                    buttonDownPosition = new Vector3(buttonLocalPosition.x, minLocalPos, buttonLocalPosition.z);
                    buttonUpPosition = new Vector3(buttonLocalPosition.x, maxLocalPos, buttonLocalPosition.z);
                    break;
                case Direction.Z:
                    buttonDownPosition = new Vector3(buttonLocalPosition.x, buttonLocalPosition.y, minLocalPos);
                    buttonUpPosition = new Vector3(buttonLocalPosition.x, buttonLocalPosition.y, maxLocalPos);
                    break;
            }

            gameObject.layer = PXRNameToLayer.Directable;

            HoverEvent += OnHover;
            UnHoverEvent += OnUnHover;
        }

        private void OnEnable()
        {
            if (PXRRig.IsVRPlay)
            {
                if (useTriggerClick)
                    PointerClickEvent += ClickedButton;
            }
            else
            {
                PointerClickEvent += ClickedButton;
            }
        }

        private void OnDisable()
        {
            if (PXRRig.IsVRPlay)
            {
                if (useTriggerClick)
                    PointerClickEvent -= ClickedButton;
            }
            else
            {
                PointerClickEvent -= ClickedButton;
            }
        }

        /// <summary>
        /// 모든 코루틴을 중지
        /// </summary>
        private void StopCoroutines()
        {
            if (routineMoveButtonUp != null)
            {
                StopCoroutine(routineMoveButtonUp);
                routineMoveButtonUp = null;
            }

            if (routineMoveButtonDown != null)
            {
                StopCoroutine(routineMoveButtonDown);
                routineMoveButtonDown = null;
            }

            if (routineDelay != null)
            {
                StopCoroutine(routineDelay);
                routineDelay = null;
            }

            if (routineAutoMoveDownAndUp != null)
            {
                StopCoroutine(routineAutoMoveDownAndUp);
                routineAutoMoveDownAndUp = null;
            }
        }
        
        /// <summary>
        /// 손가락이 닿았을 때 자동으로 버튼이 내려가는 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveButtonDown()
        {
            while (true)
            {
                float buttonDistance = 0; //childButtonMesh.localPosition.y - buttonDownPosition.y;

                switch (localDirection)
                {
                    case Direction.X:
                        buttonDistance = childButtonMesh.localPosition.x - buttonDownPosition.x;
                        break;
                    case Direction.Y:
                        buttonDistance = childButtonMesh.localPosition.y - buttonDownPosition.y;
                        break;
                    case Direction.Z:
                        buttonDistance = childButtonMesh.localPosition.z - buttonDownPosition.z;
                        break;
                }

                if (buttonDistance <= pressedOffset)
                {
                    childButtonMesh.localPosition = buttonDownPosition;
                    CompletedPressEvent?.Invoke();
                    break;
                }

                childButtonMesh.localPosition = Vector3.Lerp(childButtonMesh.localPosition, buttonDownPosition, buttonSpeed * Time.deltaTime);
                yield return null;
            }
        }
        
        /// <summary>
        /// 손가락이 떨어졌을 때 자동으로 버튼이 올라가는 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveButtonUp()
        {
            while (true)
            {
                float buttonDistance = 0; //childButtonMesh.localPosition.y - buttonDownPosition.y;

                switch (localDirection)
                {
                    case Direction.X:
                        buttonDistance = buttonUpPosition.x - childButtonMesh.localPosition.x;
                        break;
                    case Direction.Y:
                        buttonDistance = buttonUpPosition.y - childButtonMesh.localPosition.y;
                        break;
                    case Direction.Z:
                        buttonDistance = buttonUpPosition.z - childButtonMesh.localPosition.z;
                        break;
                }

                if (buttonDistance <= pressedOffset)
                {
                    childButtonMesh.localPosition = buttonUpPosition;
                    break;
                }

                childButtonMesh.localPosition = Vector3.Lerp(childButtonMesh.localPosition, buttonUpPosition, buttonSpeed * Time.deltaTime);
                yield return null;
            }
        }


        /// <summary>
        /// 트리거로 클릭했을 때
        /// </summary>
        private void ClickedButton()
        {
            if (!CanInteract)
                return;

            StopCoroutines();
            routineAutoMoveDownAndUp = CoroutineAutoMoveDownAndUp();
            StartCoroutine(routineAutoMoveDownAndUp);
        }


        /// <summary>
        /// 클릭했을 때 자동으로 내려갔다 올라옴
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoroutineAutoMoveDownAndUp()
        {
            yield return MoveButtonDown();
            yield return YieldInstructionCache.WaitForSeconds(0.2f);
            yield return MoveButtonUp();
        }

        #endregion
    }
}