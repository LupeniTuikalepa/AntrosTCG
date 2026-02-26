using System;
using System.Collections.Generic;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Cards
{
    public abstract class BattleCard<T> : GameCard<T>, IBattleCard where T : GameCardData
    {
        public bool IsDeployed => BattleGrid != null;
        public HexCoordinates Coordinates { get; protected set; }
        public BattleGrid BattleGrid { get; private set; }

        public IBattlePlayer Player { get; private set; }

        private List<object> eventRunners = new();

        protected BattleCard(T data, IBattlePlayer player) : base(data)
        {
            Player = player;
        }

        void IBattleCard.Deploy(BattleGrid grid, HexCoordinates coordinates)
        {
            Coordinates = coordinates;
            BattleGrid = grid;

            Player.AddOrRemoveMana(-Data.InvocationCost);
            OnDeploy();
        }

        void IBattleCard.Leave()
        {
            BattleGrid = null;
        }

        void IHexMember.EnterCell(HexCell hexCell)
        {
            if(BattleGrid.TryGetBattleCell(hexCell, out BattleCell cell))
                EnterCell(cell);
        }

        void IHexMember.LeaveCell(HexCell hexCell)
        {
            if(BattleGrid.TryGetBattleCell(hexCell, out BattleCell cell))
                LeaveCell(cell);
        }

        public void RegisterEventRunner<TEventRunner>(TEventRunner runner) where TEventRunner : ICardEventRunner
        {
            eventRunners.Add(runner);
        }

        public void UnregisterEventRunner<TEventRunner>(TEventRunner runner) where TEventRunner : ICardEventRunner
        {
            eventRunners.Remove(runner);
        }

        protected async Awaitable RunEvent<TCardEvent, TEventRunner>(TCardEvent cardEvent)
            where TCardEvent: ICardEvent<TEventRunner>
            where TEventRunner : ICardEventRunner
        {
            using (ListPool<TEventRunner>.Get(out List<TEventRunner> runners))
            {
                foreach (var er in eventRunners)
                {
                    if(er is TEventRunner tr)
                        runners.Add(tr);
                }

                await cardEvent.Run(runners);
            }
        }


        protected virtual void OnDeploy() { }
        protected virtual void EnterCell(BattleCell cell) { }
        protected virtual void LeaveCell(BattleCell cell) { }

    }
}