using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Dive.VRModule
{
    public class PXRAttachableBase : MonoBehaviour, IAttachable
    {
        protected Rigidbody Rigid { get; private set; }
        
        public PXRAttachableBase GetAttachableBase()
        {
            return this;
        }
        
        protected void Awake()
        {
            Rigid = GetComponent<Rigidbody>();
            // StayEvent += OnStayCollide;
            // ExitEvent += OnExitCollide;
        }

    }
}
