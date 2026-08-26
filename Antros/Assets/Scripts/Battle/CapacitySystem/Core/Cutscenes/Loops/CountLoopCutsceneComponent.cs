using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Loop host that replays the segment a fixed number of times, where the count is an int
    /// property injected into the cutscene context (e.g. a capacity parameter). Point a LoopClip
    /// at this like any other LoopCutsceneComponentBase. The count is re-read every turn, so a
    /// runtime change to the value is picked up. It carries no per-turn action beyond counting —
    /// the segment's own tracks are what replay.
    ///
    /// The segment always plays at least once (the clip is traversed once before the first
    /// "loop again?" check), so the value is the total number of plays and 0 or 1 both play once.
    /// </summary>
    public class CountLoopCutsceneComponent : LoopCutsceneComponentBase
    {
        [SerializeField]
        [Tooltip("Name of the injected int property giving how many times to play the segment.")]
        private string countProperty;

        private int index;

        // Reset the play counter when the component connects.
        protected override void OnConnect() => index = 0;

        // One traversal of the segment counts as one play.
        protected override void RunTurn() => index++;

        // Re-read the (possibly changed) count each turn; loop while plays remain.
        protected override bool ShouldLoopAgain()
        {
            int count = context != null && context.TryGetProperty(countProperty, out int value) ? value : 0;
            return index < count;
        }
    }
}
