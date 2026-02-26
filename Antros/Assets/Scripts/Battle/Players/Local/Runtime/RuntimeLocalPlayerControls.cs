using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players.Local
{

    [DefaultExecutionOrder(-2)]
    [AddComponentMenu("ATCG/Gameplay/Player/Runtime/Local Player Controls")]
    public class RuntimeLocalPlayerControls : RuntimeLocalPlayerComponent, IPhaseListener<LocalPlayerTurnPhase>
    {
        public InputAction Pan { get; private set; }
        public InputAction Move{ get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Use { get; private set; }

        public InputUser PlayerInputUser => PlayerInput.user;
        [field: SerializeField]
        public PlayerInput PlayerInput { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Pan = PlayerInput.actions["Battle/Pan"];
            Move = PlayerInput.actions["Battle/Move"];
            Zoom = PlayerInput.actions["Battle/Zoom"];
            Use = PlayerInput.actions["Battle/Use"];
        }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }


        protected override void Connect(LocalBattlePlayer player)
        {
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseBegin(LocalPlayerTurnPhase phase)
        {
            if (phase.localPlayerTurn == Player)
                PlayerInput.ActivateInput();
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseEnd(LocalPlayerTurnPhase phase)
        {
            if (phase.localPlayerTurn == Player)
                PlayerInput.DeactivateInput();
        }
    }
}