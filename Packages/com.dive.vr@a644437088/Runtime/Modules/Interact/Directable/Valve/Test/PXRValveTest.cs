using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    public class PXRValveTest : MonoBehaviour
    {
        [SerializeField]
        private PXRTurnableValve valve;
        
        [SerializeField]
        private Transform trBarrier;
        
        // min = 0
        // max = 0.325
        
        void Start()
        {
            valve.ProgressEvent += MoveBarrier;
        }
        
        private void MoveBarrier(float ratio)
        {
            var x = trBarrier.localPosition.x;
            var z = trBarrier.localPosition.z;
            trBarrier.localPosition = new Vector3(x, Mathf.Lerp(0, 0.325f, ratio),  z);
        }        
    }
}
