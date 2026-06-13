using System.Collections.Generic;
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Cards.Capacities.Behaviours.Patterns;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Mapping
{
    public class CapacityPatternMapper : Mapper<CapacityPatternData, CapacityPatternMapper.IPatternContainer>
    {

        public interface IPatternContainer : IContainer<CapacityPatternData>
        {
            void AddToBuilder(CapacityPatternData data, ref HexPatternBuilder builder);
        }
        private sealed class PatternContainer<TData, TBehaviour, TPattern>
            : Container<TData, TBehaviour>, IPatternContainer
            where TData : CapacityPatternData
            where TBehaviour : ICapacityHexPattern<TData, TPattern>
            where TPattern : IHexPattern
        {
            public PatternContainer(TBehaviour behaviour) : base(behaviour) { }

            public void AddToBuilder(CapacityPatternData data, ref HexPatternBuilder builder)
            {
                if (data is TData t)
                {
                    TPattern pattern = behaviour.CreatePattern(t);
                    if (data.IsAdditive)
                    {
                        if(data.OverridePatternOrigin)
                            builder.With(pattern, builder.origin + data.Offset);
                        else
                            builder.With(pattern);
                    }
                    else
                    {
                        if(data.OverridePatternOrigin)
                            builder.Without(pattern, builder.origin);
                        else
                            builder.Without(pattern);
                    }
                }
            }
        }

        public void Add<TData, TBehaviour, TPattern>()
            where TData : CapacityPatternData
            where TBehaviour : ICapacityHexPattern<TData, TPattern>, new()
            where TPattern : IHexPattern

            => Register(new PatternContainer<TData, TBehaviour, TPattern>(new()));

    }
}