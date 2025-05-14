#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System.Collections;
using System.Collections.Generic;
using Dive.Avatar;
using Dive.Avatar.Meta;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;

public class PXRMetaAvatarAnimator : MonoBehaviour
{
    private static readonly int Sitting = Animator.StringToHash("IsSitting");
    private static readonly int Moving = Animator.StringToHash("IsMoving");
    private static readonly int Strength = Animator.StringToHash("CrouchStrength");

    [SerializeField, ReadOnly]
    private PXRMetaAvatarEntityBase entity;

    [SerializeField]
    private float distance = 2;
    
    private PXRAvatarBridge bridge;
    private Animator animator;

    private Vector3 sittingPosition;
    private bool isSitting = false;
    private bool isMoving = false;
    private float crouchStrength = 0.0f;
    
    public bool IsSitting
    {
        get => isSitting;
        set
        {
            sittingPosition = transform.position;
            isSitting = value;
        }
    }

    public bool IsMoving
    {
        get => isMoving;
        set => isMoving = value;
    }

    public float CrouchStrength
    {
        get => crouchStrength;
        set => crouchStrength = value;
    }

    private void Awake()
    {
        entity = transform.parent.GetComponent<PXRMetaAvatarEntityBase>();

        if (entity == null)
        {
            Debug.LogWarning("PXRMetaAvatarAnimator: Entity is null");
        }
        
        animator = GetComponent<Animator>();

        bridge = entity.GetBridgeComponent().GetComponent<PXRAvatarBridge>();
        // bridge.metaAvatarAnimator = this;
    }

    private void Update()
    {
        if(entity == null || animator == null)
            return;
        
        animator.SetBool(Sitting, isSitting);
        animator.SetBool(Moving, isMoving);
        animator.SetFloat(Strength, crouchStrength);

        if (!isSitting)
            return;
        
        var sit = entity.transform.position;
        sit.y = sittingPosition.y;

        var compare = Vector3.Distance(sit, sittingPosition);

        if (!(compare > distance))
            return;
            
        PXRRoomSittingManager.StandPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        isSitting = false;
        sittingPosition = Vector3.zero;
    }
}

#endif