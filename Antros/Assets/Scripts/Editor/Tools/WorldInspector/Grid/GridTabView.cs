using System;
using System.Collections.Generic;
using System.Reflection;
using ATCG.Battle.Entities.Components;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// The "Grid" tab: a hex grid reconstructed from entities that carry a
    /// GridMemberComponent (each holds its HexCoordinates). Cells are derived from the
    /// entities themselves, so no BattleGrid API is needed. Clicking a cell lists the
    /// entities standing on it.
    ///
    /// Hexes are pointy-top, laid out from axial cube coords (x, y). The view fits the
    /// occupied bounds and redraws on geometry change.
    /// </summary>
    public sealed class GridTabView
    {
        private readonly AspectCatalog aspectCatalog;

        private World world;
        private WorldSnapshot snapshot;

        private VisualElement root;
        private VisualElement aspectBar;
        private HexGridElement gridElement;
        private ScrollView cellPane;
        private Label cellTitle;

        private readonly Dictionary<HexCoordReader.Axial, List<int>> cellToEntities = new();
        private readonly HashSet<HexCoordReader.Axial> markedCells = new();
        private readonly HashSet<string> checkedAspects = new();
        private readonly EntityComponentView componentView;
        private readonly Dictionary<string, bool> componentExpansion = new();

        private HexCoordReader.Axial? selectedCell;
        private bool aspectDefaultsApplied;

        // Cached reflection: GridMemberComponent.coordinates field.
        private static FieldInfo coordinatesField;
        private int gridMemberComponentId = -1;

        public GridTabView(AspectCatalog aspectCatalog)
        {
            this.aspectCatalog = aspectCatalog;
            componentView = new EntityComponentView(componentExpansion);
        }

        public VisualElement Root => root;

        public VisualElement Build()
        {
            root = new VisualElement();
            root.AddToClassList("wi-tab");
            root.style.flexGrow = 1;

            aspectBar = new VisualElement();
            aspectBar.AddToClassList("wi-aspectbar");
            root.Add(aspectBar);
            BuildAspectBar();

            TwoPaneSplitView split = new(1, 280, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("wi-split");
            root.Add(split);

            gridElement = new HexGridElement(OnCellClicked);
            gridElement.AddToClassList("wi-grid-canvas");
            gridElement.style.flexGrow = 1;
            split.Add(gridElement);

            VisualElement side = new();
            side.AddToClassList("wi-grid-side");
            split.Add(side);

            cellTitle = new Label("No cell selected");
            cellTitle.AddToClassList("wi-grid-side__title");
            side.Add(cellTitle);

            cellPane = new ScrollView();
            cellPane.AddToClassList("wi-component-pane");
            side.Add(cellPane);

            return root;
        }

        private void BuildAspectBar()
        {
            aspectBar.Clear();

            Label caption = new("Mark aspects:");
            caption.AddToClassList("wi-aspectbar__caption");
            aspectBar.Add(caption);

            // Default: HeroEntityAspect checked, applied once.
            if (!aspectDefaultsApplied)
            {
                foreach (AspectCatalog.Entry e in aspectCatalog.Entries)
                {
                    if (e.Name.Contains("Hero"))
                        checkedAspects.Add(e.Name);
                }
                aspectDefaultsApplied = aspectCatalog.Entries.Count > 0;
            }

            foreach (AspectCatalog.Entry e in aspectCatalog.Entries)
            {
                string aspectName = e.Name;
                ToolbarToggle t = new() { text = aspectName, value = checkedAspects.Contains(aspectName) };
                t.AddToClassList("wi-aspect-toggle");
                t.AddToClassList("atcg-toggle-accent");
                t.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue) checkedAspects.Add(aspectName);
                    else checkedAspects.Remove(aspectName);
                    RecomputeMarks();
                    gridElement?.SetMarkedCells(markedCells);
                });
                aspectBar.Add(t);
            }
        }

        public void SetData(World world, WorldSnapshot snapshot)
        {
            this.world = world;
            this.snapshot = snapshot;

            // Build the aspect toggles once aspects are discovered.
            if (aspectBar != null && aspectBar.childCount <= 1 && aspectCatalog.Entries.Count > 0)
                BuildAspectBar();

            RebuildCells();
            RecomputeMarks();
            gridElement?.SetCells(cellToEntities, selectedCell);
            gridElement?.SetMarkedCells(markedCells);
            gridElement?.SetStartingEdges(StartingLines.ComputeEdges());

            // Only rebuild the side pane when the selected cell changes — a plain
            // auto-refresh tick must not recreate the foldouts (they'd snap shut).
            RefreshCellPane(force: false);
        }

        /// <summary>
        /// A cell is "marked" if any entity on it matches any checked aspect. Cheap to
        /// recompute and only touches snapshot entities already grouped into cells.
        /// </summary>
        private void RecomputeMarks()
        {
            markedCells.Clear();
            if (world == null || checkedAspects.Count == 0)
                return;

            foreach (KeyValuePair<HexCoordReader.Axial, List<int>> kv in cellToEntities)
            {
                foreach (int entityId in kv.Value)
                {
                    if (!MatchesAnyCheckedAspect(entityId))
                        continue;
                    markedCells.Add(kv.Key);
                    break;
                }
            }
        }

        private bool MatchesAnyCheckedAspect(int entityId)
        {
            EntityAddress address = new(world, new Entity(entityId));
            foreach (AspectCatalog.Entry e in aspectCatalog.Entries)
            {
                if (!checkedAspects.Contains(e.Name))
                    continue;
                if (e.Matches(in address))
                    return true;
            }
            return false;
        }

        private void RebuildCells()
        {
            cellToEntities.Clear();

            if (world == null || snapshot == null || !snapshot.HasData)
                return;

            if (gridMemberComponentId < 0)
                gridMemberComponentId = FindComponentId(typeof(GridMemberComponent));
            if (gridMemberComponentId < 0)
                return;

            IComponentStore store;
            try { store = world.GetStore(gridMemberComponentId); }
            catch (System.Exception e)
            {
                InspectorLog.Warn($"GetStore({gridMemberComponentId}) for GridMemberComponent threw", e);
                return;
            }
            if (store == null)
            {
                InspectorLog.Warn($"No store for GridMemberComponent (id {gridMemberComponentId}) — possible id mapping drift.");
                return;
            }

            foreach (int entityId in snapshot.EntityIds)
            {
                object boxed;
                try { boxed = store.GetBoxed(entityId); }
                catch (System.Exception e) { InspectorLog.Warn($"GetBoxed({entityId}) on GridMemberComponent threw", e); continue; }
                if (boxed == null)
                    continue;

                if (!TryReadCoordinates(boxed, out object hexBoxed))
                    continue;
                if (!HexCoordReader.TryRead(hexBoxed, out HexCoordReader.Axial axial))
                    continue;

                if (!cellToEntities.TryGetValue(axial, out List<int> list))
                {
                    list = new List<int>();
                    cellToEntities[axial] = list;
                }
                list.Add(entityId);
            }
        }

        private static bool TryReadCoordinates(object gridMemberBoxed, out object hexBoxed)
        {
            hexBoxed = null;
            coordinatesField ??= typeof(GridMemberComponent).GetField(
                "coordinates", BindingFlags.Public | BindingFlags.Instance);
            if (coordinatesField == null)
            {
                InspectorLog.Warn("GridMemberComponent has no public 'coordinates' field — grid layout can't be built.");
                return false;
            }
            try { hexBoxed = coordinatesField.GetValue(gridMemberBoxed); }
            catch (System.Exception e) { InspectorLog.Warn("Reading GridMemberComponent.coordinates threw", e); return false; }
            return hexBoxed != null;
        }

        private static int FindComponentId(Type componentType)
        {
            for (int id = 0; id < ComponentRegistry.MaxComponents; id++)
            {
                if (ComponentRegistry.GetTypeForComponentID(id) == componentType)
                    return id;
            }
            return -1;
        }

        private void OnCellClicked(HexCoordReader.Axial axial)
        {
            selectedCell = axial;
            gridElement.SetSelected(axial);
            RefreshCellPane(force: true);

            // Cross-tool: announce the cell, and focus the timeline on the first entity
            // standing on it (if any), so clicking a cell lights up its commands.
            ATCG.Editor.EditorToolBus.Publish(new ATCG.Editor.CellSelectedEvent(axial.X, axial.Y));
            if (cellToEntities.TryGetValue(axial, out List<int> ids) && ids.Count > 0)
                ATCG.Editor.EditorToolBus.Publish(new ATCG.Editor.FocusEntityRequest(ids[0]));
        }

        private HexCoordReader.Axial? populatedCell;
        private bool populatedCellSet;

        private void RefreshCellPane(bool force)
        {
            if (cellPane == null)
                return;

            // Skip when the selection hasn't changed (e.g. a data tick), so open
            // foldouts in the pane survive auto-refresh.
            bool sameSelection = populatedCellSet
                && Nullable.Equals(populatedCell, selectedCell);
            if (!force && sameSelection)
                return;

            populatedCell = selectedCell;
            populatedCellSet = true;

            cellPane.Clear();

            if (selectedCell == null)
            {
                cellTitle.text = "No cell selected";
                return;
            }

            HexCoordReader.Axial axial = selectedCell.Value;
            cellTitle.text = $"Cell {axial}";

            if (!cellToEntities.TryGetValue(axial, out List<int> ids) || ids.Count == 0)
            {
                Label empty = new("(empty cell)");
                empty.AddToClassList("wi-empty");
                cellPane.Add(empty);
                return;
            }

            foreach (int id in ids)
            {
                EntityLabel.Info info = EntityLabel.Build(world, id);
                Foldout f = new() { text = info.Name, value = true };
                f.AddToClassList("wi-component");
                VisualElement holder = new();
                componentView.Populate(holder, world, id);
                f.Add(holder);
                cellPane.Add(f);
            }
        }
    }
}
