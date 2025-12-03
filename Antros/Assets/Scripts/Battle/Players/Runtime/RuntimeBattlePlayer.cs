using System;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public abstract class RuntimeBattlePlayer : MonoBehaviour
    {
        public abstract Awaitable Connect(IBattlePlayer player);
        public abstract Awaitable Disconnect(IBattlePlayer player);
    }

    public abstract class RuntimeBattlePlayer<T> : RuntimeBattlePlayer where T : class, IBattlePlayer
    {
        public T Current { get; private set; }

        private IRuntimeBattlePlayerComponent[] runtimeComponents;

        private void Awake()
        {
            runtimeComponents = GetComponentsInChildren<IRuntimeBattlePlayerComponent>();
        }

        public sealed override async Awaitable Connect(IBattlePlayer player)
        {
            if(Current != null)
                await Disconnect(Current);

            Current = (T)player;
            OnConnected();

            await Awaitable.EndOfFrameAsync();
            for (int i = 0; i < runtimeComponents.Length; i++)
                runtimeComponents[i].Connect(player);
        }

        public sealed override async Awaitable Disconnect(IBattlePlayer player)
        {
            if (player == Current)
            {
                OnDisconnected();
                Current = null;
            }

            await Awaitable.EndOfFrameAsync();
            for (int i = 0; i < runtimeComponents.Length; i++)
                runtimeComponents[i].Disconnect(player);
        }

        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
    }
}