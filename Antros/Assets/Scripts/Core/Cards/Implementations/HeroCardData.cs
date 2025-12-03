using UnityEngine;

namespace ATCG.Cards.Implementations
{
    [CreateAssetMenu(fileName = "GameCardData", menuName = "ATCG/Cards/Hero")]
    public class HeroCardData : GameCardData
    {
        [field: SerializeField, Range(1, 100)]
        public int Health { get; private set; } = 3;

        [field: SerializeField, Range(1, 10)]
        public int Strength { get; private set; } = 1;
        [field: SerializeField, Range(1, 5)]
        public int Speed { get; private set; } = 1;

        [field: SerializeField]
        public GameObject Prefab { get; private set; }
    }
}