using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Directors;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Capacities;
using ATCG.HexGrids;
using Helteix.Tools;
using Helteix.Tools.DataMapping;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ATCG.Battle.CapacitySystem.Core
{
    public class CastCapacityPhase : Phase, ICommandListener<QteCommand>, ATCG.Battle.CapacitySystem.Core.Properties.ICapacityContext
    {
        public IBattlePlayer CasterPlayer => battlePhase.GetPlayer(casterPlayerId);
        public bool HasCaster => caster.IsValid;
        public HexCoordinates CasterOrigin =>
            HasCaster && caster.TryGetComponentRO(out GridMemberComponent gridMemberComponent)?
            gridMemberComponent.coordinates:
            castPoint;

        public readonly BattlePhase battlePhase;
        public readonly CapacityData data;

        public readonly HexCoordinates castPoint;
        public readonly EntityAddress caster;
        public readonly BattleID casterPlayerId;

        private readonly ATCG.Battle.CapacitySystem.Core.Properties.CapacityPropertyBag properties = new();

        public Dictionary<RuntimeLocalBattlePlayer, CapacityDirector> directors;


        // Steps mapped by name (from Run's yield). Timeline markers pick which to
        // run; order of execution comes from the timeline, not the yield order.
        private Dictionary<string, ICapacityStep> stepsByName;

        // Barrier for steps across all screens. Built once directors are known.
        private StepBarrier stepBarrier;
        private readonly HashSet<string> stepsRun = new HashSet<string>();
        private List<float> qtes = new List<float>();

        public CastCapacityPhase(
            BattlePhase battlePhase,
            CapacityData data,
            HexCoordinates castPoint,
            EntityAddress caster,
            BattleID casterPlayerId)
        {
            this.battlePhase = battlePhase;
            this.data = data;
            this.castPoint = castPoint;
            this.caster = caster;
            this.casterPlayerId = casterPlayerId;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            qtes = ListPool<float>.Get();
            directors = DictionaryPool<RuntimeLocalBattlePlayer, CapacityDirector>.Get();
            stepsByName = DictionaryPool<string, ICapacityStep>.Get();
            properties.Declare(data.PropertyDefinitions);

            this.RegisterListener();

            CollectDirectors();
            stepBarrier = new StepBarrier(directors.Count);
            return base.Initialize(token);
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            using (CommandManager.BeginGroup($"[Cast Capacity] {data.Name}"))
            {
                if (!data.TryGet(out ICapacityContainer capacityContainer))
                    return;

                // Map the capacity's steps by name. Timeline markers will pick
                // which to run; a missing/duplicate name is a detectable error.
                foreach (ICapacityStep step in capacityContainer.GetSteps(data, this))
                {
                    if (!stepsByName.TryAdd(step.StepName, step))
                        Debug.LogError($"[{data.Name}] Duplicate step '{step.StepName}' from Run.");
                }

                // Play every screen's cutscene in parallel. Step markers reported
                // by the directors drive ProcessStep via the barrier. We finish
                // when all cutscenes have finished playing.
                int playing = directors.Count;
                if (playing == 0)
                {
                    // Headless / no screen: run all steps immediately, in yield order,
                    // with neutral effectiveness (no QTE possible without a screen).
                    foreach (ICapacityStep step in stepsByName.Values)
                        RunStepNow(step);
                    return;
                }

                AwaitableCompletionSource allDone = new AwaitableCompletionSource();
                int remaining = playing;

                foreach ((RuntimeLocalBattlePlayer screenPlayer, CapacityDirector capacityDirector) in directors)
                {
                    PlayDirector(capacityDirector, screenPlayer, token, () =>
                    {
                        remaining--;
                        if (remaining <= 0)
                            allDone.TrySetResult();
                    }).ListenForExceptions();
                }

                await allDone.Awaitable;
            }
        }

        private async Awaitable PlayDirector(CapacityDirector director,
            RuntimeLocalBattlePlayer screenPlayer, CancellationToken token, Action onDone)
        {
            try
            {
                await director.Play(this, screenPlayer, token);
            }
            finally
            {
                onDone();
            }
        }

        /// <summary>
        /// Called by a director when its cutscene crosses a StepMarker. Reports to
        /// the barrier; once ALL screens have reported this step, flush + run it once.
        /// </summary>
        public void ReportStepReached(string stepName) =>
            OnStepReportedAsync(stepName)
                .ListenForExceptions();

        public bool TryGetProperty<T>(string name, out T value) => properties.TryGet(name, out value);

        public void InjectProperty<T>(string name, T value) => properties.Set(name, value);

        private async Awaitable OnStepReportedAsync(string stepName)
        {
            stepBarrier.Report(stepName);
            await stepBarrier.Await(stepName);

            // Guard: the barrier releases once per screen report, but the step must
            // execute exactly once across all of them.
            if (!stepsRun.Add(stepName))
                return;

            if (stepsByName.TryGetValue(stepName, out ICapacityStep step))
                RunStepNow(step);
            else
                Debug.LogError($"[{data.Name}] Step marker '{stepName}' has no matching step in Run.");
        }

        private void RunStepNow(ICapacityStep step)
        {
            float effectiveness = ReadQtes();
            CapacityStepContext ctx = new CapacityStepContext(this, effectiveness, ResolveStepData(step.StepName));
            step.RunStep(ctx);
        }

        private CapacityStepData ResolveStepData(string stepName)
        {
            data.TryGetStep(stepName, out CapacityStepData stepData);
            return stepData;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            this.UnregisterListener();


            foreach ((var runtimeLocalBattlePlayer, CapacityDirector director) in directors)
                director.Dispose();

            DictionaryPool<RuntimeLocalBattlePlayer, CapacityDirector>.Release(directors);
            ListPool<float>.Release(qtes);
            DictionaryPool<string, ICapacityStep>.Release(stepsByName);
            properties.Clear();

            return base.Dispose(token);
        }

        // ---- QteCommand listener --------------------------------------------

        void ICommandListener<QteCommand>.OnBegin(in CommandListenerState state, CommandContext context,
            QteCommand command)
            => AddQteResult(command.Result);

        async Awaitable ICommandListener<QteCommand>.Play(CommandListenerState state, CommandContext context,
            QteCommand command)
        {
            state.CompleteAll(this);
            await Awaitable.MainThreadAsync();
        }

        // ---- QTE stack ------------------------------------------------------

        private bool consumedSinceLastFill;

        public void AddQteResult(float qte)
        {
            if (consumedSinceLastFill)
            {
                qtes.Clear();
                consumedSinceLastFill = false;
            }
            qtes.Add(qte);
        }

        private float ReadQtes()
        {
            consumedSinceLastFill = true;

            if (qtes.Count == 0)
                return 1f;

            float sum = 0f;
            for (int i = 0; i < qtes.Count; i++)
                sum += qtes[i];
            return sum / qtes.Count;
        }


        // ---- director collection -------------------------------------------

        private void CollectDirectors()
        {
            foreach (var player in battlePhase.Players)
            {
                if (player is LocalBattlePlayer localBattlePlayer &&
                    RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(localBattlePlayer,
                        out RuntimeLocalBattlePlayer screenPlayer))
                {
                    CapacityCutscene cutscene = SpawnCutsceneFor(screenPlayer);
                    CapacityDirector capacityDirector = new CapacityDirector(screenPlayer, casterPlayerId, cutscene);
                    directors.Add(screenPlayer, capacityDirector);
                }
            }
        }

        public bool TryGetCapacityDirector(RuntimeLocalBattlePlayer player, out CapacityDirector capacityDirector)
            => directors.TryGetValue(player, out capacityDirector);

        private CapacityCutscene SpawnCutsceneFor(RuntimeLocalBattlePlayer player)
        {
            if (data.CutsceneDirector == null)
                return null;

            var instance = Object.Instantiate(data.CutsceneDirector, player.transform);
            CapacityCutscene spawnCutsceneFor = instance.GetComponent<CapacityCutscene>();

            return spawnCutsceneFor;
        }

        public bool TryGetRuntimeCaster(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer,
            out IRuntimeEntity runtimeEntity)
        {
            runtimeEntity = null;
            if (!HasCaster)
                return false;

            return runtimeLocalBattlePlayer.RuntimeEntityManager.TryGetRuntimeEntity(caster, out runtimeEntity);
        }
    }
}