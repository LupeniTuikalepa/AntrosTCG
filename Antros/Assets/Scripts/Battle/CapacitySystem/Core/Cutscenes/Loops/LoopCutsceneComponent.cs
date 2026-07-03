using System;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Generic loop component: iterates a deterministic injected array of T, re-read
    /// every turn so runtime changes (e.g. a QTE growing the array) are picked up.
    /// Subclasses implement OnIteration to act on each element.
    /// </summary>
    public abstract class LoopCutsceneComponent<T> : LoopCutsceneComponentBase
    {
        private int index;

        // Name of the injected array property to iterate (e.g. "Targets").
        protected abstract string PropertyName { get; }

        // Resets the turn index when the component connects.
        protected override void OnConnect() => index = 0;

        // Reads the current element and advances. Re-reads the array each turn.
        protected override void RunTurn()
        {
            if (TryGetElements(out T[] elements) && index < elements.Length)
                OnIteration(elements[index], index);

            index++;
        }

        // Loops again while the (possibly updated) array still has elements ahead.
        protected override bool ShouldLoopAgain()
        {
            return TryGetElements(out T[] elements) && index < elements.Length;
        }

        // Reads the injected array fresh so a runtime change is reflected.
        private bool TryGetElements(out T[] elements)
        {
            if (phase.TryGetProperty(PropertyName, out T[] value) && value != null)
            {
                elements = value;
                return true;
            }

            elements = Array.Empty<T>();
            return false;
        }

        // Acts on one element for this turn.
        protected abstract void OnIteration(T element, int index);
    }
}