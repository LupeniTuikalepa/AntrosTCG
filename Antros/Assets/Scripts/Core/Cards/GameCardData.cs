using System;
using ATCG.Capacities;
using ATCG.Databases;
using ATCG.Enums;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace ATCG.Cards
{
    public abstract class GameCardData : GameDatabaseObject
    {
        [field: SerializeField, BoxGroup("Common")]
        public string Title { get; private set; }

        [field: SerializeField, TextArea, BoxGroup("Common")]
        public string Description { get; private set; }

        [field: SerializeField, BoxGroup("Common")]
        public Element Element { get; private set; }

        [field: SerializeField, BoxGroup("Common")]
        public CardRarity Rarity { get; private set; }

        [field: SerializeReference, BoxGroup("Common")]
        public CapacityData[] Capacities { get; private set; }

    }
}