using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.Entities.Runtime;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Editor-preview stand-in for a caster: exposes the transform, renderers and
    /// animator taken from the test-environment hero, satisfying ICutsceneActor with
    /// no runtime World/ECS/player. Cutscene elements consume it like the real caster.
    /// </summary>
    public sealed class DebugCutsceneActor : ICutsceneActor
    {
        public Transform transform { get; }
        public Renderer[] Models { get; }
        public Animator Animator { get; }

        public DebugCutsceneActor(Transform root, Animator animator)
        {
            transform = root;
            Animator = animator;
            Models = root != null
                ? root.GetComponentsInChildren<Renderer>(true)
                : System.Array.Empty<Renderer>();
        }
    }
}