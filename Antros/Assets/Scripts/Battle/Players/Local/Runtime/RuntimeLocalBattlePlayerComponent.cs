using System;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime
{
    public abstract class RuntimeLocalBattlePlayerComponent : MonoBehaviour, IRuntimeBattlePlayerComponent
    {
        public LocalBattlePlayer Player => RuntimePlayer.Current;
        public RuntimeLocalBattlePlayer RuntimePlayer { get; private set; }

        protected virtual void Awake()
        {
            RuntimePlayer = GetComponentInParent<RuntimeLocalBattlePlayer>();
        }

        public abstract void Connect(IBattlePlayer player);
        public abstract void Disconnect(IBattlePlayer battlePlayer);
    }
}