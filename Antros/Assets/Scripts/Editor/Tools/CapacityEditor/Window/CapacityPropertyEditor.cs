using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Capacities;
using ATCG.Capacities.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Edits a capacity's PropertyDefinitions as a table: Type / Array / Name / Preview
    /// Value. Structural changes (type, array, name, add/remove) persist to the asset;
    /// preview values are editor-only (EditorPrefs). Callbacks are registered ONCE in
    /// makeCell (cells are recycled), and bindCell only refreshes displayed values via
    /// SetValueWithoutNotify — registering callbacks in bindCell would stack duplicate
    /// handlers on recycled cells and cause a save/refresh feedback loop.
    /// </summary>
    public sealed class CapacityPropertyEditor
    {
        private readonly CapacityData capacity;
        private readonly string capacityGuid;
        private readonly List<Type> definitionTypes;
        private readonly List<string> typeLabels;

        private MultiColumnListView list;

        public CapacityPropertyEditor(CapacityData capacity)
        {
            this.capacity = capacity;
            capacityGuid = capacity != null
                ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(capacity))
                : string.Empty;
            definitionTypes = DiscoverDefinitionTypes();
            typeLabels = definitionTypes.Select(TypeLabel).ToList();
        }

        public VisualElement Build()
        {
            VisualElement root = new();
            if (capacity == null)
            {
                root.Add(new Label("No capacity selected."));
                return root;
            }

            if (capacity.PropertyDefinitions == null)
                EnsureNonNullList();

            list = new MultiColumnListView
            {
                itemsSource = capacity.PropertyDefinitions,
                showAddRemoveFooter = true,
                reorderable = true,
                selectionType = SelectionType.Single,
                // Dynamic row height so an array cell's inner ListView can expand the row
                // instead of being clipped to a single fixed-height line.
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = { minHeight = 160 }
            };

            list.columns.Add(new Column { title = "Type", width = 130, makeCell = MakeTypeCell, bindCell = BindTypeCell });
            list.columns.Add(new Column { title = "Array", width = 46, makeCell = MakeArrayCell, bindCell = BindArrayCell });
            list.columns.Add(new Column { title = "Name", width = 150, makeCell = MakeNameCell, bindCell = BindNameCell });
            list.columns.Add(new Column { title = "Preview Value", stretchable = true, makeCell = () => new VisualElement { style = { flexGrow = 1 } }, bindCell = BindValueCell });

            list.itemsAdded += OnItemsAdded;
            list.itemsRemoved += _ => Persist();

            root.Add(list);
            return root;
        }

        private ICapacityPropertyDefinition Def(int index)
            => index >= 0 && index < capacity.PropertyDefinitions.Count ? capacity.PropertyDefinitions[index] : null;

        private void OnItemsAdded(IEnumerable<int> indices)
        {
            if (definitionTypes.Count > 0)
                foreach (int i in indices)
                    if (capacity.PropertyDefinitions[i] == null)
                        capacity.PropertyDefinitions[i] = (ICapacityPropertyDefinition)Activator.CreateInstance(definitionTypes[0]);
            Persist();
            list.RefreshItems();
        }

        // ---- Type column (callback registered once) --------------------------

        private VisualElement MakeTypeCell()
        {
            DropdownField dropdown = new(typeLabels, 0);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = AsIndex(dropdown);
                ICapacityPropertyDefinition def = Def(index);
                if (def == null) return;

                int newIndex = typeLabels.IndexOf(evt.newValue);
                if (newIndex < 0 || def.GetType() == definitionTypes[newIndex]) return;

                var replacement = (ICapacityPropertyDefinition)Activator.CreateInstance(definitionTypes[newIndex]);
                replacement.Name = def.Name;
                replacement.IsArray = def.IsArray;
                capacity.PropertyDefinitions[index] = replacement;
                Persist();
                list.RefreshItems();
            });
            return dropdown;
        }

        private void BindTypeCell(VisualElement cell, int index)
        {
            DropdownField dropdown = (DropdownField)cell;
            dropdown.userData = index;
            ICapacityPropertyDefinition def = Def(index);
            int current = def != null ? definitionTypes.IndexOf(def.GetType()) : -1;
            dropdown.SetValueWithoutNotify(current >= 0 ? typeLabels[current] : null);
        }

        // ---- Array column ----------------------------------------------------

        private VisualElement MakeArrayCell()
        {
            Toggle toggle = new();
            toggle.RegisterValueChangedCallback(evt =>
            {
                ICapacityPropertyDefinition def = Def(AsIndex(toggle));
                if (def == null) return;
                def.IsArray = evt.newValue;
                Persist();
                list.RefreshItems();
            });
            return toggle;
        }

        private void BindArrayCell(VisualElement cell, int index)
        {
            Toggle toggle = (Toggle)cell;
            toggle.userData = index;
            ICapacityPropertyDefinition def = Def(index);
            toggle.SetValueWithoutNotify(def != null && def.IsArray);
        }

        // ---- Name column -----------------------------------------------------

        private VisualElement MakeNameCell()
        {
            TextField field = new();
            field.isDelayed = true; // persist on commit (Enter/blur), not per keystroke
            field.RegisterValueChangedCallback(evt =>
            {
                ICapacityPropertyDefinition def = Def(AsIndex(field));
                if (def == null) return;
                def.Name = evt.newValue;
                Persist();
                list.RefreshItems();
            });
            return field;
        }

        private void BindNameCell(VisualElement cell, int index)
        {
            TextField field = (TextField)cell;
            field.userData = index;
            ICapacityPropertyDefinition def = Def(index);
            field.SetValueWithoutNotify(def?.Name ?? string.Empty);
        }

        // ---- Preview Value column (rebuilt per bind; editor-only, no asset save) ----

        private void BindValueCell(VisualElement cell, int index)
        {
            cell.Clear();
            ICapacityPropertyDefinition def = Def(index);
            if (def == null || string.IsNullOrEmpty(def.Name) || def.ElementType == null)
                return;

            if (!CapacityDebugValueStore.IsEditable(def))
            {
                Label na = new($"{def.PropertyType.Name} — runtime only");
                na.SetEnabled(false);
                cell.Add(na);
                return;
            }

            cell.Add(def.IsArray ? BuildArrayField(def) : BuildScalarField(def, CurrentValue(def), null));
        }

        private object CurrentValue(ICapacityPropertyDefinition def)
            => CapacityDebugValueStore.TryGet(capacityGuid, def, out object v) ? v : null;

        private VisualElement BuildArrayField(ICapacityPropertyDefinition def)
        {
            Array current = CurrentValue(def) as Array;
            List<object> items = current != null ? current.Cast<object>().ToList() : new List<object>();

            ListView lv = new(items)
            {
                showAddRemoveFooter = true,
                reorderable = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = false,
                makeItem = () => new VisualElement()
            };
            // Let the list grow with its content; the parent row (DynamicHeight) follows.
            lv.style.flexGrow = 1;
            lv.bindItem = (e, i) =>
            {
                e.Clear();
                e.Add(BuildScalarField(def, items[i], v => { items[i] = v; SaveArray(def, items); }));
            };
            lv.itemsAdded += _ => { for (int i = 0; i < items.Count; i++) items[i] ??= DefaultOf(def.ElementType); SaveArray(def, items); };
            lv.itemsRemoved += _ => SaveArray(def, items);
            return lv;
        }

        private void SaveArray(ICapacityPropertyDefinition def, List<object> items)
        {
            Array arr = Array.CreateInstance(def.ElementType, items.Count);
            for (int i = 0; i < items.Count; i++)
                arr.SetValue(items[i] ?? DefaultOf(def.ElementType), i);
            CapacityDebugValueStore.Set(capacityGuid, def, arr);
            PushLive(def, arr);
        }

        private VisualElement BuildScalarField(ICapacityPropertyDefinition def, object value, Action<object> onChanged)
        {
            Type e = def.ElementType;
            void Emit(object v)
            {
                if (onChanged != null) onChanged(v);
                else { CapacityDebugValueStore.Set(capacityGuid, def, v); PushLive(def, v); }
            }

            if (e == typeof(float))
            {
                FloatField f = new() { value = value is float x ? x : 0f };
                f.RegisterValueChangedCallback(ev => Emit(ev.newValue));
                return f;
            }
            if (e == typeof(int))
            {
                IntegerField f = new() { value = value is int x ? x : 0 };
                f.RegisterValueChangedCallback(ev => Emit(ev.newValue));
                return f;
            }
            if (e == typeof(bool))
            {
                Toggle f = new() { value = value is bool x && x };
                f.RegisterValueChangedCallback(ev => Emit(ev.newValue));
                return f;
            }
            if (e == typeof(string))
            {
                TextField f = new() { value = value as string ?? string.Empty };
                f.RegisterValueChangedCallback(ev => Emit(ev.newValue));
                return f;
            }
            if (e == typeof(Vector3))
            {
                Vector3Field f = new() { value = value is Vector3 x ? x : Vector3.zero };
                f.RegisterValueChangedCallback(ev => Emit(ev.newValue));
                return f;
            }
            if (e == typeof(ATCG.HexGrids.HexCoordinates))
            {
                ATCG.HexGrids.HexCoordinates hc = value is ATCG.HexGrids.HexCoordinates h ? h : default;
                Vector2IntField f = new() { value = new Vector2Int(hc.X, hc.Y) };
                f.RegisterValueChangedCallback(ev => Emit(new ATCG.HexGrids.HexCoordinates(ev.newValue.x, ev.newValue.y)));
                return f;
            }
            return new Label("unsupported");
        }

        private static object DefaultOf(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;

        private void PushLive(ICapacityPropertyDefinition def, object value)
        {
            CapacityCutsceneStage stage = CapacityCutsceneStage.Current;
            if (stage != null && stage.Capacity == capacity && stage.PreviewContext != null)
                if (stage.PreviewContext.TrySetBoxed(def.Name, value))
                    stage.ReconnectElements();
        }

        // ---- persistence -----------------------------------------------------

        private void EnsureNonNullList()
        {
            SerializedObject so = new(capacity);
            SerializedProperty prop = so.FindProperty("<PropertyDefinitions>k__BackingField");
            if (prop != null)
            {
                prop.arraySize = 0;
                so.ApplyModifiedProperties();
            }
        }

        // Structural change only. Deferred one editor tick to avoid re-entrancy with
        // list rebinding, and never called for editor-only preview value edits.
        private void Persist()
        {
            EditorApplication.delayCall += () =>
            {
                if (capacity == null) return;
                EditorUtility.SetDirty(capacity);
                AssetDatabase.SaveAssetIfDirty(capacity);
            };
        }

        private static int AsIndex(VisualElement cell) => cell.userData is int i ? i : -1;

        private static List<Type> DiscoverDefinitionTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ICapacityPropertyDefinition>()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name)
                .ToList();
        }

        private static string TypeLabel(Type t)
        {
            string n = t.Name;
            const string suffix = "PropertyDefinition";
            return n.EndsWith(suffix) ? n.Substring(0, n.Length - suffix.Length) : n;
        }
    }
}