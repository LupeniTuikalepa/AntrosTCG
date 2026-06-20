
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.Capacities.Data;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Commands.GameCommands
{
    public sealed class CastCapacityCommand : Command<NoInfos>
    {
        private readonly CapacitySetup setup;

        public CastCapacityCommand(in CapacitySetup setup)
        {
            this.setup = setup;
        }

        protected override void Process(in CommandContext commandContext)
        {
            CapacityContext capacityContext = new(this, setup, commandContext);
            CapacityData capacityData = setup.data;

            using var patternBuilder = new HexPatternBuilder<BattlePatternController>(setup.castPoint, new(commandContext.Grid))
                .With(capacityData.FirePatterns);

            foreach (BattleCellAspect aspect in patternBuilder.GetBattleCells(capacityContext.BattleGrid))
            {
                //apply effects
                IEffectData[] hitEffects = capacityData.HitEffects;

                for (int i = 0; i < hitEffects.Length; i++)
                {
                    IEffectData hitData = hitEffects[i];
                    if (Mapper.TryGet<IEffectContainer>(hitData, out var container))
                        container.TryApply(hitData, aspect.EntityAddress, in capacityContext);
                }

                foreach (ComponentRef<GridMemberComponent> member in aspect.GetMembers())
                {
                    for (int i = 0; i < hitEffects.Length; i++)
                    {
                        IEffectData hitData = hitEffects[i];
                        if (Mapper.TryGet<IEffectContainer>(hitData, out var container))
                            container.TryApply(hitData, member.EntityAddress, in capacityContext);
                    }
                }
            }
        }
    }

}