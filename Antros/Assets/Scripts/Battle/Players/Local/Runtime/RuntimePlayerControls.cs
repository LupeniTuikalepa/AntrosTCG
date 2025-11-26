using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.Players.Local.Runtime
{
    public class RuntimePlayerControls : RuntimeLocalBattlePlayerComponent
    {
        public InputAction Move => playerInput.actions["Move"];

        [SerializeField]
        private PlayerInput playerInput;

        public void ActivateInputs()
        {
            playerInput.ActivateInput();
        }

        public void DeactivateInputs()
        {
            playerInput.DeactivateInput();
        }

        public override void Connect(IBattlePlayer player)
        {

        }

        public override void Disconnect(IBattlePlayer battlePlayer)
        {

        }
    }
}