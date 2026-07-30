using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Cards.Implementations
{
    [CreateAssetMenu(fileName = "GameCardData", menuName = "ATCG/Cards/Construction")]
    public class ConstructionCardData : GameCardData
    {
        [field: SerializeField, Range(1, 100), BoxGroup("Construction")]
        public int Health { get; private set; } = 3;

        [field: SerializeField, Range(1, 100), BoxGroup("Construction")]
        public int DeathCost { get; private set; } = 1;
        
        [field: SerializeField, Range(1, 10), BoxGroup("Construction")]
        public int Defense { get; private set; } = 1;
        
        [field: SerializeField, Range(-1, 10), BoxGroup("Construction")]
        public int PassiveRange { get; private set; } = 3;

        [field: SerializeField, Range(0, 10), BoxGroup("Deploy")]
        public int DeployRange { get; private set; } = 1;
        
        [field: SerializeField, BoxGroup("Deploy")]
        public GameObject Prefab { get; private set; }
    }
}