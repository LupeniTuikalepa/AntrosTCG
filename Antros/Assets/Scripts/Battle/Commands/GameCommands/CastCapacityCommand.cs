using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Commands.GameCommands
{
    public sealed class CastCapacityCommand : GameCommand<CastCapacityCommand.Infos>
    {
        //TODO remplir avec des infos pertinentes pour l'UI et la verification
        public struct Infos
        {

        }

        private readonly CapacitySetup setup;

        public CastCapacityCommand(in CapacitySetup setup)
        {
            this.setup = setup;
        }

        protected override void Process(in CommandContext commandContext)
        {
            CapacityContext capacityContext = new(this, setup, commandContext);
            CapacityData capacityData = setup.data;

            HexPatternBuilder patternBuilder = new HexPatternBuilder(setup.castPoint);
            for (int i = 0; i < capacityData.FirePatterns.Length; i++)
            {
                CapacityPatternData patternData = capacityData.FirePatterns[i];
                if (CapacityManager.TryGetFor(patternData, out CapacityPatternMapper.IPatternContainer container))
                    container.AddToBuilder(patternData, ref patternBuilder);
            }

            foreach (BattleCellAspect aspect in patternBuilder.GetBattleCells(capacityContext.BattleGrid))
            {
                //apply effects
                IEffectData[] hitEffects = capacityData.HitEffects;

                for (int i = 0; i < hitEffects.Length; i++)
                {
                    IEffectData hitData = hitEffects[i];
                    if (CapacityManager.TryGetFor(hitData, out CapacityEffectMapper.IEffectContainer container))
                        container.TryApply(hitData, aspect.EntityAddress, in capacityContext);
                }

                foreach (ComponentRef<GridMemberComponent> member in aspect.GetMembers())
                {
                    for (int i = 0; i < hitEffects.Length; i++)
                    {
                        IEffectData hitData = hitEffects[i];
                        if (CapacityManager.TryGetFor(hitData, out CapacityEffectMapper.IEffectContainer container))
                            container.TryApply(hitData, member.EntityAddress, in capacityContext);
                    }
                }
            }
        }
    }

}