using System.ComponentModel;
using UnityEngine.Timeline;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Track holding QTE clips. Needs no binding: each clip's behaviour resolves the QTE host (the
    /// cutscene implementing IQteWindowHost) from the director's GameObject, so the same track works
    /// on any cutscene — capacity or generic. One QTE per clip; clip length = QTE duration.
    /// </summary>
    [DisplayName("ATCG/QTE Track")]
    [TrackColor(0.9f, 0.6f, 0.1f)]
    [TrackClipType(typeof(QtePlayableAsset))]
    public class QteTrack : TrackAsset { }
}