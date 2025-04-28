using UnityEngine;

namespace Dive.VRModule
{
    public class PXRValveVariable : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Transform fakeHandParent;

        [SerializeField]
        private GameObject leftFakeHand;

        [SerializeField]
        private GameObject rightFakeHand;

        [SerializeField]
        private Transform headTransform;

        #endregion

        #region Public Properties

        public Transform FakeHandParent => fakeHandParent;
        public GameObject LeftFakeHand => leftFakeHand;
        public GameObject RightFakeHand => rightFakeHand;
        public Transform HeadTransform => headTransform;

        #endregion
    }
}