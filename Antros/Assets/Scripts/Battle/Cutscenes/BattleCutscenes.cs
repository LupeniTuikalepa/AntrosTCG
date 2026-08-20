using System;
using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Cutscenes;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Cutscenes
{
    /// <summary>
    /// Battle-side glue for the generic <see cref="CutscenePlayer"/>: resolves the on-screen actors
    /// and screen players for a battle event, then plays a cutscene definition. Any event (physical
    /// attack, passive activation, card arrival) calls this with the acting entity and its step
    /// handlers (each a plain Action — run a command, a side effect, or nothing). A step handler runs
    /// once, when every screen crosses the matching marker.
    /// </summary>
    public static class BattleCutscenes
    {
        /// <summary>
        /// Plays <paramref name="definition"/> for the given source entity across every screen and
        /// completes when all screens' cutscenes have finished. A null definition is a no-op.
        /// </summary>
        public static Awaitable Play(
            CutsceneDefinition definition,
            BattlePhase battlePhase,
            EntityAddress source,
            params (string, Action)[] steps) => Play(definition, battlePhase, source, null, steps);

        /// <summary>
        /// Plays <paramref name="definition"/> for the given source entity across every screen and
        /// completes when all screens' cutscenes have finished. A null definition is a no-op.
        /// </summary>
        public static async Awaitable Play(
            CutsceneDefinition definition,
            BattlePhase battlePhase,
            EntityAddress source,
            QteResultAccumulator qteResults = null,
            params (string, Action)[] steps)
        {
            // The QTE owner is the player that owns the acting entity (e.g. the attacker); only that
            // screen turns a QTE press into a networked result.
            BattleID ownerPlayerId = source.TryGetComponentRO(out BelongsToPlayerComponent belongs)
                ? belongs.playerId
                : default;

            // When the caller wants QTE effectiveness, collect the (replicated) QteCommands into its
            // accumulator for the duration; a step handler reads the averaged effectiveness from it.
            QteResultCollector collector = null;
            if (qteResults != null)
            {
                qteResults.Clear();
                collector = new QteResultCollector(qteResults);
                collector.Register();
            }

            try
            {
                using (DictionaryPool<string, Action>.Get(out var stepsDic))
                {
                    for (int i = 0; i < steps.Length; i++)
                    {
                        stepsDic.TryAdd(steps[i].Item1, steps[i].Item2);
                    }
                    await new CutscenePlayer().PlayAsync(
                        definition,
                        Screens(battlePhase),
                        screen => BuildContext(screen, source, ownerPlayerId),
                        stepsDic);
                }
            }
            finally
            {
                collector?.Unregister();
            }
        }

        private static List<RuntimeLocalBattlePlayer> Screens(BattlePhase battlePhase)
        {
            List<RuntimeLocalBattlePlayer> screens = new();
            if (battlePhase == null)
                return screens;

            foreach (IBattlePlayer player in battlePhase.Players)
                if (player is LocalBattlePlayer local && local.GetRuntime() is RuntimeLocalBattlePlayer runtime)
                    screens.Add(runtime);

            return screens;
        }

        // Injects the well-known keys the cutscene elements expect for this screen: the screen
        // player, a coordinate solver, and the source actor (the acting entity's on-screen instance).
        private static ICutsceneContext BuildContext(
            RuntimeLocalBattlePlayer screen, EntityAddress source, BattleID ownerPlayerId)
        {
            CutsceneContext context = new();
            context.With(CutsceneContextKeys.SCREEN_PLAYER, screen);
            context.With<ICutsceneCoordinateSolver>(CutsceneContextKeys.COORDINATE_SOLVER, new GridCoordinateSolver(screen));
            context.With<IQteResultReceiver>(CutsceneContextKeys.QTE_RECEIVER,
                new CutsceneQteResultReceiver(screen, ownerPlayerId));

            if (screen.RuntimeEntityManager != null
                && screen.RuntimeEntityManager.TryGetRuntimeEntity(source, out IRuntimeEntity entity)
                && entity is ICutsceneActor actor)
                context.With(CutsceneContextKeys.CASTER, actor);

            return context;
        }
    }
}