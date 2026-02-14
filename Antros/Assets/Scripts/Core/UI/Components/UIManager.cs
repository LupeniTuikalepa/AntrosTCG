using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace ATCG.UI
{
    public class UIManager : MonoBehaviour
    {
        [field: SerializeField]
        public EventSystem EventSystem { get; private set; }
        [field: SerializeField]
        public InputSystemUIInputModule InputModule { get; private set; }

        public InputActionAsset ActiveUIInputActionAsset => InputModule.actionsAsset;
    }
}