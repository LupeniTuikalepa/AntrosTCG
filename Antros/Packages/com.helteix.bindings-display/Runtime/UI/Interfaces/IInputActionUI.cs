using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.Interfaces
{
    public abstract class InputActionUI : MonoBehaviour
    {
        public InputAction CurrentAction { get; private set; }
        public bool IsConnected => CurrentAction != null;

        internal void ConnectUI(InputAction action)
        {
            if(IsConnected)
                DisconnectUI();

            CurrentAction = action;
            Connect(action);
        }

        internal void DisconnectUI()
        {
            if(!IsConnected)
                return;

            Disconnect();
        }

        public void Reconnect()
        {
            if (IsConnected)
            {
                DisconnectUI();
                ConnectUI(CurrentAction);
            }
        }

        protected abstract void Connect(InputAction action);
        protected abstract void Disconnect();

        protected virtual void OnDestroy()
        {
            if (IsConnected)
                Disconnect();
        }
    }
}