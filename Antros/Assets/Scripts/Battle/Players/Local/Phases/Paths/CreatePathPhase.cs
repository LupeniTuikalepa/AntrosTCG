using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ICreatePathPhase : ILocalPlayerPhase
    {
        public event Action<ICreatePathPhase> OnPathChanged;

        public HexCoordinates StartingPoint { get; }
        public List<HexCoordinates> CurrentPath { get;}
        public List<HexCoordinates> TemporaryPath { get;  }
    }

    public class CreatePathPhase<T> : LocalPlayerPhase<HexCoordinates[]>, ICreatePathPhase where T : IPathGenerator
    {

        private readonly struct GridFilter : IEntityFilter
        {
            public bool Accepts(EntityAddress entityAddress)
            {
                return entityAddress.Is<BattleCellAspect>(out var cell) && cell.CanBeMovedOn();
            }
        }
        public event Action<ICreatePathPhase> OnPathChanged;

        public HexCoordinates StartingPoint { get; }
        public PatternGroup PatternGroup { get; }
        public int Speed { get; }

        public List<HexCoordinates> CurrentPath { get; private set; }

        public List<HexCoordinates> TemporaryPath { get; private set; }

        public T PathGenerator { get; }

        public CreatePathPhase(LocalBattlePlayer localBattlePlayer,HexCoordinates startingPoint , int speed, PatternGroup patternGroup, T pathGenerator) : base(localBattlePlayer)
        {
            this.StartingPoint = startingPoint;
            this.Speed = speed;
            this.PatternGroup = patternGroup;
            PathGenerator = pathGenerator;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            CurrentPath = ListPool<HexCoordinates>.Get();
            TemporaryPath = ListPool<HexCoordinates>.Get();
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            ListPool<HexCoordinates>.Release(CurrentPath);
            ListPool<HexCoordinates>.Release(TemporaryPath);
            return base.Dispose(token);
        }

        protected override async Awaitable<HexCoordinates[]> Execute(CancellationToken token)
        {
            var filter = new GridFilter();
            var center = StartingPoint;
            MovementPatternController controller = new MovementPatternController(BattleGrid, center);

            CurrentPath.Add(StartingPoint);
            for (int i = 0; i < Speed; i++)
            {
                using HexPatternBuilder<MovementPatternController> builder =
                    new HexPatternBuilder<MovementPatternController>(center, controller)
                        .With(PatternGroup, center)
                        .Without(center);

                var selectEntityPhase =
                    new SelectEntityPhase<GridFilter, MovementPatternController>(LocalBattlePlayer, filter, builder);

                selectEntityPhase.OnEntityHovered += UpdateTemporaryPath;
                selectEntityPhase.OnEntityUnhovered -= ClearTemporaryPath;
                EntityAddress[] result = await selectEntityPhase;

                selectEntityPhase.OnEntityHovered -= UpdateTemporaryPath;
                selectEntityPhase.OnEntityUnhovered -= ClearTemporaryPath;

                TemporaryPath.Clear();
                if (result.Length <= 0)
                    return Array.Empty<HexCoordinates>();

                for (int j = 0; j < result.Length; j++)
                {
                    EntityAddress entityAddress = result[j];
                    if (entityAddress.TryGetComponentRO(out GridMemberComponent gridMemberComponent))
                    {
                        IEnumerable<HexCoordinates> pathBetween = PathGenerator.GetPathBetween(center, gridMemberComponent.coordinates, new PathGenerationContext(BattleGrid));
                        foreach (HexCoordinates coord in pathBetween)
                        {
                            if (center != coord)
                                CurrentPath.Add(coord);
                        }

                        center = CurrentPath[^1];
                    }
                }

                OnPathChanged?.Invoke(this);
            }

            return CurrentPath.ToArray();
        }

        private void ClearTemporaryPath(EntityAddress address)
        {
            TemporaryPath.Clear();
            OnPathChanged?.Invoke(this);
        }

        private void UpdateTemporaryPath(EntityAddress address)
        {
            if (address.TryGetComponentRO(out GridMemberComponent gridMemberComponent))
            {
                TemporaryPath.Clear();
                TemporaryPath.AddRange(PathGenerator.GetPathBetween(CurrentPath[^1], gridMemberComponent.coordinates, new PathGenerationContext(BattleGrid)));
                OnPathChanged?.Invoke(this);
            }
        }
    }
}