using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements
{
    /// <summary>
    /// Home for the bindings almost every capacity cutscene needs, so they don't have to
    /// be re-wired by hand on each one. Sits on the cutscene root, next to the
    /// PlayableDirector (CapacityCutscene already caches/restores the caster's original
    /// transform in Configure/Dispose, so rotating it here is safe — it snaps back once
    /// the cutscene ends).
    ///
    /// Currently just LookAtCastPoint; more common setup toggles will land here over time.
    /// </summary>
    public class DefaultCapacitySetup : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField]
        private Transform directorTransform;

        [Space]
        [SerializeField]
        private bool lookAtCastPoint;

        private void Reset() => directorTransform = transform;

        // Turns the caster AND the director root to face the cast point. In a real game
        // this resolves through CAST_POINT/COORDINATE_SOLVER (flattened to a pure yaw —
        // no pitch); in the Capacity Editor preview there's no meaningful cast point to
        // resolve, so both simply face Vector3.forward for a stable, predictable preview.
        public void Connect(ICapacityContext context)
        {
            if (!lookAtCastPoint)
                return;

            if (!context.TryGetProperty(CapacityContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            if (!context.TryGetProperty(CapacityContextKeys.CAST_POINT, out HexCoordinates castPoint) ||
                !context.TryGetProperty(CapacityContextKeys.COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver) ||
                solver == null)
            {
                return;
            }

            caster.LookAtCoord(castPoint, .2f);

            if (directorTransform != null)
            {
                Vector3 position = Application.isPlaying ? solver.ToWorld(castPoint) : directorTransform.position + Vector3.forward;
                directorTransform.LookAt(position);
            }
        }

        public void Disconnect()
        {

        }
    }
}