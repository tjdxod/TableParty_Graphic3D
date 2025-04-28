using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MetaAvatarCreator : MonoBehaviour
{
    public static MetaAvatarCreator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MetaAvatarCreator>();
            }

            return instance;
        }
    }

    private static MetaAvatarCreator instance;
    
    [SerializeField]
    private GameObject avatarPrefab;

    [field:SerializeField, ReadOnly]
    private int createAvatarCount = 0;
    
    private List<GameObject> avatars = new List<GameObject>();
    
    private Vector3 CreateRandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-5f, 5f), 0, UnityEngine.Random.Range(-5f, 5f));
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CreateAvatar();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DestroyAvatar();
        }
    }

    public void CreateAvatar(Vector3 position)
    {
        var avatar = Instantiate(avatarPrefab, position, Quaternion.identity);
        avatars.Add(avatar);
        createAvatarCount++;
    }
    
    public void CreateAvatar()
    {
        var avatar = Instantiate(avatarPrefab, CreateRandomPosition(), Quaternion.identity);
        avatars.Add(avatar);
        createAvatarCount++;
    }

    public void DestroyAvatar()
    {
        if (avatars.Count > 0)
        {
            var avatar = avatars[0];
            avatars.RemoveAt(0);
            Destroy(avatar);
            createAvatarCount--;
        }
    }
}
