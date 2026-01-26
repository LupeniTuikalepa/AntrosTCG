using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ATCG.UI
{
    public class UIComponent : MonoBehaviour
    {
        public UIManager Manager { get; private set; }

        public InputSystemUIInputModule InputModule => Manager.InputModule;
        public EventSystem EventSystem => Manager.EventSystem;

        protected virtual void Awake()
        {
            Manager = GetComponentInParent<UIManager>();
        }
    }
}