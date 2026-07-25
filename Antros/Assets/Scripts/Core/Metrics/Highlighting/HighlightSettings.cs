using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATCG.Metrics
{
    /// <summary>
    /// Central highlight configuration, referenced by GameMetrics.
    /// - Layers: HighlightState -> URP rendering layer (shared by outline and fill).
    /// - Themes: the colours per state for each phase type (movement, cast, deploy…).
    /// </summary>
    [CreateAssetMenu(fileName = "HighlightSettings", menuName = "Antros/Highlighting/Highlight Settings")]
    public class HighlightSettings : ScriptableObject
    {
        [Serializable]
        public struct LayerBinding
        {
            public HighlightState state;
            public RenderingLayerMask layer;
        }

        [SerializeField, FormerlySerializedAs("outlineLayers"), FormerlySerializedAs("layers")]
        private LayerBinding[] layers = Array.Empty<LayerBinding>();

        [field: SerializeField]
        public HighlightTheme MovementTheme { get; private set; }

        [field: SerializeField]
        public HighlightTheme CastTheme { get; private set; }

        [field: SerializeField]
        public HighlightTheme DeployTheme { get; private set; }

        // Null-safe: an unfilled table just yields no layer (no highlight), never throws.
        public RenderingLayerMask GetLayer(HighlightState state)
        {
            if (layers != null)
                for (int i = 0; i < layers.Length; i++)
                    if (layers[i].state == state)
                        return layers[i].layer;

            return default;
        }

        // Seeds one binding per state on asset creation so you start from a full, editable list.
        private void Reset()
        {
            HighlightState[] states = (HighlightState[])Enum.GetValues(typeof(HighlightState));
            layers = new LayerBinding[states.Length - 1]; // skip None
            int index = 0;
            foreach (HighlightState state in states)
            {
                if (state == HighlightState.None)
                    continue;
                layers[index++] = new LayerBinding { state = state };
            }
        }
    }
}
