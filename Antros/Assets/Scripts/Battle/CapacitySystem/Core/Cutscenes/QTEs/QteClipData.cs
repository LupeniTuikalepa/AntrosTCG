using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs
{
    /// <summary>
    /// Data carried by a QTE clip. Deliberately thin: the duration IS the clip
    /// length (Timeline owns it), and the critical-window width is a global game
    /// metric, so this only holds presentation hints (which gauge to spawn, where).
    /// </summary>
    [Serializable]
    public struct QteClipData
    {
        [field: Tooltip("World/screen anchor hint for the gauge. Interpretation is up to the cutscene.")]
        [field: SerializeField]
        public Vector2 ScreenOffset { get; private set; }

        [field: SerializeField]
        public bool OverrideAnchor { get; private set; }

        [field: ShowIf(nameof(OverrideAnchor))]
        [field: SerializeField]
        public HumanBodyBones BoneAnchor { get; private set; }
    }
}