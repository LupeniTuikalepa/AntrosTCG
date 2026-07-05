using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Tags the cutscene instance currently loaded in the editing scene with the
    /// capacity it represents, so CapacityStageInstantiator can find and replace
    /// it when the selection changes.
    /// </summary>
    public sealed class CapacityStageAnchor : MonoBehaviour
    {
        public string CapacityGuid;
    }
}
