namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Implemented by the loop component a clip points to (via ExposedReference). The
    /// clip only asks "should I loop again?" — it holds no count and no notion of
    /// what is iterated. The component owns the reason for looping and answers at the
    /// end of each turn.
    /// </summary>
    public interface ILoopClipHost
    {
        // Clip entered: gives the component the segment bounds to rewind to.
        void OnLoopSegmentStart(double clipStart, double clipEnd);

        // Turn ended: the component runs the turn and returns whether to loop again.
        bool OnLoopSegmentEnd();
    }
}