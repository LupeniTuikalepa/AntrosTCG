using System;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime
{
    public interface IRuntimePlayerFactory
    {
        bool TryCreateRuntimeFor(IBattlePlayer battlePlayer, out RuntimeBattlePlayer runtimeBattlePlayer);
    }

    [Serializable]
    public abstract class RuntimePlayerFactory<T> : IRuntimePlayerFactory where T : class, IBattlePlayer
    {
        [SerializeField]
        protected RuntimeBattlePlayer<T> runtimeBattlePlayerPrefab;

        public bool TryCreateRuntimeFor(IBattlePlayer battlePlayer, out RuntimeBattlePlayer runtimeBattlePlayer)
        {
            if (battlePlayer is T t)
            {
                runtimeBattlePlayer = SpawnPrefab(t);
                if (runtimeBattlePlayer is RuntimeBattlePlayer<T> compatible)
                {
                    compatible.Connect(t);
                    return true;
                }
            }

            runtimeBattlePlayer = null;
            return false;
        }

        protected virtual RuntimeBattlePlayer<T> SpawnPrefab(T battlePlayer)
        {
            return runtimeBattlePlayerPrefab.InstantiatePrefab();
        }
    }
}