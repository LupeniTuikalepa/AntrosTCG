using Helteix.Tools.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Metrics
{
    [AutoGenerateGameSettings]
    public class GameplayMetrics : GameSettings<GameplayMetrics>
    {
        [field: SerializeField, BoxGroup("Game"), Range(0, 30)]
        public int MaxMana { get; private set; } = 10;
        [field: SerializeField, BoxGroup("Game")]
        public int MaxHealth { get; private set; } = 200;


        [field: SerializeField, BoxGroup("Game"), PropertyRange(0, nameof(MaxMana))]
        public int RecoveredManaOnTurnStart { get; private set; } = 2;
        [field: SerializeField, BoxGroup("Game"), PropertyRange(0, nameof(MaxMana))]
        public int PlayerHandSize { get; private set; } = 6;
        [field: SerializeField, BoxGroup("Game"), PropertyRange(1, nameof(PlayerHandSize))]
        public int PlayerDeployedHeroCount { get; private set; } = 5;


        [field: SerializeField, BoxGroup("Grid"), Range(0, 30)]
        public uint GridRadius { get; private set; } = 7;

        [field: SerializeField, BoxGroup("Grid"), Range(0, 50)]
        public uint CellRadius { get; private set; } = 5;
        
        [field: SerializeField, BoxGroup("GameFeel"), Range(0, 30)]
        public float HeroMovementDuration { get; private set; } = .2f;

        [field: SerializeField, BoxGroup("FX")]
        public string HoveredLayerMaskName { get; private set; }

    }
}