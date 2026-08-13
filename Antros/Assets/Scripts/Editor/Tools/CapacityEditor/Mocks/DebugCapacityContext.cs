using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Editor.Tools.CutsceneEditor;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Cutscenes;
using ATCG.Battle.Entities.Runtime;
using ATCG.Capacities;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Editor-preview implementation of ICapacityContext. Declares the same closed
    /// schema from the capacity's PropertyDefinitions (so authored properties carry
    /// their tweaked test values), plus the preview built-ins (caster from the test
    /// hero, a flat coordinate solver). Property values edited in the tool window are
    /// pushed back in via InjectProperty, letting VFX be tuned without a running game.
    /// </summary>
    public sealed class DebugCapacityContext : ICapacityContext
    {
        private readonly CapacityPropertyBag bag = new();

        public DebugCapacityContext(CapacityData capacity, Transform heroRoot, Animator heroAnimator)
        {
            if (capacity != null)
                bag.Declare(capacity.PropertyDefinitions);

            bag.Allow<ICutsceneActor>(CutsceneContextKeys.CASTER);
            bag.Allow<ICutsceneCoordinateSolver>(CutsceneContextKeys.COORDINATE_SOLVER);
            bag.Allow<HexCoordinates>(CutsceneContextKeys.CAST_POINT);

            if (heroRoot != null)
                InjectProperty<ICutsceneActor>(CutsceneContextKeys.CASTER, new DebugCutsceneActor(heroRoot, heroAnimator));

            InjectProperty<ICutsceneCoordinateSolver>(
                CutsceneContextKeys.COORDINATE_SOLVER, new PreviewCoordinateSolver());
            InjectProperty(CutsceneContextKeys.CAST_POINT, default(HexCoordinates));

            // Seed authored properties with their saved editor debug values (EditorPrefs),
            // so the preview starts from the last-tweaked state.
            if (capacity != null)
            {
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(
                    UnityEditor.AssetDatabase.GetAssetPath(capacity));

                foreach (var def in capacity.PropertyDefinitions)
                {
                    if (def == null || string.IsNullOrEmpty(def.Name))
                        continue;
                    if (CapacityDebugValueStore.TryGet(guid, def, out object v))
                        bag.SetBoxed(def.Name, v);
                }
            }
        }

        public bool TryGetProperty<T>(string name, out T value) => bag.TryGet(name, out value);
        public void InjectProperty<T>(string name, T value) => bag.Set(name, value);

        // Lets the tool window push an edited value back into the schema (declared
        // properties only). Used by the property tweak panel.
        public bool TrySetBoxed(string name, object value)
        {
            if (!bag.IsDeclared(name))
                return false;
            bag.SetBoxed(name, value);
            return true;
        }
    }
}
