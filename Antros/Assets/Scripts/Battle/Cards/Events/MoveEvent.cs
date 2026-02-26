using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATCG.Cards.Implementations;
using ATCG.HexGrids;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Cards.Capacities
{
    public interface IMoveEventRunner : ICardEventRunner
    {
        Awaitable MoveTo(HexCoordinates coordinates);
    }

    public readonly struct MoveEvent : ICardEvent<IMoveEventRunner>
    {
        public readonly HeroBattleCard card;
        public readonly HexCoordinates destination;

        public MoveEvent(HeroBattleCard card, HexCoordinates destination)
        {
            this.card = card;
            this.destination = destination;
        }

        public async Awaitable Run(List<IMoveEventRunner> runners)
        {
            using (ListPool<Awaitable>.Get(out List<Awaitable> tasks))
            {
                foreach (IMoveEventRunner runner in runners)
                    tasks.Add(runner.MoveTo(destination));

                await tasks.WhenAll();
            }
        }
    }

}