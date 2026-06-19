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
    public class PatternMapper : Mapper<PatternData, PatternMapper.IPatternContainer>
    {

        public interface IPatternContainer : IContainer<PatternData>
        {
            void AddToBuilder(PatternData data, ref HexPatternBuilder builder);
            void AddToBuilder(PatternData data, ref HexPatternBuilder builder, HexCoordinates origin);
        }
        private sealed class PatternContainer<TData, TBehaviour, TPattern>
            : Container<TData, TBehaviour>, IPatternContainer
            where TData : PatternData
            where TBehaviour : IHexPatternGenerator<TData, TPattern>
            where TPattern : IHexPattern
        {
            public PatternContainer(TBehaviour behaviour) : base(behaviour) { }

            public void AddToBuilder(PatternData data, ref HexPatternBuilder builder)
            {
	            AddToBuilder(data, ref builder, builder.origin);
            }

            public void AddToBuilder(PatternData data, ref HexPatternBuilder builder, HexCoordinates origin)
            {
                if (data is TData t)
                {
                    TPattern pattern = behaviour.CreatePattern(t);
                    if (data.IsAdditive)
                    {
                        if(data.OverridePatternOrigin)
                            builder.With(pattern, origin + data.Offset);
                        else
                            builder.With(pattern, origin);
                    }
                    else
                    {
                        if(data.OverridePatternOrigin)
                            builder.Without(pattern, origin);
                        else
                            builder.Without(pattern, origin);
                    }
                }
            }
        }

        public void Add<TData, TBehaviour, TPattern>()
            where TData : PatternData
            where TBehaviour : IHexPatternGenerator<TData, TPattern>, new()
            where TPattern : IHexPattern

            => Register(new PatternContainer<TData, TBehaviour, TPattern>(new()));

    }
}