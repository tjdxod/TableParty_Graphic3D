using UnityEngine;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    public abstract class PXRPointerBase : MonoBehaviour
    {
        #region Private Fields

        protected bool canProcess = true;

        private PointerEventData pointerEventData;

        #endregion

        #region Public Properties

        public bool CanProcess { get; private set; }
        public Camera EventCamera { get; private set; }

        public PointerEventData PointerEventData => pointerEventData ??= new PointerEventData(FindObjectOfType<EventSystem>());

        [field: SerializeField]
        public GameObject CurrentObjectOnPointer { get; set; }

        #endregion

        #region Public Methods

        public abstract void Process(out bool isValid);

        #endregion

        #region Private Methods

        protected virtual void Awake()
        {
            EventCamera = GetComponent<Camera>();
        }

        #endregion
    }
}