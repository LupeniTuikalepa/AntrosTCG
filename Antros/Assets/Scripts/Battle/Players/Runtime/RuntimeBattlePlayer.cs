using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime
{
    public abstract class RuntimeBattlePlayer : MonoBehaviour
    {
        public abstract IBattlePlayer BattlePlayer { get; }
        public abstract void Disconnect();
    }


    public abstract class RuntimeBattlePlayer<T> : RuntimeBattlePlayer where T : class, IBattlePlayer
    {
        public class ComponentCache<TComponent> where TComponent : IRuntimeBattlePlayerComponent<T>
        {
            private readonly RuntimeBattlePlayer<T> player;

            public TComponent Component
            {
                get
                {
                    if(component == null)
                        player.TryGetPlayerComponent(out component);

                    return component;
                }
            }

            private TComponent component;

            public ComponentCache(RuntimeBattlePlayer<T> player)
            {
                this.player = player;
            }

            public static implicit operator TComponent(ComponentCache<TComponent> cache) => cache.Component;
        }

        protected static readonly List<RuntimeBattlePlayer<T>> RuntimeBattlePlayers = new();

        private IRuntimeBattlePlayerComponent<T>[] runtimeComponents;

        public override IBattlePlayer BattlePlayer => Player;
        public T Player { get; private set; }


        protected virtual void Awake()
        {
            runtimeComponents = GetComponentsInChildren<IRuntimeBattlePlayerComponent<T>>();
        }


        private void OnEnable()
        {
            RuntimeBattlePlayers.Add(this);
        }

        private void OnDisable()
        {
            RuntimeBattlePlayers.Remove(this);
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


        public bool TryGetPlayerComponent<TComponent>(out TComponent component) where TComponent : IRuntimeBattlePlayerComponent<T>
        {
            for (int i = 0; i < runtimeComponents.Length; i++)
            {
                if (runtimeComponents[i] is TComponent c)
                {
                    component = c;
                    return true;
                }
            }

            component = default;
            return false;
        }

        public static bool TryGetRuntimeLocalPlayerFor<TRuntimePlayer>(T player, out TRuntimePlayer runtimePlayer)
            where TRuntimePlayer : RuntimeBattlePlayer<T>
        {
            foreach (RuntimeBattlePlayer<T> runtimeBattlePlayer in RuntimeBattlePlayers)
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