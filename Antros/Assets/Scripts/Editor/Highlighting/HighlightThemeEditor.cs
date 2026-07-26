using System;
using System.Collections.Generic;
using ATCG.Metrics;
using Linework.SurfaceFill;
using Linework.WideOutline;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ATCG.Editor.Highlighting
{
    /// <summary>
    /// UIToolkit editor for HighlightTheme: an outer TabView with one tab per Preview{N} state; inside
    /// each, a darker panel holding a second TabView with an "Outline" and a "Fill" tab. The active
    /// toggle sits as a checkbox in each tab's header; the content is the embedded native Linework
    /// inspector (read-only when unchecked), plus Copy/Paste buttons. The Outline/Fill selection is kept
    /// in sync across preview tabs, and a state tab is dimmed when neither of its sides is active.
    /// </summary>
    [CustomEditor(typeof(HighlightTheme))]
    public class HighlightThemeEditor : UnityEditor.Editor
    {
        private const string TabHeaderClass = "unity-tab__header";
        private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.18f);

        // Parameter clipboards + shared side selection (persist across inspector rebuilds).
        private static Outline outlineClipboard;
        private static Fill fillClipboard;
        private static int selectedSide;

        private readonly List<TabView> sideTabViews = new();
        private bool syncingSides;

        public override VisualElement CreateInspectorGUI()
        {
            HighlightTheme theme = (HighlightTheme)target;
            theme.EditorEnsureSlots();
            serializedObject.Update();

            sideTabViews.Clear();
            TabView stateTabs = new TabView();

            SerializedProperty slotsProp = serializedObject.FindProperty("slots");
            for (int i = 0; i < slotsProp.arraySize && i < theme.EditorSlots.Count; i++)
            {
                HighlightTheme.Slot slot = theme.EditorSlots[i];
                SerializedProperty slotProp = slotsProp.GetArrayElementAtIndex(i);
                stateTabs.Add(BuildStateTab(slot, slotProp));
            }

            return stateTabs;
        }

        private Tab BuildStateTab(HighlightTheme.Slot slot, SerializedProperty slotProp)
        {
            Tab stateTab = new Tab(slot.state.ToString());

            SerializedProperty outlineActive = slotProp.FindPropertyRelative("outlineActive");
            SerializedProperty fillActive = slotProp.FindPropertyRelative("fillActive");

            bool outlineOn = outlineActive.boolValue;
            bool fillOn = fillActive.boolValue;

            VisualElement stateHeader = null;
            void UpdateDim()
            {
                if (stateHeader != null)
                    stateHeader.style.opacity = (outlineOn || fillOn) ? 1f : 0.4f;
            }

            VisualElement panel = Panel();
            TabView sideTabs = new TabView();
            sideTabs.Add(MakeSideTab("Outline", outlineActive, slot.outline, isOutline: true,
                v => { outlineOn = v; UpdateDim(); }));
            sideTabs.Add(MakeSideTab("Fill", fillActive, slot.fill, isOutline: false,
                v => { fillOn = v; UpdateDim(); }));

            RegisterSideTabView(sideTabs);
            panel.Add(sideTabs);
            stateTab.Add(panel);

            // Resolve the state tab's own header once attached, then apply the dim.
            stateTab.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                stateHeader = stateTab.Q(className: TabHeaderClass);
                UpdateDim();
            });

            return stateTab;
        }

        // Keeps every state's Outline/Fill sub-tabview on the same side, so switching preview keeps
        // Fill (or Outline) selected.
        private void RegisterSideTabView(TabView sideTabs)
        {
            sideTabViews.Add(sideTabs);
            sideTabs.selectedTabIndex = Mathf.Clamp(selectedSide, 0, 1);

            sideTabs.activeTabChanged += (_, __) =>
            {
                if (syncingSides)
                    return;

                selectedSide = sideTabs.selectedTabIndex;
                syncingSides = true;
                foreach (TabView other in sideTabViews)
                    other.selectedTabIndex = selectedSide;
                syncingSides = false;
            };
        }

        private static Tab MakeSideTab(string label, SerializedProperty activeProp, Object embedded, bool isOutline, Action<bool> onActiveChanged)
        {
            Tab tab = new Tab(label);
            tab.style.paddingTop = 6;

            VisualElement wrap = new VisualElement();

            void RebuildInspector()
            {
                wrap.Clear();
                if (embedded != null)
                    wrap.Add(new InspectorElement(embedded));
            }

            RebuildInspector();
            wrap.SetEnabled(activeProp.boolValue);

            tab.Add(CopyPasteRow(embedded, isOutline, RebuildInspector));
            tab.Add(wrap);

            // Active checkbox, placed in the tab header itself.
            Toggle check = new Toggle();
            check.style.marginRight = 4;
            check.style.marginLeft = 2;
            check.BindProperty(activeProp);
            check.RegisterValueChangedCallback(evt =>
            {
                wrap.SetEnabled(evt.newValue);
                onActiveChanged?.Invoke(evt.newValue);
            });
            check.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
            check.RegisterCallback<ClickEvent>(e => e.StopPropagation());

            void InsertCheck()
            {
                VisualElement header = tab.Q(className: TabHeaderClass);
                if (header != null && check.parent != header)
                    header.Insert(0, check);
            }

            InsertCheck();
            if (check.parent == null)
                tab.RegisterCallback<AttachToPanelEvent>(_ => InsertCheck());

            return tab;
        }

        private static VisualElement CopyPasteRow(Object target, bool isOutline, Action onPaste)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 4;

            Button copy = new Button(() =>
            {
                if (isOutline)
                    outlineClipboard = (Outline)target;
                else
                    fillClipboard = (Fill)target;
            })
            { text = "Copy" };

            Button paste = new Button(() =>
            {
                Object source = isOutline ? outlineClipboard : (Object)fillClipboard;
                if (source == null || source == target)
                    return;

                Undo.RecordObject(target, "Paste highlight params");

                // Preserve the target's identity + layer; copy only the look/parameters.
                string keepName = target.name;
                RenderingLayerMask keepLayer = isOutline ? ((Outline)target).RenderingLayer : ((Fill)target).RenderingLayer;

                EditorUtility.CopySerialized(source, target);

                target.name = keepName;
                if (isOutline)
                    ((Outline)target).RenderingLayer = keepLayer;
                else
                    ((Fill)target).RenderingLayer = keepLayer;

                EditorUtility.SetDirty(target);
                onPaste?.Invoke();
            })
            { text = "Paste" };

            copy.style.marginRight = 4;
            row.Add(copy);
            row.Add(paste);
            return row;
        }

        private static VisualElement Panel()
        {
            VisualElement panel = new VisualElement();
            panel.style.backgroundColor = PanelColor;
            panel.style.marginTop = 4;
            panel.style.paddingTop = panel.style.paddingBottom = 6;
            panel.style.paddingLeft = panel.style.paddingRight = 6;
            panel.style.borderTopLeftRadius = panel.style.borderTopRightRadius =
                panel.style.borderBottomLeftRadius = panel.style.borderBottomRightRadius = 4;
            return panel;
        }
    }
}
