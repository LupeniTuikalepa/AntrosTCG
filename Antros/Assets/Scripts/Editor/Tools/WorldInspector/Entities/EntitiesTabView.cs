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
        private ToolbarSearchField searchField;
        private Label statsHeader;
        private string searchText = "";
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

            // Text search over the entity name/id — created once so its value persists
            // across filter-bar rebuilds; placed inside the filter bar (it's a filter).
            searchField = new ToolbarSearchField();
            searchField.AddToClassList("wi-search");
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue ?? "";
                ApplyFilter();
            });

            BuildFilterBar();

            // Stats header: counts per aspect / player, refreshed each filter pass.
            statsHeader = new Label();
            statsHeader.AddToClassList("wi-stats");
            root.Add(statsHeader);

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

                // Cross-tool: tell the grid to highlight, and offer the timeline a focus.
                ATCG.Editor.EditorToolBus.Publish(new ATCG.Editor.EntitySelectedEvent(selectedEntityId));
                if (selectedEntityId >= 0)
                    ATCG.Editor.EditorToolBus.Publish(new ATCG.Editor.FocusEntityRequest(selectedEntityId));
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

            // Search sits first — it's a filter too.
            if (searchField != null)
                filterBar.Add(searchField);

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

        // id -> generation seen in the previous snapshot. Keyed with generation because
        // ids are recycled: a slot destroyed and re-created keeps its id but bumps its
        // generation, and must be treated as a NEW entity, not a known one.
        private readonly Dictionary<int, int> knownGenById = new();
        private readonly HashSet<int> newlyAppeared = new();
        private readonly HashSet<int> recentlyChanged = new();
        private readonly Dictionary<int, int> fingerprints = new();

        public void SetData(World world, WorldSnapshot snapshot)
        {
            this.world = world;
            this.snapshot = snapshot;

            // Track which ids are new, and which existing ones had a component value
            // change, since the previous snapshot — both get highlighted.
            newlyAppeared.Clear();
            recentlyChanged.Clear();

            if (snapshot != null && snapshot.HasData)
            {
                var nextFingerprints = new Dictionary<int, int>();

                foreach (int id in snapshot.EntityIds)
                {
                    int fp = ComputeFingerprint(id);
                    nextFingerprints[id] = fp;

                    int gen = snapshot.GenerationOf(id);
                    // New id, OR the same slot recycled into a different entity
                    // (generation bumped) -> treat as newly appeared, not "known".
                    if (!knownGenById.TryGetValue(id, out int prevGen) || prevGen != gen)
                        newlyAppeared.Add(id);
                    else if (fingerprints.TryGetValue(id, out int prev) && prev != fp)
                        recentlyChanged.Add(id);
                }

                knownGenById.Clear();
                foreach (int id in snapshot.EntityIds)
                    knownGenById[id] = snapshot.GenerationOf(id);

                fingerprints.Clear();
                foreach (KeyValuePair<int, int> kv in nextFingerprints)
                    fingerprints[kv.Key] = kv.Value;
            }

            ApplyFilter();
        }

        // A cheap hash over the entity's component values. Two snapshots with the same
        // fingerprint are treated as unchanged; a differing one means a value moved.
        private int ComputeFingerprint(int entityId)
        {
            if (world == null)
                return 0;

            Entity entity = new(entityId, world.GetGeneration(entityId));
            if (!world.IsAlive(entity))
                return 0;

            EntityMeta meta;
            try { meta = world.GetMeta(entity); }
            catch { return 0; }

            unchecked
            {
                int hash = 17;
                for (int cid = 0; cid < ComponentRegistry.MaxComponents; cid++)
                {
                    if (ComponentRegistry.GetTypeForComponentID(cid) == null || !meta.HasComponent(cid))
                        continue;

                    IComponentStore store;
                    try { store = world.GetStore(cid); }
                    catch { continue; }
                    if (store == null)
                        continue;

                    object boxed;
                    try { boxed = store.GetBoxed(entityId); }
                    catch { continue; }

                    hash = hash * 31 + cid;
                    hash = hash * 31 + (boxed != null ? boxed.GetHashCode() : 0);
                }
                return hash;
            }
        }

        private int builtComponentCount = -1;
        private int builtAspectCount = -1;

        public void RefreshCatalogs()
        {
            // The filter menus only need rebuilding when the discovered set changes;
            // doing it every snapshot (auto-refresh runs several times a second) was
            // needless work. Rebuild only on a count delta.
            if (componentCatalog.Entries.Count == builtComponentCount &&
                aspectCatalog.Entries.Count == builtAspectCount)
                return;

            builtComponentCount = componentCatalog.Entries.Count;
            builtAspectCount = aspectCatalog.Entries.Count;
            BuildFilterBar();
        }

        private readonly List<int> lastVisible = new();
        // Generation of each entry in lastVisible, parallel by index. A recycled slot
        // (same id, bumped generation) must count as a change even when the id-set is
        // identical, otherwise the list never rebuilds and freezes on the dead entity.
        private readonly List<int> lastVisibleGen = new();
        private int populatedEntityId = -2; // sentinel != any valid id and != -1

        private void ApplyFilter()
        {
            var newVisible = new List<int>();
            if (world != null && snapshot != null && snapshot.HasData)
            {
                bool hasSearch = !string.IsNullOrWhiteSpace(searchText);
                string needle = hasSearch ? searchText.Trim().ToLowerInvariant() : null;

                foreach (int id in snapshot.EntityIds)
                {
                    if (!filter.Passes(world, id))
                        continue;

                    if (hasSearch)
                    {
                        EntityLabel.Info info = EntityLabel.Build(world, id);
                        string hay = $"{info.Name} {id}".ToLowerInvariant();
                        if (!hay.Contains(needle))
                            continue;
                    }

                    newVisible.Add(id);
                }
            }

            // Sort for a stable display order — the world's entity span can reorder
            // (sparse-set swap-remove) without the actual set changing.
            newVisible.Sort();

            UpdateStats(newVisible);

            // Compare as a SET, not a sequence: a mere reordering must NOT count as a
            // change, otherwise every auto-refresh tick rebuilds the list, steals focus
            // (dropdowns won't open) and recreates the component pane (foldouts snap shut).
            // The comparison keys on (id, generation): comparing the bare id would miss a
            // recycled slot — a destroyed entity replaced by a new one on the same id —
            // and the list would freeze on the dead entity instead of showing the new one.
            bool changed = newVisible.Count != lastVisible.Count;
            if (!changed)
            {
                for (int i = 0; i < newVisible.Count; i++)
                {
                    if (newVisible[i] != lastVisible[i] ||
                        snapshot.GenerationOf(newVisible[i]) != lastVisibleGen[i])
                    { changed = true; break; }
                }
            }

            if (changed)
            {
                visibleIds.Clear();
                visibleIds.AddRange(newVisible);
                lastVisible.Clear();
                lastVisible.AddRange(newVisible);
                lastVisibleGen.Clear();
                foreach (int id in newVisible)
                    lastVisibleGen.Add(snapshot?.GenerationOf(id) ?? -1);
                entityList?.RefreshItems();

                int idx = visibleIds.IndexOf(selectedEntityId);
                if (idx >= 0)
                    entityList?.SetSelectionWithoutNotify(new[] { idx });
                else
                    selectedEntityId = -1;
            }

            // Repopulate the component pane when the selection changed, or when this
            // entity's own fingerprint changed since the last snapshot. Foldout state
            // survives rebuilds via the `expansion` dictionary, so this no longer causes
            // foldouts to snap shut — it just stops the pane from freezing on stale values.
            if (selectedEntityId != populatedEntityId || recentlyChanged.Contains(selectedEntityId))
                RefreshComponents();
        }

        private VisualElement MakeRow()
        {
            VisualElement row = new();
            row.AddToClassList("wi-entity-row");
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            VisualElement dot = new();
            dot.AddToClassList("wi-pastille");
            dot.name = "pastille";
            row.Add(dot);

            Label label = new();
            label.name = "label";
            label.style.flexGrow = 1;
            row.Add(label);

            return row;
        }

        private void BindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= visibleIds.Count)
                return;
            int id = visibleIds[index];

            EntityLabel.Info info = EntityLabel.Build(world, id);

            VisualElement dot = element.Q<VisualElement>("pastille");
            if (dot != null)
                dot.style.backgroundColor = info.Pastille;

            Label label = element.Q<Label>("label");
            if (label != null)
            {
                label.text = info.Active ? info.Name : $"{info.Name}  (inactive)";
                label.EnableInClassList("wi-entity-row--inactive", !info.Active);
            }

            // Highlight entities that appeared or changed since the previous snapshot.
            element.EnableInClassList("wi-entity-row--new", newlyAppeared.Contains(id));
            element.EnableInClassList("wi-entity-row--changed",
                !newlyAppeared.Contains(id) && recentlyChanged.Contains(id));
        }

        private void UpdateStats(List<int> ids)
        {
            if (statsHeader == null)
                return;

            if (world == null || ids.Count == 0)
            {
                statsHeader.text = $"{ids?.Count ?? 0} entities";
                return;
            }

            // Count how many visible entities match each discovered aspect.
            var perAspect = new List<string>();
            foreach (AspectCatalog.Entry e in aspectCatalog.Entries)
            {
                int n = 0;
                foreach (int id in ids)
                {
                    EntityAddress address = new(world, new Entity(id, world.GetGeneration(id)));
                    if (e.Matches(in address))
                        n++;
                }
                if (n > 0)
                    perAspect.Add($"{e.Name}: {n}");
            }

            statsHeader.text = perAspect.Count > 0
                ? $"{ids.Count} entities  \u00b7  {string.Join("  \u00b7  ", perAspect)}"
                : $"{ids.Count} entities";
        }

        private void RefreshComponents()
        {
            if (componentPane == null)
                return;
            componentView.Populate(componentPane, world, selectedEntityId);
            populatedEntityId = selectedEntityId;
        }
    }
}