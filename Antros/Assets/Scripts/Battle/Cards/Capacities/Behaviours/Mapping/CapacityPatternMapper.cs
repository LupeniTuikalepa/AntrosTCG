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
    public class CapacityPatternMapper : Mapper<IHexCapacityPatternData, CapacityPatternMapper.IPatternContainer>
    {

        public interface IPatternContainer : IContainer<IHexCapacityPatternData>
        {
            void AddToBuilder(IHexCapacityPatternData data, ref HexPatternBuilder builder);
        }
        private sealed class PatternContainer<TData, TBehaviour>
            : Container<TData, TBehaviour>, IPatternContainer
            where TData : IHexCapacityPatternData
            where TBehaviour : ICapacityHexPattern<TData>
        {
            public PatternContainer(TBehaviour behaviour) : base(behaviour) { }

            public void AddToBuilder(IHexCapacityPatternData data, ref HexPatternBuilder builder)
            {
                if(data is TData t)
                    behaviour.AddToBuilder(t, ref builder);
            }
        }

        public void Add<TData, TBehaviour>(TBehaviour behaviour)
            where TData : IHexCapacityPatternData
            where TBehaviour : ICapacityHexPattern<TData>
            => Register(new PatternContainer<TData, TBehaviour>(behaviour));
    }
}