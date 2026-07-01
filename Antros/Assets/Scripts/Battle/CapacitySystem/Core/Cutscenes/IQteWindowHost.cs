namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Implemented by the cutscene. The QTE clip behaviour calls these as its clip
    /// enters/exits, so the cutscene (which knows the screen role and the input
    /// gateway) drives the actual QTE. The clip stays a thin trigger carrying only
    /// data (gauge prefab, etc.); the cutscene does the logic.
    ///
    /// normalizedTime is the playhead position WITHIN the clip [0..1], so the
    /// cutscene can tell whether the press landed in the critical window (the last
    /// portion, width from a global game metric) without owning timeline math.
    /// </summary>
    public interface IQteWindowHost
    {
        /// <summary>Clip entered: open the QTE window (show gauge, arm input on owner screen).</summary>
        void OnQteWindowEnter(QteClipData data);

        /// <summary>Each frame inside the clip: advance the gauge / check input. normalized in [0,1].</summary>
        void OnQteWindowTick(QteClipData data, double normalizedTime);

        /// <summary>Clip exited: close the window. If no press happened, this is a miss (result 0).</summary>
        void OnQteWindowExit(QteClipData data);
    }
}
