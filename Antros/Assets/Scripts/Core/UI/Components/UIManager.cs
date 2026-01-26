using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Utilities;
using Unity.Networking.Transport.Error;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.Pool;

namespace ATCG.UI
{
    public class UIManager : MonoBehaviour
    {
        public event Action<InputActionAsset, InputActionAsset> OnNewUIInputActionAsset;
        public event Action<InputUser> OnUserChanged;


        [field: SerializeField]
        public EventSystem EventSystem { get; private set; }
        [field: SerializeField]
        public InputSystemUIInputModule InputModule { get; private set; }
        public InputActionAsset ActiveUIInputActionAsset { get; private set; }
        public InputUser CurrentUser { get; private set; }

        private void OnEnable()
        {
            Connect();

        }

        private void OnDisable()
        {
            Disconnect();
        }

        private void LateUpdate()
        {
            if (ActiveUIInputActionAsset == InputModule.actionsAsset)
                return;
            Disconnect();

            InputActionAsset last = ActiveUIInputActionAsset;
            ActiveUIInputActionAsset = InputModule.actionsAsset;
            Connect();
            RefreshUser();

            OnNewUIInputActionAsset?.Invoke(last, InputModule.actionsAsset);
        }

        private void Connect()
        {
            if(ActiveUIInputActionAsset == null)
                return;


            InputUser.onChange += InputUserOnChange;
        }

        private void Disconnect()
        {
            if(ActiveUIInputActionAsset == null)
                return;

            InputUser.onChange -= InputUserOnChange;
            ActiveUIInputActionAsset = null;
        }


        private void InputUserOnChange(InputUser user, InputUserChange change, InputDevice device)
        {
            switch (change)
            {
                case InputUserChange.Added:
                case InputUserChange.Removed:
                    RefreshUser();
                    return;
                case InputUserChange.DevicePaired:
                case InputUserChange.DeviceUnpaired:
                case InputUserChange.DeviceLost:
                case InputUserChange.DeviceRegained:
                case InputUserChange.ControlSchemeChanged:
                case InputUserChange.ControlsChanged:
                    if (user == CurrentUser)
                        OnUserChanged?.Invoke(CurrentUser);
                    break;
            }
        }

        private void RefreshUser()
        {
            using (ListPool<InputUser>.Get(out List<InputUser> users))
            {
                ActiveUIInputActionAsset.GetUsersListForActions(users);
                if(users.Count == 0)
                    return;

                if (users[0] == CurrentUser)
                    return;

                CurrentUser = users[0];
                OnUserChanged?.Invoke(CurrentUser);
                return;
            }
        }

    }
}