using System.Collections.Generic;
using Helteix.ControlDisplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace Helteix.ControlDisplay.UI
{
    public static class BindingDisplayEventManager
    {
        private static Dictionary<InputAction, List<IBindingDisplayEventListener>> listeners = new();

        static BindingDisplayEventManager()
        {
            InputSystem.onActionChange += OnActionChanges;
        }

        private static void OnActionChanges(object obj, InputActionChange change)
        {
            switch (change)
            {
                case InputActionChange.ActionDisabled:
                case InputActionChange.ActionEnabled:
                {
                    if (obj is InputAction action && listeners.TryGetValue(action, out var list))
                    {
                        if (BindingDisplaySettings.Current.CopyListenersListBeforeCallbacks)
                        {
                            using (ListPool<IBindingDisplayEventListener>.Get(out var temp))
                            {
                                temp.AddRange(list);
                                foreach (var listener in temp)
                                    listener?.OnEnableStateChanged(action);
                            }
                        }
                        else
                        {
                            foreach (var listener in list)
                                listener?.OnEnableStateChanged(action);
                        }
                    }
                    break;
                }

                case InputActionChange.BoundControlsChanged:
                {
                    if (obj is InputAction action)
                        SendActionChangedEvent(action);
                    if (obj is IInputActionCollection collection)
                    {
                        foreach (InputAction actionInCollection in collection)
                            SendActionChangedEvent(actionInCollection);
                    }
                    break;
                }
            }
        }

        private static void SendActionChangedEvent(InputAction action)
        {
            if (listeners.TryGetValue(action, out var list))
            {
                if (BindingDisplaySettings.Current.CopyListenersListBeforeCallbacks)
                {
                    using (ListPool<IBindingDisplayEventListener>.Get(out var temp))
                    {
                        temp.AddRange(list);
                        foreach (var listener in temp)
                            listener?.OnBindingChanged(action);
                    }
                }
                else
                {
                    foreach (var listener in list)
                        listener?.OnBindingChanged(action);
                }
            }
        }

        public static void Register(this IBindingDisplayEventListener listener, InputAction action)
        {
            if(!listeners.TryGetValue(action, out var list))
                listeners.Add(action, list = new());

            list.Add(listener);
        }

        public static void Unregister(this IBindingDisplayEventListener listener, InputAction action)
        {
            if(listeners.TryGetValue(action, out var list))
                list.Remove(listener);
        }
    }
}