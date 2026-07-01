using System;
using System.Threading;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Presentation-only, one per screen. Plays a timeline; QTE clips drive the
    /// QTE windows (centralised here), and StepMarkers fire <see cref="StepReached"/>
    /// so the owning director can relay them to the phase. Knows nothing of the
    /// caster, the network, or the logical step execution — it only plays and
    /// reports.
    /// </summary>
    public interface ICapacityCutscene
    {
        /// <summary>Raised when the playhead crosses a StepMarker. Argument is the step name.</summary>
        event Action<string> StepReached;

        /// <summary>Start playing the timeline. Returns when the whole timeline has finished.</summary>
        Awaitable Play(CancellationToken token);

        /// <summary>Stop and release the timeline.</summary>
        Awaitable Stop(CancellationToken token);

        void Dispose();
    }
}