using ATCG.Battle.Entities.Runtime.VFX;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// The narrow slice of a caster that cutscene elements consume: a transform to sit
    /// on, the renderers to drive VFX from, and the animator for bone-level binding.
    /// IRuntimeEntity implements it (its concrete entities already expose these), and
    /// the editor preview supplies a lightweight stand-in — so cutscene elements depend
    /// on this, not on the heavy runtime IRuntimeEntity. Part of the generic cutscene
    /// system (ATCG.Cutscenes) so every consumer — capacities, attacks, arrivals — shares it.
    /// Member names are intentionally lowercase where they mirror Unity's (transform),
    /// letting any MonoBehaviour or IRuntimeEntity satisfy the contract without remaps.
    /// Animator may be null for actors that have none — consumers must null-check.
    /// </summary>
    public interface ICutsceneActor : ILinkedRendererSource
    {
        Transform transform { get; }
        Animator Animator { get; }
        Awaitable LookAtCoord(HexCoordinates coordinates, float duration);
    }
}