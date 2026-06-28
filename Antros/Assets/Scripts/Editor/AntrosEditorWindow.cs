using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor
{
    /// <summary>
    /// The Antros TCG Editor hub: a single window that hosts every editor tool. A left
    /// rail lists the tools (discovered by reflection — any IEditorTool with a public
    /// parameterless constructor shows up), and the main area swaps to the selected
    /// tool's UI. New tools need no hub changes: implement IEditorTool and it appears.
    /// </summary>
    public sealed class AntrosEditorWindow : EditorWindow
    {
        private const string ThemeUss = "AntrosEditor.uss";

        private readonly List<IEditorTool> tools = new();
        private readonly Dictionary<IEditorTool, VisualElement> builtUI = new();
        private readonly Dictionary<IEditorTool, Button> railButtons = new();

        private IEditorTool active;
        private VisualElement railContainer;
        private VisualElement contentContainer;

        [MenuItem("ATCG/Antros TCG Editor")]
        public static void Open()
        {
            AntrosEditorWindow wnd = GetWindow<AntrosEditorWindow>();
            wnd.titleContent = new GUIContent("Antros TCG Editor");
            wnd.minSize = new Vector2(720, 420);
            wnd.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            EditorStyles.Load(root, "EditorTheme.uss");
            EditorStyles.Load(root, ThemeUss);

            // Left rail.
            railContainer = new VisualElement();
            railContainer.AddToClassList("ae-rail");
            root.Add(railContainer);

            Label brand = new("Antros");
            brand.AddToClassList("ae-rail__brand");
            railContainer.Add(brand);

            // Main content area.
            contentContainer = new VisualElement();
            contentContainer.AddToClassList("ae-content");
            contentContainer.style.flexGrow = 1;
            root.Add(contentContainer);

            DiscoverTools();
            BuildRail();

            if (tools.Count > 0)
                Activate(tools[0]);
            else
                ShowEmpty();
        }

        private void OnDisable()
        {
            active?.OnDeactivated();
            active = null;

            // Tools that hold long-lived subscriptions (e.g. the timeline listening to
            // CommandTrace for its whole lifetime) get a chance to release them here.
            foreach (IEditorTool tool in tools)
            {
                if (tool is System.IDisposable disposable)
                    disposable.Dispose();
            }
        }

        private void DiscoverTools()
        {
            tools.Clear();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                // Some assemblies throw on GetTypes; skipping them is expected.
                try { types = asm.GetTypes(); }
                catch { continue; }

                foreach (Type t in types)
                {
                    if (t.IsAbstract || t.IsInterface)
                        continue;
                    if (!typeof(IEditorTool).IsAssignableFrom(t))
                        continue;
                    if (t.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    try { tools.Add((IEditorTool)Activator.CreateInstance(t)); }
                    catch (Exception e) { Debug.LogWarning($"[Antros Editor] Couldn't create tool {t.Name}: {e.Message}"); }
                }
            }

            tools.Sort((a, b) =>
            {
                int c = a.Order.CompareTo(b.Order);
                return c != 0 ? c : string.CompareOrdinal(a.DisplayName, b.DisplayName);
            });
        }

        // Categorical icon colors (Cardness-style), cycled per tool.
        private static readonly Color[] IconTints =
        {
            new(0.36f, 0.66f, 1.00f), // blue
            new(0.46f, 0.78f, 0.51f), // green
            new(0.71f, 0.56f, 0.94f), // purple
            new(0.91f, 0.51f, 0.77f), // pink
            new(0.95f, 0.70f, 0.30f), // amber
            new(0.38f, 0.81f, 0.81f), // teal
        };

        private void BuildRail()
        {
            railButtons.Clear();

            for (int i = 0; i < tools.Count; i++)
            {
                IEditorTool tool = tools[i];
                IEditorTool captured = tool;
                Button b = new(() => Activate(captured));
                b.AddToClassList("ae-rail__btn");

                Label icon = new(string.IsNullOrEmpty(tool.Icon) ? "\u25A0" : tool.Icon);
                icon.AddToClassList("ae-rail__icon");
                Color tint = IconTints[i % IconTints.Length];
                icon.style.backgroundColor = new Color(tint.r, tint.g, tint.b, 0.18f);
                icon.style.color = tint;
                b.Add(icon);

                Label name = new(tool.DisplayName);
                name.AddToClassList("ae-rail__label");
                b.Add(name);

                railContainer.Add(b);
                railButtons[tool] = b;
            }
        }

        private void Activate(IEditorTool tool)
        {
            if (active == tool)
                return;

            active?.OnDeactivated();

            active = tool;
            contentContainer.Clear();

            // Header strip with the active tool's icon + name.
            VisualElement header = new();
            header.AddToClassList("ae-header");
            Label hIcon = new(string.IsNullOrEmpty(tool.Icon) ? "\u25A0" : tool.Icon);
            hIcon.AddToClassList("ae-header__icon");
            header.Add(hIcon);
            Label hName = new(tool.DisplayName);
            hName.AddToClassList("ae-header__title");
            header.Add(hName);
            contentContainer.Add(header);

            if (!builtUI.TryGetValue(tool, out VisualElement ui))
            {
                ui = tool.BuildUI();
                builtUI[tool] = ui;
            }
            ui.style.flexGrow = 1;
            contentContainer.Add(ui);

            foreach (KeyValuePair<IEditorTool, Button> kv in railButtons)
                kv.Value.EnableInClassList("ae-rail__btn--active", kv.Key == tool);

            tool.OnActivated();
        }

        private void ShowEmpty()
        {
            Label hint = new("No tools found. Implement IEditorTool to add one.");
            hint.AddToClassList("ae-empty");
            contentContainer.Add(hint);
        }
    }
}
