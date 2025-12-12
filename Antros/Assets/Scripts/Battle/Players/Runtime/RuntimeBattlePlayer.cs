using UnityEngine;

namespace ATCG.Battle.Players.Runtime
{
    public abstract class RuntimeBattlePlayer : MonoBehaviour
    {
        public abstract void Disconnect();
    }

    public abstract class RuntimeBattlePlayer<T> : RuntimeBattlePlayer where T : class, IBattlePlayer
    {
        public T Player { get; private set; }

        private IRuntimeBattlePlayerComponent<T>[] runtimeComponents;

        private void Awake()
        {
            runtimeComponents = GetComponentsInChildren<IRuntimeBattlePlayerComponent<T>>();
        }


        public void Connect(T player)
        {
            if(Player != null)
                Disconnect();

            Player = player;
            OnConnected();

            for (int i = 0; i < runtimeComponents.Length; i++)
                runtimeComponents[i].Connect(this, player);
        }


        public sealed override void Disconnect()
        {
            if (Player != null)
            {
                OnDisconnected();
                Player = null;
            }

            for (int i = 0; i < runtimeComponents.Length; i++)
                runtimeComponents[i].Disconnect(this, Player);
        }
        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
    }
}