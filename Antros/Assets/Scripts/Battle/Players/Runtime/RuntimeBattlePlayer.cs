using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime
{
    public abstract class RuntimeBattlePlayer : MonoBehaviour
    {
        public abstract void Disconnect();
    }

    public abstract class RuntimeBattlePlayer<T> : RuntimeBattlePlayer where T : class, IBattlePlayer
    {
        protected static readonly List<RuntimeBattlePlayer<T>> runtimeBattlePlayers = new();

        private IRuntimeBattlePlayerComponent<T>[] runtimeComponents;

        public T Player { get; private set; }

        private void Awake()
        {
            runtimeComponents = GetComponentsInChildren<IRuntimeBattlePlayerComponent<T>>();
        }


        private void OnEnable()
        {
            runtimeBattlePlayers.Add(this);
        }

        private void OnDisable()
        {
            runtimeBattlePlayers.Remove(this);
        }


        public void Connect(T player)
        {
            if (Player != null)
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

        public static bool TryGetRuntimeLocalPlayerFor<TRuntimePlayer>(T player, out TRuntimePlayer runtimePlayer)
            where TRuntimePlayer : RuntimeBattlePlayer<T>
        {
            foreach (RuntimeBattlePlayer<T> runtimeBattlePlayer in runtimeBattlePlayers)
                if (runtimeBattlePlayer.Player == player && runtimeBattlePlayer is TRuntimePlayer rtp)
                {
                    runtimePlayer = rtp;
                    return true;
                }

            runtimePlayer = null;
            return false;
        }
    }
}