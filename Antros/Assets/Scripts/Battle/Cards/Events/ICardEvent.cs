using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Cards.Capacities
{
    public interface ICardEvent<T> where T : ICardEventRunner
    {
        public Awaitable Run(List<T> runners);
    }

}