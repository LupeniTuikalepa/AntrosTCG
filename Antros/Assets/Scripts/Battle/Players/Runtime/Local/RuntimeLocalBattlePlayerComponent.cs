using UnityEngine;

namespace ATCG.Battle.Players.Runtime.Local
{
    public abstract class RuntimeLocalBattlePlayerComponent : MonoBehaviour, IRuntimeBattlePlayerComponent
    {
        public LocalBattlePlayer Player => RuntimePlayer.Current;
        public RuntimeLocalBattlePlayer RuntimePlayer { get; private set; }

        protected virtual void Awake()
        {
            RuntimePlayer = GetComponentInParent<RuntimeLocalBattlePlayer>();
        }


        protected abstract void Connect(LocalBattlePlayer player);
        protected abstract void Disconnect(LocalBattlePlayer player);

        void IRuntimeBattlePlayerComponent.Connect(IBattlePlayer player)
        {
            if(player is LocalBattlePlayer localBattlePlayer)
                Connect(localBattlePlayer);
        }

        void IRuntimeBattlePlayerComponent.Disconnect(IBattlePlayer player)
        {
            if(player is LocalBattlePlayer localBattlePlayer)
                Disconnect(localBattlePlayer);
        }
    }
}