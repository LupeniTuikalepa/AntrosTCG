using System;
using ATCG.Battle.Entities.Core;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids.Entities
{
    public abstract class GridEntity : IEntityContainer, IDisposable
    {
        public BattleGrid Grid => BattlePhase.BattleGrid;

        public BattleCell BattleCell => Grid.GetBattleCell(Coordinates);

        public Entity Entity { get; }
        public BattlePhase BattlePhase { get; }
        public HexCoordinates Coordinates { get; private set; }



        public GridEntity(BattlePhase battlePhase)
        {
            BattlePhase = battlePhase;
            Entity = battlePhase.World.CreateEntity();
        }

        public void GoTo(HexCoordinates coordinates)
        {
            if(Grid.TryGetBattleCell(coordinates, out BattleCell from))
                from.RemoveMember(this);

            Coordinates = coordinates;

            if(Grid.TryGetBattleCell(coordinates, out BattleCell dest))
                dest.AddMember(this);
        }

        public virtual void OnEnterCell(BattleCell cell) { }
        public virtual void OnExitCell(BattleCell cell) { }

        public void Dispose()
        {
            GoTo(HexCoordinates.None);
        }
    }
}