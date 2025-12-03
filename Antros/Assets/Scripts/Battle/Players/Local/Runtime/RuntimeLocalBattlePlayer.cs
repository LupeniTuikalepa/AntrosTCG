using ATCG.Battle.Players.Local.Runtime.Controls;
using ATCG.Battle.Players.Local.Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players.Local.Runtime
{
    public class RuntimeLocalBattlePlayer : RuntimeBattlePlayer<LocalBattlePlayer>
    {
        [field: SerializeField]
        public RuntimeLocalPlayerHUD HUD { get; private set; }

        [field: SerializeField]
        public RuntimePlayerControls Controls { get; private set; }

        [field: SerializeField]
        public RuntimePlayerCamera Camera { get; private set; }

        protected override void OnConnected()
        {

        }

        protected override void OnDisconnected()
        {

        }
    }
}