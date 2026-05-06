using System;
using ATCG.Cards;
using Helteix.Tools.Settings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Metrics
{
    [AutoGenerateGameSettings, GameSettingsPath("Antros/Game Metrics")]
    public class GameMetrics : GameSettings<GameMetrics>
    {
        [Serializable]
        public struct DualPairing<TKey, TValue>
        {
            [Serializable]
            private struct Kvp
            {
                [field: SerializeField]
                public TKey Key { get; private set; }
                [field: SerializeField]
                public TValue Value { get; private set; }
            }

            [SerializeField, TableList(AlwaysExpanded = true)]
            private Kvp[] pairings;

            public bool TryGetValueForKey(TKey key, out TValue value)
            {
                using (DictionaryPool<TKey, TValue>.Get(out var dic))
                {
                    for (int i = 0; i < pairings.Length; i++)
                        dic.Add(pairings[i].Key, pairings[i].Value);

                    return dic.TryGetValue(key, out value);
                }
            }
        }

        [field: SerializeField, BoxGroup("Game"), Range(0, 30)]
        public int MaxMana { get; private set; } = 10;
        [field: SerializeField, BoxGroup("Game")]
        public int MaxHealth { get; private set; } = 200;

        [field: SerializeField, BoxGroup("Game"), PropertyRange(0, nameof(MaxMana))]
        public int RecoveredManaOnTurnStart { get; private set; } = 2;
        [field: SerializeField, BoxGroup("Game"), PropertyRange(0, nameof(MaxMana))]
        public int MinPlayerHandSize { get; private set; } = 6;
        [field: SerializeField, BoxGroup("Game"), PropertyRange(1, nameof(MinPlayerHandSize))]
        public int PlayerDeployedHeroCount { get; private set; } = 5;


        [field: SerializeField, BoxGroup("Game"), PropertyRange(0, nameof(GridRadius))]
        public int BasicAttackRange { get; private set; } = 1;
        [field: SerializeField, BoxGroup("Game")]
        public DualPairing<CardRarity, int> CardRarityDeathCost { get; private set; }

        [field: SerializeField, BoxGroup("Game/Costs"), PropertyRange(1, nameof(MinPlayerHandSize))]
        public int BasicAttackCost { get; private set; } = 1;
        [field: SerializeField, BoxGroup("Game/Costs"), PropertyRange(1, nameof(MinPlayerHandSize))]
        public int MovementCost { get; private set; } = 1;

        [field: SerializeField, BoxGroup("Game/Costs")]
        public DualPairing<CardRarity, int> CardRarityInvocationCost { get; private set; }


        [field: SerializeField, BoxGroup("RuntimeBattleGrid"), Range(0, 30)]
        public uint GridRadius { get; private set; } = 7;

        [field: SerializeField, BoxGroup("RuntimeBattleGrid"), Range(0, 50)]
        public uint CellRadius { get; private set; } = 5;


        [field: SerializeField, BoxGroup("GameFeel")]
        public Gradient PlayerColors { get; private set; }

        [field: SerializeField, BoxGroup("Layers")]
        public LayerMask CellLayer { get; private set; }
        [field: SerializeField, BoxGroup("Layers")]
        public LayerMask HeroesLayer { get; private set; }

        public Color GetPlayerColor(int playerID, int playerCount = 2)
        {
            float max = (float)playerCount - 1;
            Color color = PlayerColors.Evaluate(playerID / max);
            return color;
        }
    }
}