using ATCG.Battle.Players.Local.UI;
using ATCG.Battle.Players.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime
{
    [AddComponentMenu("ATCG/Gameplay/Player/Runtime/Local Player")]
    public class RuntimeLocalBattlePlayer : RuntimeBattlePlayer<LocalBattlePlayer>
    {
        [field: SerializeField]
        public PlayerHUD HUD { get; private set; }

        [field: SerializeField]
        public RuntimeLocalPlayerControls Controls { get; private set; }

        [field: SerializeField]
        public RuntimeLocalPlayerCamera Camera { get; private set; }

        [ShowInInspector, ReadOnly]
        public int LocalID => runtimeBattlePlayers.IndexOf(this);


        protected override void OnConnected()
        {
            transform.position = new Vector3
            {
                x = Player.ID * 200
            };
        }

        protected override void OnDisconnected()
        {
        }
    }
}