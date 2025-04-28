using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public class PXRIntensifyInteractBase : MonoBehaviour, IIntensifyInteract
    {
        #region Private Fields
        
        [SerializeField, ShowIf(nameof(ModeType), InteractMode.Finger)]
        protected FingerType fingerType = FingerType.None;

        [SerializeField, ShowIf(nameof(ModeType), InteractMode.Palm)]
        protected PalmType palmType = PalmType.None;

        [SerializeField]
        protected float radius = 0.00875f;

        [SerializeField]
        private Transform tail;

        private readonly List<PXRDirectableBase> directableList = new List<PXRDirectableBase>();
        private readonly List<PXRDirectableBase> overlapResultList = new List<PXRDirectableBase>(10);
        private readonly Collider[] overlapResults = new Collider[10];
        private PXRInteractAddon interactAddon;
        
        private int length;

        private Color color = Color.green;

        #endregion

        #region Public Properties
        
        [field: SerializeField]
        public HandSide HandSide { get; private set; } = HandSide.Left;
        
        [field: SerializeField]
        public InteractMode ModeType { private set; get; } = InteractMode.None;

        public int Count => directableList.Count;
        public List<PXRDirectableBase> DirectableList => directableList;
        public List<PXRDirectableBase> OverlapResultList => overlapResultList;
        
        public int Type
        {
            get
            {
                switch (ModeType)
                {
                    case InteractMode.Finger:
                        return (int)fingerType;
                    case InteractMode.Palm:
                        return (int)palmType;
                    default:
                        return -1;
                }
            }
        }

        public float Radius => radius;

        #endregion

        #region Public Methods

        public void Initialize(PXRInteractAddon addon)
        {
            interactAddon = addon;
        }

        public PXRDirectableBase[] GetDirectableArray()
        {
            return directableList.ToArray();
        }

        public void ForceAllRelease()
        {
            if(Count == 0)
                return;
            
            foreach (var directable in directableList)
            {
                directable.OnExitEvent(this);
                color = Color.green;
                directable.ForceRelease();
            }
            
            directableList.Clear();
        }
        
        public (Collider[], int) OverlapAtPoint(InteractMode mode)
        {
            var layerMask = 1 << PXRNameToLayer.Directable;
            // length = Physics.OverlapSphereNonAlloc(transform.position, radius, overlapResults, layerMask);

            length = Physics.OverlapCapsuleNonAlloc(transform.position, tail.position, radius, overlapResults, layerMask);

            overlapResultList.Clear();

            if (length == 0)
            {
                if (directableList.Count <= 0)
                    return (null, -1);

                foreach (var directable in directableList)
                {
                    directable.OnExitEvent(this);
                    color = Color.green;
                }

                directableList.Clear();

                return (null, -1);
            }

            for (var i = 0; i < length; i++)
            {
                var directable = overlapResults[i].GetComponent<IDirectable>().GetInteractableBase();
                if (directable == null)
                    continue;

                overlapResultList.Add(directable);

                if (mode == InteractMode.Finger)
                {
                    var isPressable = interactAddon.IsPressable(this);

                    if (!isPressable)
                    {
                        if (directableList.Contains(directable))
                        {
                            directable.OnExitEvent(this);
                            directableList.Remove(directable);
                            color = Color.green;
                        }

                        continue;
                    }
                }

                switch (directable.DirectableState)
                {
                    case DirectableState.Enter:
                        directable.OnStayEvent(this);
                        color = Color.red;
                        break;
                    case DirectableState.Stay:
                        directable.OnStayEvent(this);
                        color = Color.red;
                        break;
                    case DirectableState.Exit:
                        directable.OnEnterEvent(this);
                        color = Color.red;
                        directableList.Add(directable);
                        break;
                }
            }

            foreach (var directable in directableList)
            {
                if (!overlapResultList.Contains(directable))
                {
                    directable.OnExitEvent(this);
                    color = Color.green;
                }
            }

            directableList.Clear();
            directableList.AddRange(overlapResultList);

            return length == 0 ? (null, -1) : (overlapResults, length);
        }

        public (Vector3, Vector3) GetPosition(bool isWorld = true)
        {
            return isWorld ? (transform.position, tail.position) : (transform.localPosition, tail.localPosition);
        }

        public (Quaternion, Quaternion) GetRotation(bool isWorld = true)
        {
            return isWorld ? (transform.rotation, tail.rotation) : (transform.localRotation, tail.localRotation);
        }

        public Vector3 GetDirection(bool isWorld = true)
        {
            return isWorld
                ? (transform.position - tail.position)
                : (transform.localPosition - tail.localPosition);
        }

        public PXRIntensifyInteractBase GetIntensifyInteractBase()
        {
            return this;
        }

        #endregion

        #region Private Methods

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, radius);

            if (tail != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(tail.position, radius);
            }
        }
#endif

        #endregion
    }
}