// Assets/Scripts/Core/Cutscenes/TweenTrack.cs
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    [DisplayName("ATCG/Tween Track")]
    [TrackColor(0.8f, 0.4f, 0.7f)]
    [TrackClipType(typeof(GoToClip))]
    [TrackClipType(typeof(FollowClip))]
    [TrackBindingType(typeof(Transform))]
    public class TweenTrack : TrackAsset { }
}