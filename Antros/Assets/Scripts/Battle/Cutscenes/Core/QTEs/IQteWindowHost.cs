using ATCG.Battle;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Implemented by the cutscene. A QTE clip registers on open and removes itself
    /// on close. The cutscene arbitrates press attribution (FIFO) across open
    /// windows — required because QTEs can overlap and a single press must resolve
    /// only ONE QTE.
    /// </summary>
    public interface IQteWindowHost
    {
        void SetQteData(BattleID qteID, QteClipData data, double time, double duration);
    }
}