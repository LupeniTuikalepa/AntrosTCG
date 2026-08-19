using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.Characters
{
    /// <summary>
    /// Synty spawns its work-in-progress character as a plain "Combined Character" GameObject in the
    /// active scene, which otherwise clutters the hierarchy and gets saved with the scene. Whenever it
    /// appears we flag it HideAndDontSave, so it stays out of the hierarchy and is never persisted — it
    /// still renders in the Scene view (HideInHierarchy only hides the list entry). Idempotent, so it
    /// won't loop when the flag change itself triggers another hierarchy change.
    /// </summary>
    [InitializeOnLoad]
    internal static class SidekickOutputHider
    {
        private const string OutputModelName = "Combined Character";

        static SidekickOutputHider()
        {
            EditorApplication.hierarchyChanged += FlagOutput;
        }

        private static void FlagOutput()
        {
            GameObject output = GameObject.Find(OutputModelName);
            if (output != null && output.hideFlags != HideFlags.HideAndDontSave)
                output.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
