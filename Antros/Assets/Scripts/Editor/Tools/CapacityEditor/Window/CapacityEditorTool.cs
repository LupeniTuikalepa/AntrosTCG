using System;
using System.Reflection;
using ATCG.Capacities;
using ATCG.Editor.Tools.CutsceneEditor;   // CutsceneAuthoring — opens the shared authoring stage
using ATCG.Editor.Tools.DatabaseBrowser;
using Unity.Scripting.LifecycleManagement;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Thin capacity asset browser. All authoring (stage, tracks, steps, tags, keys, properties) now
    /// lives in the shared Cutscenes tool (Explore / Edition); this tool only lists capacity assets,
    /// creates new ones (source-gen flow), and opens one for editing. It still exposes the "currently
    /// edited capacity" — sourced from the open stage — that StepMarkerEditor's step dropdown reads.
    /// </summary>
    [AutoStaticsCleanup]
    public sealed partial class CapacityEditorTool : IEditorTool
    {
        private const string ThemeUss = "EditorTheme.uss";
        private const string ToolUss = "CapacityEditor.uss";

        public string DisplayName => "Capacities";
        public string Icon => "⏱";
        public int Order => 50;

        // The capacity currently open in the shared cutscene stage. StepMarkerEditor's dropdown reads
        // this so a marker can only point at a declared step. Sourced from the stage so it survives
        // domain reloads; there's no tool-local selection any more.
        public static CapacityData CurrentlyEdited => CapacityCutsceneStage.Current?.Capacity;

        private DatabaseBrowserView<CapacityData> assetsView;

        public VisualElement BuildUI()
        {
            VisualElement root = new();
            root.AddToClassList("ce-root");
            root.style.flexGrow = 1;
            root.style.minHeight = 0;
            EditorStyleLoader.Load(root, ThemeUss);
            EditorStyleLoader.Load(root, ToolUss);

            Toolbar bar = new();
            bar.Add(new ToolbarButton(NewCapacityModal.Open) { text = "＋ New Capacity" });
            root.Add(bar);

            assetsView = new DatabaseBrowserView<CapacityData>(
                "Assets/Resources/Database/Capacities", "Capacities", OpenForEdit);

            VisualElement view = assetsView.Build();
            view.style.flexGrow = 1;
            view.style.minHeight = 0;
            root.Add(view);
            return root;
        }

        public void OnActivated() => assetsView?.Reload();

        public void OnDeactivated() { }

        // The browser's Edit action opens the capacity in the shared cutscene stage.
        private static void OpenForEdit(CapacityData capacity)
        {
            if (capacity != null)
                CutsceneAuthoring.Open(capacity);
        }

        // Reads the generated `static string[] DeclaredSteps` off the concrete type.
        public static string[] GetDeclaredSteps(CapacityData capacity)
        {
            if (capacity == null)
                return Array.Empty<string>();

            FieldInfo field = capacity.GetType().GetField(
                "DeclaredSteps",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return field?.GetValue(null) as string[] ?? Array.Empty<string>();
        }
    }
}
