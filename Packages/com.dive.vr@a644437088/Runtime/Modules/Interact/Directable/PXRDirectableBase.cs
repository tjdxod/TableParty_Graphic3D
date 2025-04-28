using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public delegate void OnDirectableEvent(PXRIntensifyInteractBase interact);
    
    public abstract class PXRDirectableBase : MonoBehaviour, IDirectable
    {
        #region Public Fields

        public event OnDirectableEvent EnterEvent;
        public event OnDirectableEvent StayEvent;
        public event OnDirectableEvent ExitEvent;

        #endregion

        #region Private Fields

        protected readonly List<PXRIntensifyInteractBase> InteractList = new List<PXRIntensifyInteractBase>();

        #endregion

        #region Public Properties

        [field: SerializeField, LabelText("상호작용 타입")]
        public DirectableType DirectableType { get; private set; } = DirectableType.None;

        [field: SerializeField, LabelText("IntensifyInteract와의 상태"), ReadOnly]
        public DirectableState DirectableState { get; private set; } = DirectableState.Exit;

        [field: SerializeField, LabelText("상호작용 가능 여부")]
        public bool CanInteract { get; private set; } 

        #endregion

        #region Public Methods

        public PXRDirectableBase GetInteractableBase()
        {
            return this;
        }

        public abstract void ForceRelease(bool useEvent = true);

        public abstract void ForcePress(bool useEvent = true);
        
        public virtual bool OnEnterEvent(PXRIntensifyInteractBase interact)
        {
            if (interact.ModeType == InteractMode.None)
            {
                DirectableState = DirectableState.Exit;
                return false;
            }

            DirectableState = DirectableState.Enter;

            if (!InteractList.Contains(interact))
                InteractList.Add(interact);
            
            EnterEvent?.Invoke(interact);
            return true;
        }

        public virtual bool OnStayEvent(PXRIntensifyInteractBase interact)
        {
            if (interact.ModeType == InteractMode.None)
            {
                DirectableState = DirectableState.Exit;
                return false;
            }

            DirectableState = DirectableState.Stay;

            if (!InteractList.Contains(interact))
                InteractList.Add(interact);

            StayEvent?.Invoke(interact);
            return true;
        }

        public virtual bool OnExitEvent(PXRIntensifyInteractBase interact)
        {
            if (interact.ModeType == InteractMode.None)
            {
                DirectableState = DirectableState.Exit;
                return false;
            }

            DirectableState = DirectableState.Exit;

            if (InteractList.Contains(interact))
                InteractList.Remove(interact);

            ExitEvent?.Invoke(interact);
            return true;
        }

        public void EnableInteract()
        {
            CanInteract = true;
        }
        
        public void DisableInteract()
        {
            CanInteract = false;
        }
        
        #endregion

        #region Private Methods

        protected virtual void Awake()
        {
            gameObject.layer = PXRNameToLayer.Directable;
        }

        protected static (Vector3, Vector3) LocalToWorldPosition((Vector3, Vector3) localPositions, Transform transform)
        {
            return (transform.TransformPoint(localPositions.Item1), transform.TransformPoint(localPositions.Item2));
        }

        protected static Vector3 LocalToWorldPosition(Vector3 localPosition, Transform transform)
        {
            return transform.TransformPoint(localPosition);
        }
        
        protected static (Vector3, Vector3) WorldToLocalPosition((Vector3, Vector3) worldPositions, Transform transform)
        {
            return (transform.InverseTransformPoint(worldPositions.Item1), transform.InverseTransformPoint(worldPositions.Item2));
        }
        
        protected static Vector3 WorldToLocalPosition(Vector3 worldPosition, Transform transform)
        {
            return transform.InverseTransformPoint(worldPosition);
        }
        
        protected static Quaternion LocalToWorldRotation(Quaternion localRotation, Transform transform)
        {
            return transform.rotation * localRotation;
        }
        
        protected static Quaternion WorldToLocalRotation(Quaternion worldRotation, Transform transform)
        {
            return Quaternion.Inverse(transform.rotation) * worldRotation;
        }
        
        #endregion
    }
}