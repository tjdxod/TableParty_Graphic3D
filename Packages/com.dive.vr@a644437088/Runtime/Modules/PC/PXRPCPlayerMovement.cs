using UnityEngine;
using UnityEngine.InputSystem;


namespace Dive.VRModule
{
    // ReSharper disable once IdentifierTypo
    public class PXRPCPlayerMovement : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private InputActionReference movementReference;

        [SerializeField]
        private InputActionReference handMoveReference;

        [SerializeField]
        private InputActionReference mouseDeltaReference;

        [SerializeField]
        private Transform rightHand;

        [SerializeField]
        private Transform teleportPivot;

        private Camera cam;
        private PXRGrabber grabber;
        private Rigidbody rigid;
        private Vector2 inputVec;

        [SerializeField]
        private float moveSpeed = 3f;

        [SerializeField]
        private float rotSpeed = 1f;

        private float rotX = 0f;
        private float rotY = 0f;
        private readonly float handMoveSpeed = 1f;

        private bool canMove = true;
        private bool canRotate = true;

        #endregion

        #region Public Properties

        public bool IsMoving => inputVec != Vector2.zero;

        #endregion

        #region Public Methods

        public void EnableMove()
        {
            canMove = true;
        }

        public void DisableMove()
        {
            canMove = false;
        }

        public void EnableMoveAndRotate()
        {
            canMove = true;
            canRotate = true;
        }


        public void DisableMoveAndRotate()
        {
            canMove = false;
            canRotate = false;
        }

        public void PCRecenter()
        {
            DisableMoveAndRotate();

            transform.position = teleportPivot.position;
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.rotation = teleportPivot.rotation;
            cam.transform.localRotation = Quaternion.identity;
            rotX = 0f;
            rotY = 0f;

            EnableMoveAndRotate();
        }

        #endregion

        #region Private Methods

        private void Awake()
        {
            grabber = GetComponentInChildren<PXRGrabber>();
            rigid = GetComponent<Rigidbody>();

            cam = Camera.main;
        }

        private void Start()
        {
            movementReference.action.Enable();
            handMoveReference.action.Enable();
            mouseDeltaReference.action.Enable();

            grabber.ReleasedEvent += OnReleasedEvent;
        }

        private void Update()
        {
            GetInputMoveValue();

            RotateCamera();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void GetInputMoveValue()
        {
            inputVec = movementReference.action.ReadValue<Vector2>().normalized;
        }

        private void MovePlayer()
        {
            if (!canMove)
                return;

            var moveDir = new Vector3(inputVec.x, 0, inputVec.y);

            moveDir = cam.transform.TransformDirection(moveDir).normalized;
            moveDir.y = 0f;
            moveDir *= moveSpeed;

            rigid.velocity = moveDir * Time.deltaTime;
        }
        
        private void RotateCamera()
        {
            if (!canRotate)
                return;

            var mouseX = mouseDeltaReference.action.ReadValue<Vector2>().x;
            var mouseY = mouseDeltaReference.action.ReadValue<Vector2>().y;

            rotX += mouseX * rotSpeed;
            rotY += mouseY * rotSpeed;

            rotY = Mathf.Clamp(rotY, -80f, 80f);

            cam.transform.localEulerAngles = new Vector3(-rotY, rotX, 0);
        }
        
        private void HandMove()
        {
            var move = handMoveReference.action.ReadValue<float>();

            var handLocalPos = rightHand.localPosition;
            handLocalPos.z += move * handMoveSpeed * Time.deltaTime;
            handLocalPos.z = Mathf.Clamp(handLocalPos.z, 0f, 0.4f);
            rightHand.localPosition = handLocalPos;
        }
        
        // ReSharper disable once ParameterHidesMember
        private void OnReleasedEvent(PXRGrabber grabber, PXRGrabbable grabbable, HandSide releaseHandSide)
        {
        }

        private void ResetHandPos()
        {
            var handLocalPos = rightHand.localPosition;
            handLocalPos.z = 0f;
            rightHand.localPosition = handLocalPos;
        }

        #endregion
    }
}