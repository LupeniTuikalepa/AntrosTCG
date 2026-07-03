using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs.Timeline
{
    /// <summary>
    /// Track holding QTE clips. Bound to the cutscene (which implements
    /// IQteWindowHost) via the track binding, so every clip's behaviour receives
    /// the cutscene as playerData and can register its window. One QTE per clip;
    /// clip length = QTE duration.
    /// </summary>
    [TrackColor(0.9f, 0.6f, 0.1f)]
    [TrackClipType(typeof(QtePlayableAsset))]
    [TrackBindingType(typeof(CapacityCutscene))]
    public class QteTrack : TrackAsset { }
}