using System;
using System.Collections.Generic;
using System.Reflection;
using ATCG.Battle.Entities.Components;
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
        private World world;
        private WorldSnapshot snapshot;

        private VisualElement root;
        private HexGridElement gridElement;
        private ScrollView cellPane;
        private Label cellTitle;

        private readonly Dictionary<HexCoordReader.Axial, List<int>> cellToEntities = new();
        private readonly EntityComponentView componentView;
        private readonly Dictionary<string, bool> componentExpansion = new();

        private HexCoordReader.Axial? selectedCell;

        // Cached reflection: GridMemberComponent.coordinates field.
        private static FieldInfo coordinatesField;
        private int gridMemberComponentId = -1;

        public GridTabView()
        {
            componentView = new EntityComponentView(componentExpansion);
        }

        public VisualElement Root => root;

        public VisualElement Build()
        {
            root = new VisualElement();
            root.AddToClassList("wi-tab");
            root.style.flexGrow = 1;

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

        public void SetData(World world, WorldSnapshot snapshot)
        {
            this.world = world;
            this.snapshot = snapshot;
            RebuildCells();
            gridElement?.SetCells(cellToEntities, selectedCell);
            gridElement?.SetStartingLines(StartingLines.Compute());
            RefreshCellPane();
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
            catch { return; }
            if (store == null)
                return;

            foreach (int entityId in snapshot.EntityIds)
            {
                object boxed;
                try { boxed = store.GetBoxed(entityId); }
                catch { continue; }
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
                return false;
            try { hexBoxed = coordinatesField.GetValue(gridMemberBoxed); }
            catch { return false; }
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
            RefreshCellPane();
        }

        private void RefreshCellPane()
        {
            if (cellPane == null)
                return;
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
                Foldout f = new() { text = $"Entity {id}", value = false };
                f.AddToClassList("wi-component");
                VisualElement holder = new();
                componentView.Populate(holder, world, id);
                f.Add(holder);
                cellPane.Add(f);
            }
        }
    }
}