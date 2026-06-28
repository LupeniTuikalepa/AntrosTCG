using System.Threading;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Presentation-only. Lives on the capacity's prefab (referenced from the
    /// CapacityData) and binds its own PlayableDirector / VFX / QTE custom track.
    /// Knows NOTHING about the caster, the screens, or the network — it just
    /// plays a timeline and exposes its consumption points.
    ///
    /// A CapacityDirector (one per screen, created by the phase) owns and drives
    /// this. The same cutscene asset is used whether the cast comes from a hero
    /// or a spell card.
    /// </summary>
    public interface ICapacityCutscene
    {
        /// <summary>Start playback from the beginning (or hold on frame 0 until first advance).</summary>
        Awaitable Begin(CancellationToken token);

        /// <summary>
        /// Run the timeline up to the next QTE window and play it, returning the
        /// [0,1] result of the local player's input. Used by the OWNER screen.
        /// </summary>
        Awaitable<float> PlayNextQteWindow(CancellationToken token);

        /// <summary>
        /// Run the timeline up to the next consumption marker WITHOUT interacting,
        /// pausing there if the result has not been provided yet. Used by the
        /// NON-OWNER screen (it observes; the result arrives via QteCommand).
        /// <paramref name="resultProvided"/> lets the director unblock the marker
        /// once the matching QteCommand has been stacked.
        /// </summary>
        Awaitable AdvanceToNextConsumption(CancellationToken token);

        /// <summary>Stop and release the timeline.</summary>
        Awaitable End(CancellationToken token);
    }
}
