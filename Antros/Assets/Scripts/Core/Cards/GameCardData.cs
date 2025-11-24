using System;
using ATCG.Capacities;
using ATCG.Databases;
using ATCG.Enums;
using Unity.Collections;
using UnityEngine;

namespace ATCG.Cards
{
    [CreateAssetMenu(fileName = "GameCardData", menuName = "ATCG/GameCardData")]
    public abstract class GameCardData : GameDatabaseObject
    {
        [field: SerializeField]
        public string Title { get; private set; }

        [field: SerializeField]
        public string Description { get; private set; }

        [field: SerializeField]
        public Element Element { get; private set; }

        [field: SerializeReference]
        public ICapacityDescriptions[] Capacities { get; private set; }

    }
}