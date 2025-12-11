using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.Players.Runtime.Local
{
    public class RuntimePlayerControls : RuntimeLocalBattlePlayerComponent
    {
        public InputAction Pan { get; private set; }
        public InputAction Move { get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Use { get; private set; }

        [field: SerializeField]
        public PlayerInput PlayerInput { get; private set; }
        [field: SerializeField]
        public string KeyboardScheme { get; private set; }
        [field: SerializeField]
        public string GamepadScheme { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Pan = PlayerInput.actions["Pan"];
            Move = PlayerInput.actions["Move"];
            Zoom = PlayerInput.actions["Zoom"];
            Use = PlayerInput.actions["Use"];
        }

        protected override void Connect(LocalBattlePlayer player)
        {
        }

        protected override void Disconnect(LocalBattlePlayer battlePlayer)
        {

        }
    }
}