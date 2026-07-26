using System;
using System.Collections.Generic;
using Linework.SurfaceFill;
using Linework.WideOutline;
using UnityEngine;

namespace ATCG.Metrics
{
    /// <summary>
    /// Per-phase highlight look: one slot per Preview{N} state, each holding an embedded Linework
    /// WideOutline <see cref="Outline"/> and SurfaceFill <see cref="Fill"/> (always present, edited via
    /// their native inspectors in the custom editor). A per-side <c>active</c> flag decides whether that
    /// entry is editable and pushed onto the Linework settings by the controllers; the controllers also
    /// set each entry's RenderingLayer from the slot's state, so mapping is automatic.
    /// </summary>
    [CreateAssetMenu(fileName = "HighlightTheme", menuName = "Antros/Highlighting/Highlight Theme")]
    public class HighlightTheme : ScriptableObject
    {
        [Serializable]
        public class Slot
        {
            public HighlightState state;
            public bool outlineActive;
            public Outline outline;
            public bool fillActive;
            public Fill fill;
        }

        [SerializeField]
        private List<Slot> slots = new();

        public IEnumerable<(Outline outline, HighlightState state)> ActiveOutlines
        {
            get
            {
                foreach (Slot slot in slots)
                    if (slot != null && slot.outlineActive && slot.outline != null)
                        yield return (slot.outline, slot.state);
            }
        }

        public IEnumerable<(Fill fill, HighlightState state)> ActiveFills
        {
            get
            {
                foreach (Slot slot in slots)
                    if (slot != null && slot.fillActive && slot.fill != null)
                        yield return (slot.fill, slot.state);
            }
        }

        private static bool IsPreview(HighlightState state) => state.ToString().StartsWith("Preview");

#if UNITY_EDITOR
        public IReadOnlyList<Slot> EditorSlots => slots;

        // Guarantees one slot (with embedded Outline + Fill sub-assets) per Preview{N} state, ordered
        // by the enum. Called by the custom editor so every layer is always laid out.
        public void EditorEnsureSlots()
        {
            List<Slot> ordered = new List<Slot>();
            bool changed = false;

            foreach (HighlightState state in Enum.GetValues(typeof(HighlightState)))
            {
                if (!IsPreview(state))
                    continue;

                Slot slot = slots.Find(s => s != null && s.state == state);
                if (slot == null)
                {
                    slot = new Slot { state = state, outlineActive = false, fillActive = false };
                    changed = true;
                }

                // Regenerate any embedded sub-asset that went missing (e.g. lost on reimport).
                if (slot.outline == null)
                {
                    slot.outline = CreateOutline(state);
                    changed = true;
                }

                if (slot.fill == null)
                {
                    slot.fill = CreateFill(state);
                    changed = true;
                }

                ordered.Add(slot);
            }

            slots = ordered;
            changed |= SyncLayers();

            if (changed)
                SaveChange();
        }

        // Keeps each Outline/Fill's RenderingLayer in sync with the layer mapped for its state, so the
        // mapping is automatic (the runtime controllers also enforce it). Returns true if anything changed.
        private bool SyncLayers()
        {
            GameMetrics metrics = GameMetrics.Current;
            if (metrics == null)
                return false;

            bool changed = false;
            foreach (Slot slot in slots)
            {
                if (slot == null)
                    continue;

                RenderingLayerMask layer = metrics.GetHighlightLayer(slot.state);

                if (slot.outline != null && (uint)slot.outline.RenderingLayer != (uint)layer)
                {
                    slot.outline.RenderingLayer = layer;
                    UnityEditor.EditorUtility.SetDirty(slot.outline);
                    changed = true;
                }

                if (slot.fill != null && (uint)slot.fill.RenderingLayer != (uint)layer)
                {
                    slot.fill.RenderingLayer = layer;
                    UnityEditor.EditorUtility.SetDirty(slot.fill);
                    changed = true;
                }
            }

            return changed;
        }

        private Outline CreateOutline(HighlightState state)
        {
            Outline outline = CreateInstance<Outline>();
            outline.name = state + " Outline";
            UnityEditor.AssetDatabase.AddObjectToAsset(outline, this);
            return outline;
        }

        private Fill CreateFill(HighlightState state)
        {
            Fill fill = CreateInstance<Fill>();
            fill.name = state + " Fill";
            UnityEditor.AssetDatabase.AddObjectToAsset(fill, this);
            return fill;
        }

        private void SaveChange()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
        }
#endif
    }
}
