// Assets/Scripts/Core/Cutscenes/OrbitalRotateTrack.cs

using System.ComponentModel;
using Unity.Cinemachine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    [TrackColor(0.3f, 0.5f, 0.9f)]
    [TrackClipType(typeof(OrbitalRotateClip))]
    [TrackClipType(typeof(OrbitalSpeedRotateClip))]
    [TrackBindingType(typeof(CinemachineOrbitalFollow))]
    public class OrbitalRotateTrack : TrackAsset { }
}