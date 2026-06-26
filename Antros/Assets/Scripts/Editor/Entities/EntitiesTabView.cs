using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// The "Entities" tab: a filter bar (by components and by aspect), a filtered
    /// entity list, and a component detail pane. Reads from a WorldSnapshot so the
    /// list is stable between explicit refreshes.
    /// </summary>
    public sealed class EntitiesTabView
    {
        private readonly EntityFilter filter = new();
        private readonly ComponentCatalog componentCatalog;
        private readonly AspectCatalog aspectCatalog;
        private readonly Dictionary<string, bool> componentExpansion = new();
        private readonly EntityComponentView componentView;

        private readonly List<int> visibleIds = new();

        private World world;
        private WorldSnapshot snapshot;

        private VisualElement root;
        private VisualElement filterBar;
        private VisualElement chipRow;
        private ListView entityList;
        private ScrollView componentPane;
        private int selectedEntityId = -1;

        public EntitiesTabView(ComponentCatalog componentCatalog, AspectCatalog aspectCatalog)
        {
            this.componentCatalog = componentCatalog;
            this.aspectCatalog = aspectCatalog;
            componentView = new EntityComponentView(componentExpansion);
        }

        public VisualElement Root => root;

        public VisualElement Build()
        {
            root = new VisualElement();
            root.AddToClassList("wi-tab");
            root.style.flexGrow = 1;

            filterBar = new VisualElement();
            filterBar.AddToClassList("wi-filterbar");
            root.Add(filterBar);
            BuildFilterBar();

            TwoPaneSplitView split = new(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("wi-split");
            root.Add(split);

            entityList = new ListView(visibleIds, 18, MakeRow, BindRow)
            {
                selectionType = SelectionType.Single
            };
            entityList.AddToClassList("wi-entity-list");
            entityList.selectionChanged += _ =>
            {
                selectedEntityId = entityList.selectedIndex >= 0 && entityList.selectedIndex < visibleIds.Count
                    ? visibleIds[entityList.selectedIndex]
                    : -1;
                RefreshComponents();
            };
            split.Add(entityList);

            componentPane = new ScrollView();
            componentPane.AddToClassList("wi-component-pane");
            split.Add(componentPane);

            return root;
        }

        private void BuildFilterBar()
        {
            filterBar.Clear();

            // Component multi-select via a dropdown of toggles.
            ToolbarMenu compMenu = new() { text = "Components" };
            compMenu.AddToClassList("wi-filter-menu");
            foreach (ComponentCatalog.Entry e in componentCatalog.Entries)
            {
                int id = e.Id;
                compMenu.menu.AppendAction(
                    e.Name,
                    _ => { filter.ToggleComponent(id); ApplyFilter(); RebuildFilterChips(); },
                    _ => filter.IsComponentSelected(id)
                        ? DropdownMenuAction.Status.Checked
                        : DropdownMenuAction.Status.Normal);
            }
            filterBar.Add(compMenu);

            // Aspect single-select.
            ToolbarMenu aspectMenu = new() { text = "Aspect" };
            aspectMenu.AddToClassList("wi-filter-menu");
            aspectMenu.menu.AppendAction("(none)", _ => { filter.ClearAspect(); ApplyFilter(); RebuildFilterChips(); },
                _ => filter.HasAspect ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Checked);
            foreach (AspectCatalog.Entry e in aspectCatalog.Entries)
            {
                AspectCatalog.Entry entry = e;
                aspectMenu.menu.AppendAction(e.Name,
                    _ => { filter.SetAspect(entry); ApplyFilter(); RebuildFilterChips(); },
                    _ => filter.Aspect == entry
                        ? DropdownMenuAction.Status.Checked
                        : DropdownMenuAction.Status.Normal);
            }
            filterBar.Add(aspectMenu);

            Button clear = new(() => { filter.ClearAll(); ApplyFilter(); RebuildFilterChips(); BuildFilterBar(); }) { text = "Reset" };
            clear.AddToClassList("wi-filter-reset");
            filterBar.Add(clear);

            chipRow = new VisualElement();
            chipRow.AddToClassList("wi-chip-row");
            filterBar.Add(chipRow);
            RebuildFilterChips();
        }

        private void RebuildFilterChips()
        {
            if (chipRow == null)
                return;
            chipRow.Clear();

            foreach (int id in new List<int>(filter.RequiredComponentIds))
            {
                if (!componentCatalog.TryGetType(id, out var t))
                    continue;
                int captured = id;
                Label chip = MakeChip($"{t.Name}  \u00d7", () =>
                {
                    filter.RemoveComponent(captured);
                    ApplyFilter();
                    RebuildFilterChips();
                });
                chipRow.Add(chip);
            }

            if (filter.HasAspect)
            {
                Label chip = MakeChip($"{filter.Aspect.Name}  \u00d7", () =>
                {
                    filter.ClearAspect();
                    ApplyFilter();
                    RebuildFilterChips();
                });
                chip.AddToClassList("wi-chip--aspect");
                chipRow.Add(chip);
            }
        }

        private static Label MakeChip(string text, System.Action onClick)
        {
            Label chip = new(text);
            chip.AddToClassList("wi-chip");
            chip.RegisterCallback<MouseDownEvent>(_ => onClick());
            return chip;
        }

        public void SetData(World world, WorldSnapshot snapshot)
        {
            this.world = world;
            this.snapshot = snapshot;
            ApplyFilter();
        }

        public void RefreshCatalogs()
        {
            BuildFilterBar();
        }

        private void ApplyFilter()
        {
            visibleIds.Clear();

            if (world != null && snapshot != null && snapshot.HasData)
            {
                foreach (int id in snapshot.EntityIds)
                {
                    if (filter.Passes(world, id))
                        visibleIds.Add(id);
                }
            }

            entityList?.RefreshItems();

            int idx = visibleIds.IndexOf(selectedEntityId);
            if (idx >= 0)
                entityList?.SetSelectionWithoutNotify(new[] { idx });
            else
            {
                selectedEntityId = -1;
                RefreshComponents();
            }
        }

        private VisualElement MakeRow()
        {
            Label label = new();
            label.AddToClassList("wi-entity-row");
            return label;
        }

        private void BindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= visibleIds.Count)
                return;
            int id = visibleIds[index];
            Label label = (Label)element;

            bool active = false;
            try { active = world.IsActive(new Entity(id)); } catch { /* teardown race */ }

            label.text = active ? $"Entity {id}" : $"Entity {id}  (inactive)";
            label.EnableInClassList("wi-entity-row--inactive", !active);
        }

        private void RefreshComponents()
        {
            if (componentPane == null)
                return;
            componentView.Populate(componentPane, world, selectedEntityId);
        }
    }
}
