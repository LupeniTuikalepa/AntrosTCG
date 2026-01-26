using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Players;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local
{

    [DefaultExecutionOrder(-2)]
    [AddComponentMenu("ATCG/Gameplay/Player/Runtime/Local Player Controls")]
    public class RuntimeLocalPlayerControls : RuntimeLocalPlayerComponent
    {
        public InputAction Pan { get; private set; }
        public InputAction Move{ get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Use { get; private set; }

        public InputUser PlayerInputUser => Player.Profile.InputUser;

        [SerializeField]
        private InputSystemUIInputModule module;
        [SerializeField]
        private VirtualMouseInput virtualMouseInput;

        [field: SerializeField]
        public InputActionAsset InputActionAsset { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            InputActionAsset = Instantiate(InputActionAsset);
            InputActionAsset.Disable();

            module.actionsAsset = InputActionAsset;
            Pan = InputActionAsset["Battle/Pan"];
            Move = InputActionAsset["Battle/Move"];
            Zoom = InputActionAsset["Battle/Zoom"];
            Use = InputActionAsset["Battle/Use"];
        }

        private void OnDestroy()
        {
            Destroy(InputActionAsset);
        }

        protected override void Connect(LocalBattlePlayer player)
        {
            InputUser user = player.Profile.InputUser;
            InputActionAsset.name = $"{player.Profile.Infos.name} Controls";

            InputActionAsset.Enable();

            InputControlScheme? controlScheme = InputControlScheme.FindControlSchemeForDevices(
                user.pairedDevices,
                InputActionAsset.controlSchemes,
                allowUnsuccesfulMatch: true);

            if (controlScheme == null)
                return;

            user.ActivateControlScheme(controlScheme.Value);
            InputControlList<InputDevice> devices = InputUser.GetUnpairedInputDevices();
            foreach (InputDevice device in devices)
            {
                if (controlScheme.Value.SupportsDevice(device))
                    InputUser.PerformPairingWithDevice(device, user);
            }

            bool useVirtualMouse = controlScheme.Value.name == "Gamepad";
            virtualMouseInput.gameObject.SetActive(useVirtualMouse);
            if(useVirtualMouse)
                InputUser.PerformPairingWithDevice(virtualMouseInput.virtualMouse, user);

            user.AssociateActionsWithUser(InputActionAsset);
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {
            InputActionAsset.Disable();
        }

    }
}