using System;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Cutscenes;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    [RequireComponent(typeof(PlayableDirector))]
    [AddComponentMenu("ATCG/Gameplay/Capacities/Capacity Cutscene")]
    public class CapacityCutscene : Cutscene
    {
        public event Action<CastCapacityPhase, Qte> OnQteWindowOpened;
        public event Action<CastCapacityPhase, Qte> OnQteWindowClosed;
        public event Action<CastCapacityPhase, Qte> OnQteResolved;

        private CastCapacityPhase phase;
        private ICutsceneActor caster;
        private Vector3 casterInitialPosition;
        private Quaternion casterInitialRotation;

        public bool IsHost =>
            phase != null
            && ScreenPlayer?.BattlePlayer != null
            && phase.casterPlayerId == ScreenPlayer.BattlePlayer.ID;

        public void Configure(CastCapacityPhase capacityPhase, RuntimeLocalBattlePlayer screenPlayer, IQteResultReceiver receiver)
        {
            phase = capacityPhase;

            CutsceneCapacityContext context = new(capacityPhase, screenPlayer, receiver);
            Configure(context);

            caster = context.GetCaster();
            if (caster != null)
            {
                casterInitialPosition = caster.transform.position;
                casterInitialRotation = caster.transform.rotation;
            }
        }

        protected override bool ShouldHookInput() => IsHost;

        protected override void OnArbiterBuilt(QteWindowArbiter windowArbiter)
        {
            windowArbiter.WindowOpened += qte => OnQteWindowOpened?.Invoke(phase, qte);
            windowArbiter.WindowClosed += qte => OnQteWindowClosed?.Invoke(phase, qte);
            windowArbiter.Resolved += qte => OnQteResolved?.Invoke(phase, qte);
        }

        public override void Dispose()
        {
            if (caster != null)
            {
                caster.transform.SetPositionAndRotation(casterInitialPosition, casterInitialRotation);
            }
            base.Dispose();
        }
    }
}
