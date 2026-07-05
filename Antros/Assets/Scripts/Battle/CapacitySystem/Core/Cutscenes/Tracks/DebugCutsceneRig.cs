using Unity.Cinemachine;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Stand-in binding targets for the capacity-editing scene. Runtime auto-bind
    /// (CutsceneChannels) resolves against a live CastCapacityPhase; this rig gives
    /// the editor tool something concrete to bind to while authoring, so timelines
    /// scrub correctly in edit mode with no Play Mode session.
    /// </summary>
    public class DebugCutsceneRig : MonoBehaviour
    {
        [SerializeField] private Animator heroAnimator;
        [SerializeField] private CinemachineBrain cinemachineBrain;

        public Animator HeroAnimator => heroAnimator;
        public CinemachineBrain CinemachineBrain => cinemachineBrain;
    }
}