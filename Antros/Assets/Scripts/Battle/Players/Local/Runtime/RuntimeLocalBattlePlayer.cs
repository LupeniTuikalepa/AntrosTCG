using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime
{
    public class RuntimeLocalBattlePlayer : RuntimeBattlePlayer<LocalBattlePlayer>
    {
        [field: SerializeField]
        public RuntimePlayerControls Controls { get; private set; }
        [field: SerializeField]
        public RuntimePlayerCamera Camera { get; private set; }

        protected override void OnConnected()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnDisconnected()
        {
            throw new System.NotImplementedException();
        }
    }
}