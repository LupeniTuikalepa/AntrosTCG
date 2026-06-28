using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Capacities;
using ATCG.Battle.CapacitySystem.Capacities;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Directors;
using ATCG.Battle.CapacitySystem.Directors;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Capacities;
using ATCG.HexGrids;
using Helteix.Tools.DataMapping;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.CapacitySystem.Core
{
    public class CastCapacityPhase : Phase, ICommandListener<QteCommand>
    {
        public readonly BattlePhase battlePhase;
        public readonly CapacityData data;

        public readonly HexCoordinates castPoint;
        public readonly EntityAddress caster;
        public readonly BattleID casterPlayerId;

        public List<ICapacityDirector> directors;

        private List<float> QTEs;

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
            QTEs = ListPool<float>.Get();
            directors = ListPool<ICapacityDirector>.Get();

            this.RegisterListener();

            CollectDirectors();
            return base.Initialize(token);
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            using (CommandManager.BeginGroup($"[Cast Capacity] {data.Name}"))
            {
                if (!data.TryGet(out ICapacityContainer capacityContainer))
                    return;

                for (int i = 0; i < directors.Count; i++)
                    await directors[i].Begin(this, token);

                foreach (ICapacityStep stepHolder in capacityContainer.Run(data, this))
                {
                    // Let every screen advance its cutscene to the next boundary.
                    // Owner emits QteCommand(s); others wait. QteCommands land via
                    // OnBegin and fill the stack before the flush below.
                    for (int i = 0; i < directors.Count; i++)
                        await directors[i].AdvanceToNextStep(token);

                    if (!data.TryGetStep(stepHolder.StepName, out CapacityStepData stepData))
                    {
                        Debug.LogError($"[{data.Name}] No data for step '{stepHolder.StepName}'. Skipping.");
                        continue;
                    }

                    float effectiveness = FlushQtes();
                    CapacityStepContext stepContext = new CapacityStepContext(this, effectiveness, stepData);
                    stepHolder.RunStep(stepContext);
                }

                for (int i = 0; i < directors.Count; i++)
                    await directors[i].End(token);
            }
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            this.UnregisterListener();

            ListPool<ICapacityDirector>.Release(directors);
            ListPool<float>.Release(QTEs);

            return base.Dispose(token);
        }

        // ---- QteCommand listener --------------------------------------------

        void ICommandListener<QteCommand>.OnBegin(in CommandListenerState state, CommandContext context, QteCommand command)
            => AddQteResult(command.Result);

        async Awaitable ICommandListener<QteCommand>.Play(CommandListenerState state, CommandContext context, QteCommand command)
        {
            state.CompleteAll(this);
            await Awaitable.MainThreadAsync();
        }

        // ---- QTE stack ------------------------------------------------------

        public void AddQteResult(float qte) => QTEs.Add(qte);

        private float FlushQtes()
        {
            if (QTEs.Count == 0)
                return 1f;

            float result = 0f;
            for (int i = 0; i < QTEs.Count; i++)
                result += QTEs[i];
            result /= QTEs.Count;

            QTEs.Clear();
            return result;
        }

        // ---- director collection -------------------------------------------

        // One director per screen. Each is given the casting player's id (routing)
        // and its own cutscene spawned from the capacity prefab. Iterating screens
        // (not entities) is what lets spell cards with no caster entity still work.
        private void CollectDirectors()
        {
            foreach (var player in battlePhase.Players)
            {
                if (player is LocalBattlePlayer localBattlePlayer &&
                    RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(localBattlePlayer,
                        out RuntimeLocalBattlePlayer screenPlayer))
                {
                    // TODO: spawn the cutscene prefab referenced by `data` for this
                    // screen and bind it. Until CapacityData exposes a prefab, inject
                    // null / a stub so the chain runs.
                    ICapacityCutscene cutscene = SpawnCutsceneFor(screenPlayer);

                    directors.Add(new CapacityDirector(screenPlayer, casterPlayerId, cutscene));
                }
            }
        }

        private ICapacityCutscene SpawnCutsceneFor(RuntimeLocalBattlePlayer screen)
        {
            // TODO: Object.Instantiate(data.CutscenePrefab) under `screen`, then
            // GetComponent<CapacityCutscene>(). Returns null for now.
            return null;
        }
    }
}