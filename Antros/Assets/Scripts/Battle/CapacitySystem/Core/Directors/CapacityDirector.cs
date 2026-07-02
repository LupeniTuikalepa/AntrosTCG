using System.Threading;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Directors
{
    /// <summary>
    /// One per screen. Plays its cutscene, relays step markers to the phase, and is
    /// the sink for QTE results: it decides — based on the OWNER role — whether a
    /// signalled score becomes a QteCommand. Keeping emission here (not in the
    /// cutscene) keeps presentation network-agnostic and lets a QTE be simulated by
    /// calling SubmitQteResult directly.
    /// </summary>
    public class CapacityDirector : IQteResultReceiver
    {
        public CastCapacityPhase Phase { get; private set; }

        public readonly RuntimeLocalBattlePlayer runtimePlayer;
        public readonly BattleID casterPlayerId;
        public readonly CapacityCutscene cutscene;

        public bool IsOwner =>
            runtimePlayer.BattlePlayer != null
            && runtimePlayer.BattlePlayer.ID == casterPlayerId;

        public CapacityDirector(
            RuntimeLocalBattlePlayer runtimePlayer,
            BattleID casterPlayerId,
            CapacityCutscene cutscene)
        {
            this.runtimePlayer = runtimePlayer;
            this.casterPlayerId = casterPlayerId;
            this.cutscene = cutscene;
        }

        public async Awaitable Play(CastCapacityPhase phase, RuntimeLocalBattlePlayer screenPlayer, CancellationToken token)
        {
            this.Phase = phase;

            if (cutscene == null)
            {
                await Awaitable.MainThreadAsync();
                return;
            }

            cutscene.Configure(phase, screenPlayer, this);
            cutscene.StepReached += OnStepReached;
            try
            {
                await cutscene.Play(token);
            }
            finally
            {
                cutscene.StepReached -= OnStepReached;
            }
        }

        public async Awaitable Stop(CancellationToken token)
        {
            if (cutscene != null)
                await cutscene.Stop(token);
            else
                await Awaitable.MainThreadAsync();
        }

        // IQteResultReceiver: only the owner turns a score into a command.
        public void SubmitQteResult(float score)
        {
            if (!IsOwner)
                return;

            IBattlePlayer casterPlayer = runtimePlayer.BattlePlayer;
            QteCommand qteCommand = new QteCommand(casterPlayer, score);
            qteCommand.Run(casterPlayer.BattlePhase);
        }

        private void OnStepReached(string stepName) => Phase.ReportStepReached(stepName);

        public void Dispose() => cutscene?.Dispose();
    }
}