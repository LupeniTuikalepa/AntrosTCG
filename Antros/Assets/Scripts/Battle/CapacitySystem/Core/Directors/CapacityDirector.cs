using System.Threading;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Directors;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Directors
{
    /// <summary>
    /// One per screen. Plays its cutscene through and relays step markers to the
    /// phase. The phase barriers reports across all screens and runs the step once
    /// all have reported. QTE ownership (input vs observe) is decided by comparing
    /// this screen's player to the casting player — used by the QTE clip logic.
    /// </summary>
    public class CapacityDirector : ICapacityDirector
    {
        public CastCapacityPhase Phase { get; private set; }

        public readonly RuntimeLocalBattlePlayer runtimePlayer;
        public readonly BattleID casterPlayerId;
        public readonly ICapacityCutscene cutscene;


        public bool IsOwner =>
            runtimePlayer.BattlePlayer != null
            && runtimePlayer.BattlePlayer.ID == casterPlayerId;

        public CapacityDirector(
            RuntimeLocalBattlePlayer runtimePlayer,
            BattleID casterPlayerId,
            ICapacityCutscene cutscene)
        {
            this.runtimePlayer = runtimePlayer;
            this.casterPlayerId = casterPlayerId;
            this.cutscene = cutscene;
        }

        public async Awaitable Play(CastCapacityPhase phase,  RuntimeLocalBattlePlayer screenPlayer, CancellationToken token)
        {
            this.Phase = phase;

            if (cutscene == null)
            {
                await Awaitable.MainThreadAsync();
                return;
            }

            // Tell the cutscene its role so it knows whether to read QTE input and
            // emit the QteCommand. Only the owner screen does.
            if (cutscene is CapacityCutscene concrete)
                concrete.Configure(phase, screenPlayer);

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

        // Relay the step marker to the phase barrier. The phase runs the step once
        // every screen has reported it.
        private void OnStepReached(string stepName) => Phase.ReportStepReached(stepName);

        public void Dispose()
        {
            cutscene?.Dispose();
        }
    }
}