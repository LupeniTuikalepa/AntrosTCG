using System;
using System.Linq;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players.Local.Runtime
{
    [DefaultExecutionOrder(-2), AddComponentMenu("ATCG/Gameplay/Player/Runtime/Local Player Controls")]
    public class PlayerControls : RuntimeLocalPlayerComponent, IPhaseListener<LocalPlayerTurnPhase>
    {
        [field: SerializeField]
        public PlayerInput PlayerInput { get; private set; }

        public InputAction Pan { get; private set; }
        public InputAction Move { get; private set; }
        public InputAction Rotate { get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Use { get; private set; }
        public InputAction Pointer { get; private set; }

        public InputUser PlayerInputUser => PlayerInput.user;

        public InputSystemUIInputModule InputModule => PlayerInput.uiInputModule;
        public EventSystem EventSystem
        {
            get
            {
                if (eventSystem == null)
                    eventSystem = InputModule.GetComponent<EventSystem>();

                return eventSystem;
            }
        }

        private EventSystem eventSystem;

        protected void Awake()
        {
            //Camera inputs
            Move = PlayerInput.actions["Battle/Move"];
            Zoom = PlayerInput.actions["Battle/Zoom"];
            Rotate = PlayerInput.actions["Battle/Rotate"];
            Pointer = PlayerInput.actions["Battle/Point"];
            Pan = PlayerInput.actions["Battle/Pan"];

            //Shortcuts
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

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseBegin(LocalPlayerTurnPhase phase)
        {
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseEnd(LocalPlayerTurnPhase phase)
        {
        }

        private void LateUpdate()
        {
            if(Player != null)
                Debug.Log($"Is connected to {Player.ID}", this);
        }

        protected override void Connect(LocalBattlePlayer player)
        {
            Debug.Log($"Connected controls with player {player.ID}", gameObject);
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {
            Debug.Log($"Disconnect controls with player {player.ID}", gameObject);
        }
    }
}