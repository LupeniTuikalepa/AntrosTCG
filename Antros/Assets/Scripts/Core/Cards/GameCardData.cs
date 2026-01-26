using System;
using ATCG.Capacities;
using ATCG.Databases;
using ATCG.Enums;
using Unity.Collections;
using UnityEngine;

namespace ATCG.Cards
{
    public abstract class GameCardData : GameDatabaseObject
    {
        [field: SerializeField]
        public string Title { get; private set; }

        [field: SerializeField, TextArea]
        public string Description { get; private set; }

        [field: SerializeField]
        public Element Element { get; private set; }
        [field: SerializeField, Min(0)]
        public int InvocationCost { get; private set; }


        [field: SerializeReference]
        public CapacityData[] Capacities { get; private set; }

    }
}