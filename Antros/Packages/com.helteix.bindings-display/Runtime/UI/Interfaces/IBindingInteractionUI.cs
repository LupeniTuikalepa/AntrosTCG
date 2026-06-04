using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.Interfaces
{
    public interface IBindingInteractionUI
    {
        void Sync(BindingDescription description, string interaction);
    }
}