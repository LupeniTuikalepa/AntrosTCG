using System;
using System.Collections.Generic;
using ATCG.Capacities;
using ATCG.Databases;
using ATCG.Elements;
using ATCG.Enums;
using UnityEngine;

namespace ATCG
{
    [Serializable]
    public class ElementCapacityProvider : ICapacityDataProvider
    {
        [field: SerializeField]
        public List<Element> Elements { get; private set; }
        public IEnumerable<CapacityData> GetCapacities()
        {
            foreach (var capacityData in GameController.GameDatabase.GetAll<CapacityData>())
            {
                if (Elements.Contains(capacityData.Element))
                {
                    yield return capacityData;
                }
            }
        }
    }
}