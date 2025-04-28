using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    /// <summary>
    /// 게임 오브젝트가 파괴되지않도록
    /// </summary>
    public class DontDestroyObject : MonoBehaviour
    {
        private enum Type
        {
            Mine,
            SpecificComponent,
        }

        #region Public Fields

        [SerializeField]
        private Type type;

        [SerializeField, ShowIf(nameof(type), Type.SpecificComponent), InfoBox("SpecificComponent 설정의 경우 해당 컴포넌트가 현재 씬에\n2개 이상일 경우 파괴됩니다.")]
        private Component component;

        #endregion

        #region Private Methods

        private void Awake()
        {
            if (type == Type.Mine)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                var objs = FindObjectsOfType(component.GetType());

                if (objs.Length == 1)
                {
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        #endregion
    }
}