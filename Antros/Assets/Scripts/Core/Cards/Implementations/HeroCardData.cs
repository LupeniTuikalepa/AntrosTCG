using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Cards.Implementations
{
    [CreateAssetMenu(fileName = "GameCardData", menuName = "ATCG/Cards/Hero")]
    public class HeroCardData : GameCardData
    {
        [field: SerializeField, Range(1, 100), BoxGroup("Heroes")]
        public int Health { get; private set; } = 3;

        [field: SerializeField, Range(1, 100), BoxGroup("Heroes")]
        public int DeathCost { get; private set; } = 1;

        [field: SerializeField, Range(1, 10), BoxGroup("Heroes")]
        public int Strength { get; private set; } = 1;

        [field: SerializeField, Range(1, 5), BoxGroup("Heroes")]
        public int Speed { get; private set; } = 1;
    }
}