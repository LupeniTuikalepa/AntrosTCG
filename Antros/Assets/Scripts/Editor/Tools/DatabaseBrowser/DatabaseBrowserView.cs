using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATCG.Elements;
using ATCG.Enums;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.DatabaseBrowser
{
    /// <summary>
    /// Reusable browser view over a family of GameDatabaseObject assets: a searchable,
    /// element-filterable, sortable MultiColumnListView with an inspector pane (the same UX
    /// as the Cards tool, minus the deck toggle). Name and Element columns are filled by
    /// reflection, so a type without those fields (e.g. DeployableData) still lists cleanly —
    /// it falls back to the asset file name and hides the Element column + filter.
    ///
    /// This is a plain view (not an IEditorTool), so it can be hosted anywhere: the standalone
    /// Status/Passives/Deployables tools wrap it, and the Capacities window embeds it in its
    /// "Assets" tab. Pass an <paramref name="editAction"/> to add an "Edit" button column.
    /// </summary>
    public sealed class DatabaseBrowserView<T> where T : ScriptableObject
    {
        private static readonly PropertyInfo NameProperty = ResolveNameProperty();
        private static readonly PropertyInfo ElementProperty = ResolveElementProperty();
        private static bool HasElement => ElementProperty != null;

        private readonly string folderPath;
        private readonly string emptyHint;
        private readonly Action<T> editAction;

        private readonly List<T> all = new();
        private readonly List<T> filtered = new();

        private int elementFilter;  // 0 = All, else (Element)(index - 1)
        private string search = string.Empty;

        private MultiColumnListView list;
        private Label countLabel;
        private VisualElement inspectorContainer;

        /// <param name="folderPath">Resources folder scanned (recursively) for assets.</param>
        /// <param name="emptyHint">Short label shown in the empty inspector, e.g. "Status".</param>
        /// <param name="editAction">Optional per-row action; when set an "Edit" column appears.</param>
        public DatabaseBrowserView(string folderPath, string emptyHint, Action<T> editAction = null)
        {
            this.folderPath = folderPath;
            this.emptyHint = emptyHint;
            this.editAction = editAction;
        }

        public VisualElement Build()
        {
            Load();

            VisualElement root = new VisualElement { style = { flexGrow = 1, minHeight = 0 } };
            EditorStyleLoader.Load(root, "EditorTheme.uss");
            root.Add(BuildToolbar());
            root.Add(BuildCount());

            TwoPaneSplitView split = new TwoPaneSplitView(1, 320, TwoPaneSplitViewOrientation.Horizontal)
            {
                style = { flexGrow = 1, minHeight = 0 }
            };
            split.Add(BuildList());
            split.Add(BuildInspector());
            root.Add(split);

            Refresh();
            return root;
        }

        /// <summary>Re-scans the folder and refreshes the list (call when the tab becomes visible).</summary>
        public void Reload()
        {
            Load();
            Refresh();
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new Toolbar();

            if (HasElement)
            {
                PopupField<string> elementPopup = new PopupField<string>("Element", ElementOptions(), 0)
                {
                    style = { marginRight = 6 }
                };
                elementPopup.RegisterValueChangedCallback(_ => { elementFilter = elementPopup.index; Refresh(); });
                toolbar.Add(elementPopup);
            }

            ToolbarSearchField searchField = new ToolbarSearchField { style = { flexGrow = 1 } };
            searchField.RegisterValueChangedCallback(e => { search = e.newValue; Refresh(); });
            toolbar.Add(searchField);

            toolbar.Add(new ToolbarButton(() => { Load(); Refresh(); }) { text = "Refresh" });
            return toolbar;
        }

        private VisualElement BuildCount()
        {
            countLabel = new Label
            {
                style = { marginTop = 2, marginBottom = 2, marginLeft = 4, marginRight = 4, opacity = 0.7f }
            };
            return countLabel;
        }

        private MultiColumnListView BuildList()
        {
            list = new MultiColumnListView
            {
                itemsSource = filtered,
                selectionType = SelectionType.Single,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                sortingMode = ColumnSortingMode.Custom,
                style = { flexGrow = 1, minHeight = 0 },
            };
            list.AddToClassList("atcg-list");

            if (editAction != null)
            {
                list.columns.Add(new Column
                {
                    name = "edit", title = string.Empty, width = 28, sortable = false, optional = false,
                    makeCell = MakeEditCell,
                    bindCell = (e, i) =>
                    {
                        T asset = filtered[i];
                        ((Button)e).clickable = new Clickable(() => editAction(asset));
                    },
                });
            }

            list.columns.Add(new Column
            {
                name = "name", title = "Name", stretchable = true, minWidth = 140, optional = false,
                makeCell = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 4 } },
                bindCell = (e, i) => ((Label)e).text = DisplayNameOf(filtered[i]),
            });

            if (HasElement)
            {
                list.columns.Add(new Column
                {
                    name = "element", title = "Element", width = 120, optional = true,
                    makeCell = MakeElementCell, bindCell = BindElementCell,
                });
            }

            list.columns.Add(new Column
            {
                name = "type", title = "Type", width = 150, optional = true,
                makeCell = () => new Label { style = { opacity = 0.7f } },
                bindCell = (e, i) => ((Label)e).text = filtered[i].GetType().Name,
            });

            list.columns.Add(new Column
            {
                name = "ping", title = string.Empty, width = 30, sortable = false, optional = true,
                makeCell = () => new Button { text = "→" },
                bindCell = (e, i) =>
                {
                    T asset = filtered[i];
                    ((Button)e).clickable = new Clickable(() => EditorGUIUtility.PingObject(asset));
                },
            });

            list.columnSortingChanged += () => { SortFiltered(); list.RefreshItems(); };
            list.selectedIndicesChanged += _ => UpdateInspector();
            return list;
        }

        // Edit action cell: Unity's built-in edit (pencil) icon rather than a text button, in a
        // narrow column. Falls back to a glyph if the built-in icon can't be resolved.
        private static Button MakeEditCell()
        {
            Button button = new Button { tooltip = "Edit" };
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.paddingTop = 0;
            button.style.paddingBottom = 0;

            Texture icon = EditorGUIUtility.IconContent("editicon.sml")?.image;
            if (icon != null)
            {
                button.style.backgroundImage = new StyleBackground((Texture2D)icon);
                button.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                button.text = "✎";
            }

            return button;
        }

        // Editable Element cell: an EnumField that writes back to the asset's serialized
        // backing field (the property setter is private), with Undo + dirtying.
        private static VisualElement MakeElementCell()
        {
            EnumField field = new EnumField(default(Element)) { style = { marginTop = 1, marginBottom = 1 } };
            field.RegisterValueChangedCallback(evt =>
            {
                if (field.userData is T asset && evt.newValue is Element element)
                    SetElement(asset, element);
            });
            return field;
        }

        private void BindElementCell(VisualElement cell, int index)
        {
            EnumField field = (EnumField)cell;
            field.userData = filtered[index];
            field.SetValueWithoutNotify(ElementOf(filtered[index]) ?? default);
        }

        private static void SetElement(T asset, Element value)
        {
            SerializedObject so = new SerializedObject(asset);
            SerializedProperty prop = so.FindProperty("<Element>k__BackingField");
            if (prop == null)
                return;

            int idx = Array.IndexOf(prop.enumNames, value.ToString());
            if (idx < 0)
                return;

            Undo.RecordObject(asset, "Change element");
            prop.enumValueIndex = idx;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private VisualElement BuildInspector()
        {
            inspectorContainer = new ScrollView { style = { flexGrow = 1, minWidth = 200 } };
            ShowInspectorEmpty();
            return inspectorContainer;
        }

        private void UpdateInspector()
        {
            if (inspectorContainer == null)
                return;

            inspectorContainer.Clear();

            int index = list.selectedIndex;
            if (index < 0 || index >= filtered.Count)
            {
                ShowInspectorEmpty();
                return;
            }

            inspectorContainer.Add(new InspectorElement(filtered[index]));
        }

        private void ShowInspectorEmpty()
        {
            if (inspectorContainer == null)
                return;

            inspectorContainer.Clear();
            inspectorContainer.Add(new Label($"Sélectionnez un élément ({emptyHint})")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal,
                    opacity = 0.6f,
                    marginTop = 24,
                    paddingLeft = 12,
                    paddingRight = 12,
                },
            });
        }

        private void Load()
        {
            all.Clear();

            if (!AssetDatabase.IsValidFolder(folderPath))
                return;

            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath }))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                    all.Add(asset);
            }

            all.Sort((a, b) => string.Compare(DisplayNameOf(a), DisplayNameOf(b), StringComparison.OrdinalIgnoreCase));
        }

        private void Refresh()
        {
            filtered.Clear();
            filtered.AddRange(all.Where(Passes));
            SortFiltered();
            list?.RefreshItems();
            list?.ClearSelection();
            ShowInspectorEmpty();
            UpdateCount();
        }

        private bool Passes(T asset)
        {
            if (asset == null)
                return false;

            if (HasElement && elementFilter > 0)
            {
                Element? e = ElementOf(asset);
                if (e == null || (int)e.Value != elementFilter - 1)
                    return false;
            }

            if (!string.IsNullOrEmpty(search) &&
                DisplayNameOf(asset).IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void SortFiltered()
        {
            if (list == null)
                return;

            List<SortColumnDescription> sorts = list.sortedColumns.ToList();
            if (sorts.Count == 0)
                return;

            filtered.Sort((a, b) =>
            {
                foreach (SortColumnDescription sort in sorts)
                {
                    int c = Compare(sort.columnName, a, b);
                    if (sort.direction == SortDirection.Descending)
                        c = -c;
                    if (c != 0)
                        return c;
                }
                return 0;
            });
        }

        private int Compare(string column, T a, T b) => column switch
        {
            "element" => ((int)(ElementOf(a) ?? 0)).CompareTo((int)(ElementOf(b) ?? 0)),
            "type" => string.Compare(a.GetType().Name, b.GetType().Name, StringComparison.Ordinal),
            _ => string.Compare(DisplayNameOf(a), DisplayNameOf(b), StringComparison.OrdinalIgnoreCase),
        };

        private void UpdateCount()
        {
            if (countLabel == null)
                return;

            countLabel.text = $"{filtered.Count} {(filtered.Count == 1 ? "asset" : "assets")}";
        }

        private static string DisplayNameOf(T asset)
        {
            if (NameProperty != null)
            {
                string n = NameProperty.GetValue(asset) as string;
                if (!string.IsNullOrEmpty(n))
                    return n;
            }
            return asset.name;
        }

        private static Element? ElementOf(T asset)
        {
            if (ElementProperty == null)
                return null;
            return ElementProperty.GetValue(asset) is Element e ? e : (Element?)null;
        }

        private static PropertyInfo ResolveNameProperty()
        {
            PropertyInfo p = typeof(T).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            return p != null && p.PropertyType == typeof(string) ? p : null;
        }

        private static PropertyInfo ResolveElementProperty()
        {
            PropertyInfo p = typeof(T).GetProperty("Element", BindingFlags.Public | BindingFlags.Instance);
            return p != null && p.PropertyType == typeof(Element) ? p : null;
        }

        private static List<string> ElementOptions()
            => new List<string> { "All" }.Concat(Enum.GetNames(typeof(Element))).ToList();
    }
}
