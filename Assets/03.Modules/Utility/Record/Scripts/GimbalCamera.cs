using System;
using UnityEngine;

public class GimbalCamera : MonoBehaviour
{
    [SerializeField]
    private bool isUpdate = true;

    public bool isFreeMove = false;

    [SerializeField]
    private float speed = 25f;

    [SerializeField]
    private float rot = 5f;

    // [SerializeField]
    // private float fixX = 0f;
    //
    // [SerializeField]
    // private float fixY = 0f;
    //
    // [SerializeField]
    // private float fixZ = 0;

    [SerializeField]
    private Vector3 fixPosition = Vector3.zero;
    
    [Space]
    [SerializeField]
    private bool useKalmanFilter = false;

    [SerializeField]
    private float q = 0.00005f;

    [SerializeField]
    private float r = 0.01f;

    [Header("마우스 이동 및 회전")]
    [SerializeField]
    private float moveSpeed = 0.5f;
    
    [SerializeField]
    private float rotSpeed = 0.5f;
    
    private Camera mainCamera;
    private bool hasMainCamera = false;

    private KalmanFilterRotation kalmanFilter;

    private const float MainSpeed = 5.0f;
    private const float ShiftAdd = 15.0f;
    private const float MaxShift = 50.0f;
    private float totalRun = 1.0f;

    private void Start()
    {
    // #if !UNITY_EDITOR
    //      gameObject.SetActive(false);
    // #endif

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라가 없습니다.");
            return;
        }

        hasMainCamera = true;

        var component = GetComponent<Camera>();
        component.cullingMask = mainCamera.cullingMask;

        kalmanFilter = new KalmanFilterRotation(q, r);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 자유시점
            isFreeMove = !isFreeMove;
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            moveSpeed -= 0.01f;
        }
        
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            moveSpeed += 0.01f;
        }

        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            rotSpeed -= 0.01f;
        }
        
        if (Input.GetKeyDown(KeyCode.Quote))
        {
            rotSpeed += 0.01f;
        }

        moveSpeed = Mathf.Clamp(moveSpeed, 0f, 1f);
        rotSpeed = Mathf.Clamp(rotSpeed, 0f, 1f);
    }
    
    private void LateUpdate()
    {
        if (!hasMainCamera)
            return;

        if (!isUpdate)
            return;

        if (isFreeMove)
        {
            Move();
            return;
        }

        var mainCameraRotation = mainCamera.transform.rotation;

        Vector3 targetRot;

        if (!useKalmanFilter)
        {
            var quaternion = Quaternion.Slerp(transform.rotation, mainCameraRotation, Time.deltaTime * rot);
            targetRot = quaternion.eulerAngles;
        }
        else
        {
            targetRot = kalmanFilter.Update(mainCamera.transform.rotation, q, r).eulerAngles;
        }

        targetRot.z = 0f;

        transform.rotation = Quaternion.Euler(targetRot);

        var forward = mainCamera.transform.forward.normalized;
        var up = mainCamera.transform.up.normalized;
        var right = mainCamera.transform.right.normalized;
        var targetPos = mainCamera.transform.position + forward * fixPosition.z + up * fixPosition.y + right * fixPosition.x;
        
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
    }

    private void Move()
    {
        if (Input.GetMouseButton(1))
        {
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");
            var mouseDir = new Vector3(-mouseY, mouseX, 0);
            transform.eulerAngles += mouseDir * (5f * rotSpeed);
        }
        
        var p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                totalRun += Time.deltaTime;
                p *= totalRun * ShiftAdd;
                p.x = Mathf.Clamp(p.x, -MaxShift, MaxShift);
                p.y = Mathf.Clamp(p.y, -MaxShift, MaxShift);
                p.z = Mathf.Clamp(p.z, -MaxShift, MaxShift);
            }
            else
            {
                totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
                p *= MainSpeed;
            }

            p *= Time.deltaTime * moveSpeed;
            var newPosition = transform.position;
            if (Input.GetKey(KeyCode.Space))
            {
                //If player wants to move on X and Z axis only
                transform.Translate(p);
                newPosition.x = transform.position.x;
                newPosition.z = transform.position.z;
                transform.position = newPosition;
            }
            else
            {
                transform.Translate(p);
            }
        }
    }

    private Vector3 GetBaseInput()
    {
        var pVelocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            pVelocity += new Vector3(0, 0, 1);
        }

        if (Input.GetKey(KeyCode.S))
        {
            pVelocity += new Vector3(0, 0, -1);
        }

        if (Input.GetKey(KeyCode.A))
        {
            pVelocity += new Vector3(-1, 0, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            pVelocity += new Vector3(1, 0, 0);
        }

        return pVelocity;
    }
}