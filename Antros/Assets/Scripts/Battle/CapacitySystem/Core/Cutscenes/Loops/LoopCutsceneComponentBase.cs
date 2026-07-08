using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Non-generic base so a LoopClip's ExposedReference can point to it (exposed
    /// refs need a concrete UnityEngine.Object type). Owns the rewind mechanics and
    /// answers "loop again?" at each turn; the reason for looping lives in subclasses.
    ///
    /// Purely local: the rewind replays the segment (anim/particles) per turn. No
    /// cross-screen sync — determinism lives in the per-turn commands and in the
    /// deterministic data the subclass reads to decide.
    /// </summary>
    public abstract class LoopCutsceneComponentBase : MonoBehaviour, ICapacityCutsceneElement, ILoopClipHost
    {
        [SerializeField]
        protected PlayableDirector playableDirector;

        // The driving context (game phase or editor preview). Subclasses read their
        // deterministic data (injected arrays, refs) from it via TryGetProperty.
        protected ICapacityContext context;

        private double segmentStart;

        // Element hook: cache the context. Count is resolved per turn.
        void ICapacityCutsceneElement.Connect(ICapacityContext capacityContext)
        {
            context = capacityContext;
            OnConnect();
        }

        void ICapacityCutsceneElement.Disconnect()
        {

        }

        // Stores the rewind point for this segment.
        public void OnLoopSegmentStart(double clipStart, double clipEnd)
        {
            segmentStart = clipStart;
        }

        // Runs the current turn, then rewinds if another turn is due.
        public bool OnLoopSegmentEnd()
        {
            RunTurn();

            bool again = ShouldLoopAgain();
            if (again)
                playableDirector.time = segmentStart;

            return again;
        }

        // Optional per-connect setup for subclasses.
        protected virtual void OnConnect() { }

        // Runs one turn (advance index, act, emit command if owner...).
        protected abstract void RunTurn();

        // Re-evaluated each turn so the count can change at runtime. Must read
        // deterministic data only.
        protected abstract bool ShouldLoopAgain();
    }
}