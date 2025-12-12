using ATCG.Battle.Players.Local.CameraControls;
using ATCG.Battle.Players.Runtime;
using ATCG.Battle.Players.Runtime.UI;
using UnityEngine;

namespace ATCG.Battle.Players.Local
{
    public class RuntimeLocalBattlePlayer : RuntimeBattlePlayer<LocalBattlePlayer>
    {
        [field: SerializeField]
        public PlayerHUD HUD { get; private set; }

        [field: SerializeField]
        public RuntimeLocalPlayerControls Controls { get; private set; }

        [field: SerializeField]
        public RuntimeLocalPlayerCamera Camera { get; private set; }

        protected override void OnConnected()
        {

        }

        protected override void OnDisconnected()
        {

        }
    }
}