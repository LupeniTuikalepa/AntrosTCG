using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Commands.GameCommands
{
    public sealed class CastCapacityCommand : GameCommand
    {
        public readonly struct Context
        {
            public readonly CastCapacityCommand evt;
            public readonly GameCommandContext gameCommandContext;
            public readonly CapacityContext capacityContext;

            public BattleGrid BattleGrid => gameCommandContext.Grid;

            public World World => gameCommandContext.battlePhase.world;

            public Context(CastCapacityCommand evt, CapacityContext capacityContext,
                GameCommandContext gameCommandContext)
            {
                this.evt = evt;
                this.capacityContext = capacityContext;
                this.gameCommandContext = gameCommandContext;
            }

            /// <summary>
            ///     Shortcut to embed a command to the main cast capacity command.
            /// </summary>
            /// <param name="command"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public void EmbedCommand<T>(T command) where T : GameCommand
            {
                evt.Embed(in gameCommandContext, command);
            }
        }

        private readonly CapacityContext capacityContext;

        public CastCapacityCommand(in CapacityContext capacityContext)
        {
            this.capacityContext = capacityContext;
        }

        protected override void Process(in GameCommandContext gameCommandContext)
        {
            Context context = new(this, capacityContext, gameCommandContext);
            CapacityData capacityData = capacityContext.data;

            HexPatternBuilder patternBuilder = new HexPatternBuilder(capacityContext.castPoint);
            for (int i = 0; i < capacityData.FirePatterns.Length; i++)
            {
                IHexCapacityPatternData patternData = capacityData.FirePatterns[i];
                if (CapacityManager.TryGetFor(patternData, out CapacityPatternMapper.IPatternContainer container))
                    container.AddToBuilder(patternData, ref patternBuilder);
            }

            foreach (BattleCellAspect aspect in patternBuilder.GetBattleCells(context.BattleGrid))
            {
                //apply effects
                IEffectData[] hitEffects = capacityData.HitEffects;

                for (int i = 0; i < hitEffects.Length; i++)
                {
                    IEffectData hitData = hitEffects[i];
                    if (CapacityManager.TryGetFor(hitData, out CapacityEffectMapper.IEffectContainer container))
                        container.TryApply(hitData, aspect.EntityAddress, in context);
                }

                foreach (ComponentRef<HexCoordinatesComponent> member in aspect.GetMembers())
                {
                    for (int i = 0; i < hitEffects.Length; i++)
                    {
                        IEffectData hitData = hitEffects[i];
                        if (CapacityManager.TryGetFor(hitData, out CapacityEffectMapper.IEffectContainer container))
                            container.TryApply(hitData, member.EntityAddress, in context);
                    }
                }
            }
        }
    }
}