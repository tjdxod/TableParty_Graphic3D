using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dive.VRModule
{
    // ReSharper disable once IdentifierTypo
    public class PVRPCTeleporter : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        // ReSharper disable once NotAccessedField.Local
        private List<PXRTeleportSpaceBase> teleportAreaList = new List<PXRTeleportSpaceBase>();

        // ReSharper disable once NotAccessedField.Local
        private PXRPlayerController playerController;
        private PXRTeleportSpaceBase currentTeleportBase;
        // ReSharper disable once NotAccessedField.Local
        private PXRPCPlayerMovement pcMovement;
        private IEnumerator routineCheckExitInterval;

        private bool isTeleporting = false;
        private const float ExitInterval = 0.8f;

        #endregion

        #region Public Properties

        public bool IsTeleporting => isTeleporting;

        #endregion

        #region Public Methods

        /// <summary>
        /// 텔레포트 포인트 추가
        /// </summary>
        /// <param name="teleportSpace">텔레포트 포인트</param>
        public void AddTeleportPoint(PXRTeleportSpaceBase teleportSpace)
        {
        }

        /// <summary>
        /// 텔레포트 포인트 삭제
        /// </summary>
        /// <param name="teleportSpace">텔레포트 포인트</param>
        public void RemoveTeleportPoint(PXRTeleportSpaceBase teleportSpace)
        {
        }

        #endregion

        #region Private Methods

        private void Awake()
        {
            playerController = GetComponent<PXRPlayerController>();
            pcMovement = GetComponent<PXRPCPlayerMovement>();
        }

        /// <summary>
        /// 트리거 부딪치면 텔레포트로 들어갈 수 있게
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.layer.Equals(PXRNameToLayer.TeleportSpace))
                return;

            var space = other.GetComponent<PXRTeleportSpaceBase>();

            if (space.Equals(currentTeleportBase))
                return;

            if (space)
            {
                EnterTeleportPoint(space);
            }
        }

        /// <summary>
        /// 텔레포트 포인트 입장
        /// </summary>
        /// <param name="space">입장한 텔레포트 포인트</param>
        private void EnterTeleportPoint(PXRTeleportSpaceBase space)
        {
            isTeleporting = false;

            // 이전 포인트에서 나감 (거리 벗어나기 전에 다른 포인트 들어갔을 때만)
            if (currentTeleportBase)
            {
            }

            currentTeleportBase = space;

            //DeactivateTeleportPoints();
            //pcMovement.DisableMove();

            //playerController.AfterForceMoving += AfterForceMoving;
            //playerController.MoveToFixedDestination(area.transform.position);

            if (routineCheckExitInterval != null)
            {
                StopCoroutine(routineCheckExitInterval);
                routineCheckExitInterval = null;
            }

            routineCheckExitInterval = CoroutineCheckExitInterval();
            StartCoroutine(routineCheckExitInterval);
        }


        /// <summary>
        /// 텔레포트 포인트에서 나가는 거리 체크
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoroutineCheckExitInterval()
        {
            while (true)
            {
                var dist = Vector3.Distance(transform.position, currentTeleportBase.transform.position);

                if (dist > ExitInterval)
                {
                    ExitTeleportPoint();
                    yield break;
                }

                yield return YieldInstructionCache.WaitForFixedUpdate;
            }
        }

        // 포인트에서 나감
        private void ExitTeleportPoint()
        {
            currentTeleportBase = null;
        }
        
        #endregion
    }
}