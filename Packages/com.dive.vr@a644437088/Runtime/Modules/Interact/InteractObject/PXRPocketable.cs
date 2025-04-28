using System;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 수납이 가능한 오브젝트 (현재 미사용)
    /// </summary>
    public class PXRPocketable : MonoBehaviour
    {
        /// <summary>
        /// 수납이 가능한지 판단
        /// </summary>
        [field: SerializeField]
        public bool CanPocketed { get; private set; } = true;

        /// <summary>
        /// 현재 수납된 상태인지 판단
        /// </summary>
        [field: SerializeField]
        public bool IsPocketed { get; private set; } = false;

        /// <summary>
        /// 수납된 경우 이벤트
        /// </summary>
        public event Action EnterPocket;
        
        /// <summary>
        /// 꺼낸 경우 이벤트
        /// </summary>
        public event Action ExitPocket;

        /// <summary>
        /// 수납 이벤트 실행
        /// </summary>
        public void DoEnterPocket()
        {
            IsPocketed = true;

            EnterPocket?.Invoke();
        }

        /// <summary>
        /// 꺼내기 이벤트 실행
        /// </summary>
        public void DoExitPocket()
        {
            IsPocketed = false;

            ExitPocket?.Invoke();
        }

        /// <summary>
        /// 수납 가능 상태로 변경
        /// </summary>
        public void EnableCanPocketed()
        {
            CanPocketed = true;
        }

        /// <summary>
        /// 수납 불가능 상태도 변경
        /// </summary>
        public void DisableCanPocketed()
        {
            CanPocketed = false;
        }
    }        
}
