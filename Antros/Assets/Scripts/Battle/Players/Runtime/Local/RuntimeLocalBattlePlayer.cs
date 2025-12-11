using ATCG.Battle.Players.Runtime.Local.CameraControls;
using ATCG.Battle.Players.Runtime.Local.UI.HUD;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime.Local
{
    public class RuntimeLocalBattlePlayer : RuntimeBattlePlayer<LocalBattlePlayer>
    {
        [field: SerializeField]
        public RuntimeLocalHUD HUD { get; private set; }

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