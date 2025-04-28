using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Attributes;
using UnityEngine;

public class TeleportAvatarCreator : MonoBehaviour
{
    [SerializeField]
    private bool isCreateAvatar = false;

    private void Awake()
    {
        if(isCreateAvatar)
            CreateAvatar();
    }
    
    [Button]
    public void CreateAvatar()
    {
        MetaAvatarCreator.Instance.CreateAvatar(transform.position);
    }
}
