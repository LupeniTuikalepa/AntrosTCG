using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.UI;
using ATCG.Battle.Players.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime
{
    [AddComponentMenu("ATCG/Gameplay/Player/Runtime/Local Player")]
    public class RuntimeLocalBattlePlayer : RuntimeBattlePlayer<LocalBattlePlayer>
    {
        public ComponentCache<PlayerHUD> HUD { get; private set; }
        public ComponentCache<PlayerControls> Controls { get; private set; }
        public ComponentCache<PlayerCamera> Camera { get; private set; }
        public ComponentCache<PlayerInteractions> Interactions { get; private set; }

        [ShowInInspector, ReadOnly]
        public int LocalID => RuntimeBattlePlayers.IndexOf(this);

        protected override void Awake()
        {
            base.Awake();
            HUD = new ComponentCache<PlayerHUD>(this);
            Controls = new ComponentCache<PlayerControls>(this);
            Camera = new ComponentCache<PlayerCamera>(this);
            Interactions = new ComponentCache<PlayerInteractions>(this);
        }

        protected override void OnConnected()
        {
            gameObject.name = $"Local Player {LocalID}";
            transform.position = new Vector3
            {
                x = Player.ID * 200
            };
        }

        protected override void OnDisconnected()
        {

        }


        public static bool TryGetRuntimeLocalPlayerFor(LocalBattlePlayer player, out RuntimeLocalBattlePlayer runtimePlayer)
            => TryGetRuntimePlayerFor(player, out runtimePlayer);
    }
}