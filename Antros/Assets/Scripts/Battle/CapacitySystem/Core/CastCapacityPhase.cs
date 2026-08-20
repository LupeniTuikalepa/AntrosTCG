using System;
using ATCG.Cutscenes;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Directors;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.CapacitySystem.Core.Setup;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Local.UI;
using ATCG.Capacities;
using ATCG.Capacities.Setup;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
using Helteix.ChanneledProperties;
using Helteix.Tools;
using Helteix.Tools.DataMapping;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ATCG.Battle.CapacitySystem.Core
{
    public class CastCapacityPhase : Phase, ICommandDirector<QteCommand>, IGlobalHUDPhase
    {
        public ChannelKey ChannelKey { get; }

        public IBattlePlayer CasterPlayer => battlePhase.GetPlayer(casterPlayerId);
        public bool HasCaster => caster.IsValid;

        public HexCoordinates CasterOrigin =>
            HasCaster && caster.TryGetComponentRO(out GridMemberComponent gridMember)
                ? gridMember.coordinates
                : castPoint;

        public readonly BattlePhase battlePhase;
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;
        public readonly EntityAddress caster;
        public readonly BattleID casterPlayerId;

        public Dictionary<RuntimeLocalBattlePlayer, CapacityDirector> directors;

        private readonly CapacityPropertyBag properties = new();
        private readonly HashSet<string> stepsRun = new();
        private readonly QteResultAccumulator qteResults = new();

        private Dictionary<string, ICapacityStep> stepsByName;
        private StepBarrier stepBarrier;

        public CastCapacityPhase(
            BattlePhase battlePhase,
            CapacityData data,
            HexCoordinates castPoint,
            EntityAddress caster,
            BattleID casterPlayerId)
        {
            ChannelKey = ChannelKey.GetUniqueChannelKey();
            this.battlePhase = battlePhase;
            this.data = data;
            this.castPoint = castPoint;
            this.caster = caster;
            this.casterPlayerId = casterPlayerId;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            qteResults.Clear();
            directors = DictionaryPool<RuntimeLocalBattlePlayer, CapacityDirector>.Get();
            stepsByName = DictionaryPool<string, ICapacityStep>.Get();
            properties.Declare(data.PropertyDefinitions);

            this.Register();

            CollectDirectors();
            stepBarrier = new StepBarrier(directors.Count);
            return base.Initialize(token);
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            using (CommandManager.BeginGroup($"[Cast Capacity] {data.Name}"))
            {
                if (!data.TryGet(out ICapacityContainer container))
                    return;

                if (CasterPlayer is LocalBattlePlayer localPlayer && !await RunSetups(container, localPlayer))
                    return;

                MapSteps(container);

                if (directors.Count == 0)
                {
                    foreach (ICapacityStep step in stepsByName.Values)
                        RunStepNow(step);
                    return;
                }

                await PlayAllDirectors(token);
            }
        }

        private async Awaitable<bool> RunSetups(ICapacityContainer container, LocalBattlePlayer localPlayer)
        {
            using HexPatternBuilder pattern = BuildHitPattern(container);
            CapacityTargets targets = ResolveTargets(container, pattern);

            foreach (var setup in data.Setups)
            {
                if (setup == null || !setup.TryGet(out ICapacitySetupContainer setupContainer))
                    continue;

                bool success = await setupContainer.Execute(setup, new CapacitySetupContext
                {
                    data = data,
                    caster = caster,
                    player = localPlayer,
                    battlePhase = battlePhase,
                    castPoints = castPoint,
                    targets = targets,
                    castCapacityPhase = this,
                });

                if (!success)
                    return false;
            }

            return true;
        }

        private void MapSteps(ICapacityContainer container)
        {
            foreach (ICapacityStep step in container.GetSteps(data, this))
                if (!stepsByName.TryAdd(step.StepName, step))
                    Debug.LogError($"[{data.Name}] Duplicate step '{step.StepName}' from Run.");
        }

        private async Awaitable PlayAllDirectors(CancellationToken token)
        {
            AwaitableCompletionSource allDone = new();
            int remaining = directors.Count;

            foreach ((RuntimeLocalBattlePlayer screenPlayer, CapacityDirector director) in directors)
                PlayDirector(director, screenPlayer, token, () =>
                {
                    if (--remaining <= 0)
                        allDone.TrySetResult();
                }).ListenForExceptions();

            await allDone.Awaitable;
        }

        private async Awaitable PlayDirector(
            CapacityDirector director, RuntimeLocalBattlePlayer screenPlayer, CancellationToken token, Action onDone)
        {
            try
            {
                await director.Play(this, screenPlayer, token);
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

        public void ReportStepReached(string stepName) => OnStepReportedAsync(stepName).ListenForExceptions();

        private async Awaitable OnStepReportedAsync(string stepName)
        {
            stepBarrier.Report(stepName);
            await stepBarrier.Await(stepName);

            if (!stepsRun.Add(stepName))
                return;

            if (stepsByName.TryGetValue(stepName, out ICapacityStep step))
                RunStepNow(step);
            else
                Debug.LogError($"[{data.Name}] Step marker '{stepName}' has no matching step in Run.");
        }

        private void RunStepNow(ICapacityStep step)
        {
            if (!data.TryGet(out ICapacityContainer container))
                return;

            using HexPatternBuilder pattern = BuildHitPattern(container);
            CapacityTargets targets = ResolveTargets(container, pattern);

            CapacityStepContext ctx = new(this, ReadQtes(), ResolveStepData(step.StepName), targets, pattern);
            step.RunStep(ctx);
        }

        private HexPatternBuilder BuildHitPattern(ICapacityContainer container)
        {
            HexPatternBuilder pattern = new(castPoint, new BattleIgnoreOriginPatternController(battlePhase.BattleGrid, castPoint));
            container.GetHitPattern(data, ref pattern, battlePhase.BattleGrid, castPoint, CasterOrigin);
            return pattern;
        }

        private CapacityTargets ResolveTargets(ICapacityContainer container, HexPatternBuilder pattern)
        {
            CapacityTargets targets = new();
            foreach (BattleCellAspect cell in pattern.GetBattleCells(battlePhase.BattleGrid))
                container.GetTargets(data, cell, targets, CasterPlayer);
            return targets;
        }

        private CapacityStepData ResolveStepData(string stepName)
        {
            data.TryGetStep(stepName, out CapacityStepData stepData);
            return stepData;
        }

        public bool TryGetProperty<T>(string name, out T value) => properties.TryGet(name, out value);

        public void InjectProperty<T>(string name, T value)
        {
            if (!properties.IsDeclared(name))
                properties.Allow<T>(name);

            properties.Set(name, value);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            this.Unregister();

            foreach ((_, CapacityDirector director) in directors)
                director.Dispose();

            DictionaryPool<RuntimeLocalBattlePlayer, CapacityDirector>.Release(directors);
            DictionaryPool<string, ICapacityStep>.Release(stepsByName);
            properties.Clear();

            return base.Dispose(token);
        }

        void ICommandDirector<QteCommand>.OnBegin(in CommandDirectorState state, CommandContext context, QteCommand command)
            => AddQteResult(command.Result);

        async Awaitable ICommandDirector<QteCommand>.Play(CommandDirectorState state, CommandContext context, QteCommand command)
        {
            state.CompleteAll(this);
            await Awaitable.MainThreadAsync();
        }

        public void AddQteResult(float qte) => qteResults.Add(qte);

        private float ReadQtes() => qteResults.Read();

        private void CollectDirectors()
        {
            foreach (IBattlePlayer player in battlePhase.Players)
            {
                if (player is LocalBattlePlayer localPlayer &&
                    RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(localPlayer, out RuntimeLocalBattlePlayer screenPlayer))
                {
                    CapacityCutscene cutscene = SpawnCutsceneFor(screenPlayer);
                    directors.Add(screenPlayer, new CapacityDirector(screenPlayer, casterPlayerId, cutscene));
                }
            }
        }

        public bool TryGetCapacityDirector(RuntimeLocalBattlePlayer player, out CapacityDirector director)
            => directors.TryGetValue(player, out director);

        private CapacityCutscene SpawnCutsceneFor(RuntimeLocalBattlePlayer player)
        {
            if (data.Director == null)
                return null;

            var instance = Object.Instantiate(data.Director, player.transform);
            return instance.GetComponent<CapacityCutscene>();
        }

        public bool TryGetRuntimeCaster(RuntimeLocalBattlePlayer player, out IRuntimeEntity runtimeEntity)
        {
            runtimeEntity = null;
            return HasCaster && player.RuntimeEntityManager.TryGetRuntimeEntity(caster, out runtimeEntity);
        }
    }
}
