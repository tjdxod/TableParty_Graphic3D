using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Dive.VRModule
{
    public class PXRAttachable : PXRAttachableBase
    {
        private readonly Dictionary<PXRIntensifyInteractBase, Vector3> interactDictionary = new Dictionary<PXRIntensifyInteractBase, Vector3>();
        
        // ReSharper disable once UnusedMember.Local
        private void OnStayCollide(PXRIntensifyInteractBase interact)
        {
            if(interactDictionary.ContainsKey(interact))
            {
                var prevPosition = interactDictionary[interact];
                var comparePosition = interact.GetPosition().Item1;
                var moveDirection = comparePosition - prevPosition;
                var moveDistance = moveDirection.magnitude;
                var direction = moveDirection.normalized;
                
                Rigid.MovePosition(Rigid.position + direction * moveDistance);
                
                interactDictionary[interact] = interact.GetPosition().Item1;
            }
            else
            {
                interactDictionary.Add(interact, interact.GetPosition().Item1);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnExitCollide(PXRIntensifyInteractBase interact)
        {
            if (interactDictionary.ContainsKey(interact))
                interactDictionary.Remove(interact);
        }

        public PXRAttachable GetAttachable()
        {
            return this;
        }
    }
}