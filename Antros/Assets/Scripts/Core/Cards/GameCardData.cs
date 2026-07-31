using System;
using ATCG.Capacities;
using ATCG.Databases;
using ATCG.Enums;
using ATCG.Passives.Datas;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace ATCG.Cards
{
    public abstract class GameCardData : GameDatabaseObject
    {
        [field: SerializeField, BoxGroup("Common")]
        public string Title { get; private set; }

        [field: SerializeField, TextArea(5, 10), BoxGroup("Common")]
        public string Description { get; private set; }

        [field: SerializeField, BoxGroup("Common")]
        public Element Element { get; private set; }

        [field: SerializeField, BoxGroup("Common")]
        public CardRarity Rarity { get; private set; }

        [field: SerializeReference, BoxGroup("Common"), InlineProperty, HideLabel]
        public ICapacityDataProvider Capacities { get; private set; }

        [field: SerializeReference, BoxGroup("Common"), ListDrawerSettings(DefaultExpandedState = true, ShowFoldout = false)]
        public PassiveData[] Passives { get; private set; }

        protected override void Reset()
        {
            base.Reset();
            Capacities = new DefaultCapacityProvider();
        }
    }
}