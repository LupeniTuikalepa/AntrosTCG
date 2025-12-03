using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players.Local.Runtime.Controls
{
    public class RuntimePlayerControls : RuntimeLocalBattlePlayerComponent
    {
        public InputAction Pan { get; private set; }
        public InputAction Move { get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Use { get; private set; }

        [SerializeField]
        private PlayerInput playerInput;

        protected override void Awake()
        {
            base.Awake();

            Pan = playerInput.actions["Pan"];
            Move = playerInput.actions["Move"];
            Zoom = playerInput.actions["Zoom"];
            Use = playerInput.actions["Use"];
        }

        public override void Connect(IBattlePlayer player)
        {
            if (player is LocalBattlePlayer localBattlePlayer)
            {
                /*
                InputUser inputUser = localBattlePlayer.Profile.InputUser;
                playerInput.neverAutoSwitchControlSchemes = true;
                playerInput.SwitchCurrentControlScheme(inputUser.pairedDevices.ToArray());
                */
            }
        }

        public override void Disconnect(IBattlePlayer battlePlayer)
        {

        }
    }
}