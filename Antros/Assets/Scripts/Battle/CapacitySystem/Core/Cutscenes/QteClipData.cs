using System;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Data a QTE clip carries. Deliberately thin: the duration IS the clip length
    /// (Timeline owns it), and the critical-window width is a global game metric,
    /// so this only holds presentation hints (which gauge to spawn, where).
    /// </summary>
    [Serializable]
    public struct QteClipData
    {
        [Tooltip("Optional gauge prefab override; falls back to a default if null.")]
        public GameObject gaugePrefab;

        [Tooltip("World/screen anchor hint for the gauge. Interpretation is up to the cutscene.")]
        public Vector3 anchorOffset;
    }
}
