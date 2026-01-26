using Helteix.Tools.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Metrics
{
    [AutoGenerateGameSettings, GameSettingsPath("Antros/Gameplay Metrics")]
    public class GameplayMetrics : GameSettings<GameplayMetrics>
    {
        private struct CardCostPair
        {

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


        [field: SerializeField, BoxGroup("Grid"), Range(0, 30)]
        public uint GridRadius { get; private set; } = 7;

        [field: SerializeField, BoxGroup("Grid"), Range(0, 50)]
        public uint CellRadius { get; private set; } = 5;

        [field: SerializeField, BoxGroup("GameFeel"), Range(0, 30)]
        public float HeroMovementDuration { get; private set; } = .2f;

        [field: SerializeField, BoxGroup("GameFeel")]
        public Gradient PlayerColors { get; private set; }

        public Color GetPlayerColor(int playerID, int playerCount = 2) => PlayerColors.Evaluate(playerID / (float)playerCount - 1);
    }
}