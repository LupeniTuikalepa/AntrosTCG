namespace ATCG.Cutscenes
{
    /// <summary>
    /// Receives a QTE result [0,1] from the presentation layer. The implementation
    /// (the director) decides what to do with it — typically emit a QteCommand if
    /// this screen is the owner. Pulling emission up to here means a QTE can be
    /// SIMULATED by calling SubmitQteResult directly, with no real input or clip.
    /// </summary>
    public interface IQteResultReceiver
    {
        void SubmitQteResult(float score);
    }
}