using System;
using System.Collections.Generic;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Cards.Capacities.Behaviours.Patterns;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.Capacities.Data;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.GameCommands
{
    public sealed class CastCapacityCommand : GameCommand
    {
        public readonly struct Context
        {
            public readonly CastCapacityCommand evt;
            public readonly GameCommandContext gameCommandContext;
            public readonly CapacityContext capacityContext;

            public BattleGrid Grid => gameCommandContext.Grid;

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

            using (HashSetPool<BattleCellAspect>.Get(out HashSet<BattleCellAspect> targetedCells))
            {
                FillTargetedCells(targetedCells, in context);
                HitCells(targetedCells, in context);
            }
        }

        private void FillTargetedCells(HashSet<BattleCellAspect> targetedCells, in Context context)
        {
            ICapacityCastPatternData[] firePatternsData = capacityContext.data.FirePatterns;
            for (int i = 0; i < firePatternsData.Length; i++)
            {
                ICapacityCastPatternData firePatternData = firePatternsData[i];
                if (CapacityManager.TryGetFor(firePatternData, out ICapacityCastPattern castPattern))
                    castPattern.FillTargetedCells(firePatternData, in capacityContext, targetedCells);
            }
        }

        private void HitCells(IEnumerable<BattleCellAspect> targetedCells, in Context context)
        {
            using IDisposable disposable = FillMapping(out Dictionary<IEffectData, ICapacityEffect> mapping);
            CapacityData capacityData = capacityContext.data;

            foreach (BattleCellAspect battleCell in targetedCells)
            {
                //apply effects
                IEffectData[] hitEffects = capacityData.HitEffects;

                for (int i = 0; i < hitEffects.Length; i++)
                {
                    IEffectData hitData = hitEffects[i];
                    if (mapping.TryGetValue(hitData, out ICapacityEffect hitEffect))
                        hitEffect.TryApplyEffectTo(hitData, battleCell.EntityAddress, in context);
                }

                foreach (ComponentRef<BattleGridElementComponent> member in battleCell.GetMembers())
                {
                    for (int i = 0; i < hitEffects.Length; i++)
                    {
                        IEffectData data = hitEffects[i];
                        if (mapping.TryGetValue(data, out ICapacityEffect effect))
                            effect.TryApplyEffectTo(data, member.Address, in context);
                    }
                }
            }
        }

        private IDisposable FillMapping(out Dictionary<IEffectData, ICapacityEffect> hitEffectsMapping)
        {
            CapacityData data = capacityContext.data;
            var disposable = DictionaryPool<IEffectData, ICapacityEffect>.Get(out hitEffectsMapping);

            foreach (IEffectData hitEffectData in data.HitEffects)
                if (CapacityManager.TryGetFor(hitEffectData, out ICapacityEffect hitEffect))
                    hitEffectsMapping[hitEffectData] = hitEffect;

            return disposable;
        }
    }
}