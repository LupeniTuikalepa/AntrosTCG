using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Cutscenes;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Plays a <see cref="CutsceneDefinition"/> across every screen: it spawns one <see cref="Cutscene"/>
    /// per screen from the definition's director prefab, binds each to a per-screen context, plays them
    /// in parallel, and fires each declared step exactly once — when EVERY screen has crossed that
    /// step's marker (via <see cref="StepBarrier"/>). Consumers (physical attacks, passive activations,
    /// card arrivals) supply the per-screen context and the step handlers — a step handler is just an
    /// Action, free to run a command, trigger a side effect, or do nothing.
    ///
    /// This is the standalone, QTE-free player; the capacity system keeps its own richer flow for now.
    /// </summary>
    public sealed class CutscenePlayer
    {
        /// <summary>
        /// Plays the definition and completes when all screens' cutscenes have finished. If a screen's
        /// prefab has no <see cref="Cutscene"/> component it is skipped; if none remain, this is a no-op.
        /// </summary>
        public async Awaitable PlayAsync(
            CutsceneDefinition definition,
            IReadOnlyList<RuntimeLocalBattlePlayer> screens,
            Func<RuntimeLocalBattlePlayer, ICutsceneContext> contextFactory,
            IReadOnlyDictionary<string, Action> stepHandlers,
            CancellationToken token = default)
        {
            if (definition == null || definition.Director == null || screens == null)
                return;

            List<(RuntimeLocalBattlePlayer screen, Cutscene cutscene)> instances = new();
            foreach (RuntimeLocalBattlePlayer screen in screens)
            {
                if (screen == null)
                    continue;

                GameObject instance = Object.Instantiate(definition.Director.gameObject, screen.transform);

                // Director prefabs built from the shared template ship with a CapacityCutscene rather
                // than the generic Cutscene. Attach one on the fly so any cutscene kind plays through
                // this generic player (RequireComponent guarantees the PlayableDirector is there).
                Cutscene cutscene = instance.GetComponent<Cutscene>();
                if (cutscene == null)
                    cutscene = instance.AddComponent<Cutscene>();

                cutscene.Configure(contextFactory != null ? contextFactory(screen) : new CutsceneContext());
                instances.Add((screen, cutscene));
            }

            if (instances.Count == 0)
            {
                // No screen / no Cutscene component on the prefab: run the steps headless (in the
                // definition's declared order) so the gameplay still happens without visuals.
                RunHeadless(definition, stepHandlers);
                return;
            }

            StepBarrier barrier = new StepBarrier(instances.Count);
            HashSet<string> ran = new();

            foreach ((_, Cutscene cutscene) in instances)
                cutscene.StepReached += stepName =>
                    OnStepReached(stepName, barrier, ran, stepHandlers).ListenForExceptions();

            AwaitableCompletionSource allDone = new();
            int remaining = instances.Count;
            foreach ((RuntimeLocalBattlePlayer screen, Cutscene cutscene) in instances)
                PlayOne(screen, cutscene, token, () =>
                {
                    if (--remaining <= 0)
                        allDone.TrySetResult();
                }).ListenForExceptions();

            await allDone.Awaitable;

            foreach ((_, Cutscene cutscene) in instances)
                cutscene.Dispose();
        }

        // Runs every step handler once, in the definition's declared order, without any timeline.
        private static void RunHeadless(CutsceneDefinition definition, IReadOnlyDictionary<string, Action> handlers)
        {
            if (handlers == null)
                return;

            if (definition?.DeclaredSteps != null)
            {
                foreach (string step in definition.DeclaredSteps)
                    if (handlers.TryGetValue(step, out Action handler))
                        handler?.Invoke();
            }
            else
            {
                foreach (Action handler in handlers.Values)
                    handler?.Invoke();
            }
        }

        // Reported by a screen when its timeline crosses the step marker. Once all screens have
        // reported, the step's handler runs exactly once.
        private static async Awaitable OnStepReached(
            string stepName, StepBarrier barrier, HashSet<string> ran, IReadOnlyDictionary<string, Action> handlers)
        {
            barrier.Report(stepName);
            await barrier.Await(stepName);

            if (!ran.Add(stepName))
                return;

            if (handlers != null && handlers.TryGetValue(stepName, out Action handler))
                handler?.Invoke();
        }

        // Plays a screen's cutscene inside a per-player HUD phase, so that screen's HUD hides for the
        // duration and comes back when the cutscene ends.
        private static async Awaitable PlayOne(
            RuntimeLocalBattlePlayer screen, Cutscene cutscene, CancellationToken token, Action onDone)
        {
            try
            {
                await new LocalPlayerCutscenePhase(screen.BattlePlayer, cutscene).Run();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                onDone();
            }
        }
    }
}