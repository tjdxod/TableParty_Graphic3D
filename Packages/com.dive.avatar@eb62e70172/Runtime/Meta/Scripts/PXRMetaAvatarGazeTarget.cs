#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System.Collections;
using UnityEngine;
using Oculus.Avatar2;

namespace Dive.Avatar.Meta
{
    /// <summary>
    /// 메타 아바타 시선 대상을 설정하는 클래스
    /// </summary>
    [RequireComponent(typeof(PXRMetaAvatarEntityBase))]
    public class PXRMetaAvatarGazeTarget : MonoBehaviour
    {
        #region Private Fields

        private static readonly CAPI.ovrAvatar2JointType HEAD_GAZE_TARGET_JNT = CAPI.ovrAvatar2JointType.Head;
        private static readonly CAPI.ovrAvatar2JointType LEFT_HAND_GAZE_TARGET_JNT = CAPI.ovrAvatar2JointType.LeftHandIndexProximal;
        private static readonly CAPI.ovrAvatar2JointType RIGHT_HAND_GAZE_TARGET_JNT = CAPI.ovrAvatar2JointType.RightHandIndexProximal;
        private PXRMetaAvatarEntityBase avatarEntityBase;        

        #endregion
        
        protected IEnumerator Start()
        {
            avatarEntityBase = GetComponent<PXRMetaAvatarEntityBase>();
            yield return new WaitUntil(() => avatarEntityBase.HasJoints);

            SetGazeTargetTransform(transform.Find("Joint Head").gameObject, CAPI.ovrAvatar2GazeTargetType.AvatarHead);
            SetGazeTargetTransform(transform.Find("Joint LeftHandWrist").gameObject, CAPI.ovrAvatar2GazeTargetType.AvatarHand);
            SetGazeTargetTransform(transform.Find("Joint RightHandWrist").gameObject, CAPI.ovrAvatar2GazeTargetType.AvatarHand);
        }

        private void SetGazeTargetTransform(GameObject gazeTargetObj, CAPI.ovrAvatar2GazeTargetType targetType)
        {
            var gazeTarget = gazeTargetObj.AddComponent<OvrAvatarGazeTarget>();
            if (gazeTarget)
            {
                gazeTarget.TargetType = targetType;
            }
            else
            {
                OvrAvatarLog.LogError($"SampleAvatarGazeTargets: No gaze target component found for {gazeTargetObj.name}");
            }
        }
    }
}

#endif