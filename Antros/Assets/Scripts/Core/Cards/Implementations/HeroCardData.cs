using UnityEngine;

namespace ATCG.Cards.Implementations
{
    [CreateAssetMenu(fileName = "GameCardData", menuName = "ATCG/Cards/Hero")]
    public class HeroCardData : GameCardData
    {
        [field: SerializeField]
        public int Health { get; private set; }

        [field: SerializeField, Range(1, 5)]
        public int Speed { get; private set; }

        [field: SerializeField]
        public GameObject Prefab { get; private set; }
    }
}