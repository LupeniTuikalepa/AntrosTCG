using System;
using ATCG.Players;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

namespace ATCG.Battle.Players.Local
{
    public class RuntimeLocalPlayerControls : RuntimeLocalPlayerComponent
    {
        public InputAction Pan { get; private set; }
        public InputAction Move{ get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Use { get; private set; }

        public InputUser InputUser => Player.Profile.InputUser;

        [SerializeField]
        private InputSystemUIInputModule module;

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
            user.AssociateActionsWithUser(InputActionAsset);

            InputActionAsset.Enable();

            InputControlScheme? controlScheme = InputControlScheme.FindControlSchemeForDevices(
                user.pairedDevices,
                InputActionAsset.controlSchemes,
                allowUnsuccesfulMatch: true);

            if (controlScheme != null)
                user.ActivateControlScheme(controlScheme.Value);
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {
            InputActionAsset.Disable();
        }
    }
}