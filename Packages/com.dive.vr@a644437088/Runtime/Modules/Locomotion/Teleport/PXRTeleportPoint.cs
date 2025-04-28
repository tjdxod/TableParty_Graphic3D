using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public enum PointParticleState
    {
        None,
        One,
        Toggle
    }

    public class PXRTeleportPoint : PXRTeleportSpaceBase
    {
        #region Private Fields

        [Header("포인트")]
        [SerializeField, LabelText("파티클 포인트 사용 유무"), Tooltip("파티클 포인트를 사용할 것인지")]
        private PointParticleState useParticlePoint = PointParticleState.None;

        [SerializeField, LabelText("파티클 포인트"), ShowIf(nameof(useParticlePoint), PointParticleState.One)]
        private GameObject oneParticlePoint;

        [SerializeField, LabelText("On 파티클 포인트"), ShowIf(nameof(useParticlePoint), PointParticleState.Toggle)]
        private GameObject toggleOnParticlePoint;

        [SerializeField, LabelText("Off 파티클 포인트"), ShowIf(nameof(useParticlePoint), PointParticleState.Toggle)]
        private GameObject toggleOffParticlePoint;

        [SerializeField]
        private PXRTeleportOwnerArea overrideArea;

        private IEnumerator routineActivePoint;

        #endregion
        
        #region Public Methods

        public override void AdditionalActive()
        {
            if (routineActivePoint != null)
            {
                StopCoroutine(routineActivePoint);
                routineActivePoint = null;
            }

            var prevCanTeleport = CanTeleport;
            RefreshPoints(prevCanTeleport);

            if (!isActiveAndEnabled)
                return;

            routineActivePoint = CoroutineActivePoint(prevCanTeleport);
            StartCoroutine(routineActivePoint);
        }

        public override void AdditionalInactive()
        {
            if (routineActivePoint == null) 
                return;
            
            StopCoroutine(routineActivePoint);
            routineActivePoint = null;
        }

        public override void OnEnterPlayer()
        {
            if (overrideArea == null)
                base.OnEnterPlayer();
            else
                overrideArea.OnEnterPlayer();
        }

        public override void EnteredPlayerToSpace()
        {
            CanVisible = false;
            InactivePoints();
            DisableCanTeleport();
        }

        public override void OnExitPlayer()
        {
            if (overrideArea == null)
                base.OnExitPlayer();
            else
                overrideArea.OnExitPlayer();
        }

        public override void ExitedPlayerToSpace()
        {
            CanVisible = true;
            ActivePoints();
            EnableCanTeleport();

            if (!UseActiveOnlyTeleporting)
                RefreshPoints(CanTeleport);
        }

        public override PXRTeleportSpaceBase GetTeleportSpace()
        {
            if (overrideArea != null)
                return overrideArea;

            return this;
        }

        public override PXRTeleportSpaceBase GetOriginalTeleportSpace()
        {
            return this;
        }

        #endregion

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();

            if (SpaceType != SpaceType.Point)
                SpaceType = SpaceType.Point;

            if (useParticlePoint == PointParticleState.One)
            {
                if (oneParticlePoint == null)
                {
                    Debug.LogError("파티클 포인트를 설정해주세요.");
                    return;
                }

                oneParticlePoint.SetActive(false);
            }
            else if (useParticlePoint == PointParticleState.Toggle)
            {
                if (toggleOnParticlePoint == null || toggleOffParticlePoint == null)
                {
                    Debug.LogError("파티클 포인트를 설정해주세요.");
                    return;
                }

                toggleOnParticlePoint.SetActive(false);
                toggleOffParticlePoint.SetActive(false);
            }
        }

        private void Start()
        {
            RegisterActiveOnlyTeleport();
        }

        private void OnDestroy()
        {
            UnregisterActiveOnlyTeleport();
        }

        protected void ActivePoints()
        {
            if (!CanVisible || useParticlePoint == PointParticleState.None)
                return;

            if (useParticlePoint == PointParticleState.One)
            {
                oneParticlePoint.SetActive(true);
            }
            else if (useParticlePoint == PointParticleState.Toggle)
            {
                if (CanTeleport)
                {
                    toggleOnParticlePoint.SetActive(true);
                    toggleOffParticlePoint.SetActive(false);
                }
                else
                {
                    toggleOnParticlePoint.SetActive(false);
                    toggleOffParticlePoint.SetActive(true);
                }
            }
        }

        protected void InactivePoints()
        {
            if (!CanVisible || useParticlePoint == PointParticleState.None)
                return;

            if (useParticlePoint == PointParticleState.One)
            {
                oneParticlePoint.SetActive(false);
            }
            else if (useParticlePoint == PointParticleState.Toggle)
            {
                toggleOnParticlePoint.SetActive(false);
                toggleOffParticlePoint.SetActive(false);
            }
        }

        protected void RefreshPoints(bool canTeleport)
        {
            if (!CanVisible || useParticlePoint == PointParticleState.None)
            {
                InactivePoints();
                return;
            }

            if (useParticlePoint == PointParticleState.One)
            {
                oneParticlePoint.SetActive(canTeleport);
            }
            else if (useParticlePoint == PointParticleState.Toggle)
            {
                if (canTeleport)
                {
                    toggleOnParticlePoint.SetActive(true);
                    toggleOffParticlePoint.SetActive(false);
                }
                else
                {
                    toggleOnParticlePoint.SetActive(false);
                    toggleOffParticlePoint.SetActive(true);
                }
            }
        }

        private IEnumerator CoroutineActivePoint(bool prevCanTeleport)
        {
            while (isActiveAndEnabled)
            {
                if (prevCanTeleport != CanTeleport)
                {
                    prevCanTeleport = CanTeleport;
                    RefreshPoints(prevCanTeleport);
                }

                yield return null;
            }
            // ReSharper disable once FunctionNeverReturns
        }

        #endregion
    }
}