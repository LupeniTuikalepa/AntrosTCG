using System.Collections.Generic;
using Helteix.ControlDisplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace Helteix.ControlDisplay.UI
{
    public static class BindingDisplayEventManager
    {
        private static readonly Dictionary<InputAction, List<IBindingDisplayEventListener>> Listeners = new();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            InputSystem.onActionChange += OnActionChanges;
            Listeners.Clear();
        }

        private static void OnActionChanges(object obj, InputActionChange change)
        {
            switch (change)
            {
                case InputActionChange.ActionDisabled:
                case InputActionChange.ActionEnabled:
                {
                    if (obj is InputAction action && Listeners.TryGetValue(action, out var list))
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
            if (Listeners.TryGetValue(action, out var list))
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
            if(!Listeners.TryGetValue(action, out var list))
                Listeners.Add(action, list = new());

            list.Add(listener);
        }

        public static void Unregister(this IBindingDisplayEventListener listener, InputAction action)
        {
            if(Listeners.TryGetValue(action, out var list))
                list.Remove(listener);
        }
    }
}