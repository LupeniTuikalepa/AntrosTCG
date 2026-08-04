using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Per-clip impulse data: the impulse profile to fire (signal + envelope + channel) and
    /// the velocity handed to it, which sets the kick's direction and strength. The mixer
    /// fires this once when the clip is entered — duration and shape come from the profile's
    /// envelope, not the clip length.
    /// </summary>
    [Serializable]
    public sealed class ScreenShakeImpulseBehaviour : PlayableBehaviour
    {
        public CinemachineImpulseDefinition definition = new();
        public Vector3 velocity = new Vector3(0f, -1f, 0f);
    }
}
