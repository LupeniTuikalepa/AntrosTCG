// PanelHeightClamp.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ATCG.UI.Layout
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class PanelHeightClamp : UIBehaviour, ILayoutController
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private float minHeight = 0f;
        [SerializeField] private float maxHeight = 400f;

        private RectTransform _rect;
        [System.NonSerialized] private DrivenRectTransformTracker _tracker;

        private RectTransform Rect => _rect ??= (RectTransform)transform;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(Rect);
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            LayoutRebuilder.MarkLayoutForRebuild(Rect);
            SetDirty();
        }

#endif
        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        protected override void OnTransformParentChanged()
        {
            SetDirty();
        }

        private void SetDirty()
        {
            if (!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild(Rect);
        }

        // ILayoutController — called automatically by CanvasUpdateRegistry
        // during the layout rebuild pass, same as ContentSizeFitter.
        void ILayoutController.SetLayoutHorizontal() { }

        void ILayoutController.SetLayoutVertical()
        {
            if (content == null) return;

            _tracker.Clear();
            _tracker.Add(this, Rect, DrivenTransformProperties.SizeDeltaY);

            float preferred = LayoutUtility.GetPreferredHeight(content);
            float clamped = Mathf.Clamp(preferred, minHeight, maxHeight);

            Vector2 size = Rect.sizeDelta;
            size.y = clamped;
            Rect.sizeDelta = size;
        }
    }
}