using UnityEngine;

namespace ATCG.Battle.Players
{
    public abstract class RuntimeBattlePlayer : MonoBehaviour
    {
        public abstract bool IsCompatibleWith(IBattlePlayer player);

        public abstract void Connect(IBattlePlayer player);
        public abstract void Disconnect(IBattlePlayer player);
    }

    public abstract class RuntimeBattlePlayer<T> : RuntimeBattlePlayer where T : class, IBattlePlayer
    {
        public sealed override bool IsCompatibleWith(IBattlePlayer player) => player is T;
        public T Current { get; private set; }


        public sealed override void Connect(IBattlePlayer player)
        {
            if(Current != null)
                Disconnect(Current);

            OnConnected();
            Current = (T)player;
        }

        public sealed override void Disconnect(IBattlePlayer player)
        {
            if (player == (IBattlePlayer)Current)
            {
                OnDisconnected();
                Current = null;
            }
        }

        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
    }
}