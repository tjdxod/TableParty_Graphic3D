#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System.Collections;
using Cysharp.Threading.Tasks;
using Dive.VRModule;
using Oculus.Platform;
using Oculus.Platform.Models;
using Photon.Pun;
using UnityEngine;

public class PXRMetaRoom : MonoBehaviourPunCallbacks
{
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnected);

        Debug.LogWarning("접속됨.");

        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        Debug.LogWarning("방에 들어옴.");
    }

    public override async void OnJoinedRoom()
    {
        await UniTask.WaitUntil(Core.IsInitialized);
        var loggedMsg = Users.GetLoggedInUser().OnComplete(Callback);
    }

    private void Callback(Message<User> user)
    {
        if(user.IsError)
        {
            Debug.LogError("Error getting logged in user.");
            return;
        }
        
        var id = user.Data.ID.ToString();
        
        var avatar = PhotonNetwork.Instantiate("NetworkMetaAvatar", Vector3.zero, Quaternion.identity, 0, new object[]
        {
            PhotonNetwork.LocalPlayer.ActorNumber, id
        });
        
        avatar.transform.SetParent(PXRRig.PlayerController.transform);
        avatar.transform.localPosition = Vector3.zero;
        avatar.transform.localRotation = Quaternion.identity;
    }
}

#endif