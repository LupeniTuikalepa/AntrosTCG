// Assets/Scripts/Core/Cutscenes/ICutsceneCasterAnimatorSource.cs
using UnityEngine;

namespace ATCG.Core.Cutscenes
{
    /// <summary>
    /// Core-visible seam exposing the caster's Animator to Timeline clips in this assembly,
    /// which can't reference the Battle-side ICutsceneActor / injection context (ATCG.Battle
    /// depends on ATCG.Core, not the reverse). A Battle element that receives the injected
    /// caster implements this and forwards its Animator, so a Core clip (e.g. FollowBoneClip)
    /// resolves the caster through the existing injection system with no serialized reference
    /// of its own. May be null before the caster is connected, or for a casterless actor.
    /// </summary>
    public interface ICutsceneCasterAnimatorSource
    {
        Animator CasterAnimator { get; }
    }
}
