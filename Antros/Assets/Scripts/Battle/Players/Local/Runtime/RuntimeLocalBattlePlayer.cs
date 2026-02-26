using System.Collections.Generic;
using ATCG.Battle.Players.Local.CameraControls;
using ATCG.Battle.Players.Runtime;
using ATCG.Battle.Players.Runtime.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Players.Local
{
    [AddComponentMenu("ATCG/Gameplay/Player/Runtime/Local Player")]
    public class RuntimeLocalBattlePlayer : RuntimeBattlePlayer<LocalBattlePlayer>
    {
        [ShowInInspector, ReadOnly]
        public int LocalID => runtimeBattlePlayers.IndexOf(this);

        [field: SerializeField]
        public PlayerHUD HUD { get; private set; }

        [field: SerializeField]
        public RuntimeLocalPlayerControls Controls { get; private set; }

        [field: SerializeField]
        public RuntimeLocalPlayerCamera Camera { get; private set; }



        protected override void OnConnected()
        {
            transform.position = new Vector3()
            {
                x = Player.ID * 200
            };
        }

        protected override void OnDisconnected()
        {

        }

    }
}