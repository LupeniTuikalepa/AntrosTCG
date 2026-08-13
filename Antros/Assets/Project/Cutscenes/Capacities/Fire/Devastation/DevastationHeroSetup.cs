using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.Entities.Runtime;
using ATCG.HexGrids;
using UnityEngine;

using ATCG.Cutscenes;
namespace ATCG.Capacities.Devastation
{
    public class DevastationHeroSetup : MonoBehaviour, ICapacityCutsceneElement
    {
        public void Connect(ICutsceneContext context)
        {
            if (!context.TryGetProperty(CutsceneContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;
            if (!context.TryGetProperty(CutsceneContextKeys.CAST_POINT, out HexCoordinates castPoint))
                return;
            if (!context.TryGetProperty(CutsceneContextKeys.COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver))
                return;

            // Face the cast point. Reimplemented from IRuntimeEntity.LookAtCoord via the
            // actor transform so it works both in game and in the editor preview.
            Vector3 target = solver.ToWorld(castPoint);
            Vector3 toTarget = target - caster.transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
                caster.transform.rotation = Quaternion.LookRotation(toTarget);
        }

        public void Disconnect()
        {

        }
    }
}