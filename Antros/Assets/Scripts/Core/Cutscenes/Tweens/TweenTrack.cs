// Assets/Scripts/Core/Cutscenes/TweenTrack.cs
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    [TrackColor(0.8f, 0.4f, 0.7f)]
    [TrackClipType(typeof(GoToClip))]
    [TrackBindingType(typeof(Transform))]
    public class TweenTrack : TrackAsset { }
}