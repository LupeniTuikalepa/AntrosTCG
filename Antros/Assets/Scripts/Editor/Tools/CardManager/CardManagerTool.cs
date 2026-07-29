using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Cards;
using ATCG.Cards.Implementations;
using ATCG.Enums;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CardManager
{
    /// <summary>
    /// Antros TCG Editor tool: a multi-column, sortable, column-hideable list of every card asset under
    /// Resources/Database/Cards (filterable by rarity / element / type). Toggling a card's "Deck" cell
    /// stores its membership in the DebugStartingDeck singleton (not on the card), consumed by
    /// BattleLauncher.
    /// </summary>
    public sealed class CardManagerTool : IEditorTool
    {
        private const string CardsFolder = "Assets/Resources/Database/Cards";

        public string DisplayName => "Cards";
        public string Icon => "♠";
        public int Order => 50;

        private readonly List<GameCardData> allCards = new();
        private readonly List<GameCardData> filtered = new();

        private int rarityFilter;   // 0 = All, else (CardRarity)(index-1)
        private int elementFilter;  // 0 = All, else (Element)(index-1)
        private int typeFilter;     // 0 = All, 1 = Heroes, 2 = Constructions
        private string search = string.Empty;

        private MultiColumnListView list;
        private Label countLabel;
        private VisualElement inspectorContainer;

        public VisualElement BuildUI()
        {
            EnsureDeckAsset();
            LoadCards();

            VisualElement root = new VisualElement { style = { flexGrow = 1, minHeight = 0 } };
            root.Add(BuildToolbar());
            root.Add(BuildActions());

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

        public void OnActivated()
        {
            EnsureDeckAsset();
            LoadCards();
            Refresh();
        }

        // GameSettings.Current only returns a throwaway (non-persistent) instance when no asset exists,
        // which breaks Undo/SetDirty and never saves. Make sure a real asset exists and is preloaded so
        // DebugStartingDeck.Current resolves to it.
        private static void EnsureDeckAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:DebugStartingDeck");
            DebugStartingDeck asset = guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<DebugStartingDeck>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;

            if (asset == null)
            {
                const string folder = "Assets/Project/Settings";
                if (!AssetDatabase.IsValidFolder(folder))
                    AssetDatabase.CreateFolder("Assets/Project", "Settings");

                asset = ScriptableObject.CreateInstance<DebugStartingDeck>();
                AssetDatabase.CreateAsset(asset, folder + "/DebugStartingDeck.asset");
                AssetDatabase.SaveAssets();
            }

            List<UnityEngine.Object> preloaded = PlayerSettings.GetPreloadedAssets().ToList();
            if (!preloaded.Contains(asset))
            {
                preloaded.Add(asset);
                PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
            }
        }

        public void OnDeactivated()
        {
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new Toolbar();

            toolbar.Add(Popup("Rarity", RarityOptions(), i => { rarityFilter = i; Refresh(); }));
            toolbar.Add(Popup("Element", ElementOptions(), i => { elementFilter = i; Refresh(); }));
            toolbar.Add(Popup("Type", TypeOptions.ToList(), i => { typeFilter = i; Refresh(); }));

            ToolbarSearchField searchField = new ToolbarSearchField { style = { flexGrow = 1 } };
            searchField.RegisterValueChangedCallback(e => { search = e.newValue; Refresh(); });
            toolbar.Add(searchField);

            toolbar.Add(new ToolbarButton(() => { LoadCards(); Refresh(); }) { text = "Refresh" });
            return toolbar;
        }

        private VisualElement BuildActions()
        {
            VisualElement actions = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 2, marginBottom = 2, marginLeft = 4, marginRight = 4 }
            };

            countLabel = new Label { style = { flexGrow = 1 } };
            actions.Add(countLabel);
            actions.Add(new Button(() => SetAll(true)) { text = "Enable filtered" });
            actions.Add(new Button(() => SetAll(false)) { text = "Disable filtered" });
            return actions;
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

            list.columns.Add(new Column
            {
                name = "deck", title = "Deck", width = 46, sortable = true, optional = false,
                makeCell = MakeToggleCell, bindCell = BindToggleCell,
            });
            list.columns.Add(new Column
            {
                name = "name", title = "Name", stretchable = true, minWidth = 140, optional = false,
                makeCell = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 4 } },
                bindCell = (e, i) => ((Label)e).text = CardTitle(filtered[i]),
            });
            list.columns.Add(new Column
            {
                name = "rarity", title = "Rarity", width = 110, optional = true,
                makeCell = MakeRarityCell, bindCell = BindRarityCell,
            });
            list.columns.Add(new Column
            {
                name = "element", title = "Element", width = 110, optional = true,
                makeCell = MakeElementCell, bindCell = BindElementCell,
            });
            list.columns.Add(new Column
            {
                name = "type", title = "Type", width = 110, optional = true,
                makeCell = () => new Label { style = { opacity = 0.7f } }, bindCell = (e, i) => ((Label)e).text = TypeName(filtered[i]),
            });
            list.columns.Add(new Column
            {
                name = "ping", title = string.Empty, width = 30, sortable = false, optional = true,
                makeCell = () => new Button { text = "→" },
                bindCell = (e, i) =>
                {
                    GameCardData card = filtered[i];
                    ((Button)e).clickable = new Clickable(() => EditorGUIUtility.PingObject(card));
                },
            });

            list.columnSortingChanged += () => { SortFiltered(); list.RefreshItems(); };
            list.selectedIndicesChanged += _ => UpdateInspector();
            return list;
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
            Label hint = new Label("Cliquez sur une carte pour en sélectionner une")
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
            };
            inspectorContainer.Add(hint);
        }

        private VisualElement MakeToggleCell()
        {
            Toggle toggle = new Toggle { style = { alignSelf = Align.Center } };
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (toggle.userData is GameCardData card)
                {
                    SetActive(card, evt.newValue);
                    UpdateCount();
                }
            });
            return toggle;
        }

        private void BindToggleCell(VisualElement cell, int index)
        {
            GameCardData card = filtered[index];
            Toggle toggle = (Toggle)cell;
            toggle.userData = card;
            toggle.SetValueWithoutNotify(DebugStartingDeck.Current.IsActive(card));
        }

        private static VisualElement MakeRarityCell()
        {
            EnumField field = new EnumField(CardRarity.Common) { style = { marginTop = 1, marginBottom = 1 } };
            field.RegisterValueChangedCallback(evt =>
            {
                if (field.userData is GameCardData card && evt.newValue is CardRarity rarity)
                {
                    Undo.RecordObject(card, "Change card rarity");
                    card.EditorSetRarity(rarity);
                    EditorUtility.SetDirty(card);
                }
            });
            return field;
        }

        private void BindRarityCell(VisualElement cell, int index)
        {
            EnumField field = (EnumField)cell;
            field.userData = filtered[index];
            field.SetValueWithoutNotify(filtered[index].Rarity);
        }

        private static VisualElement MakeElementCell()
        {
            EnumField field = new EnumField(Element.Time) { style = { marginTop = 1, marginBottom = 1 } };
            field.RegisterValueChangedCallback(evt =>
            {
                if (field.userData is GameCardData card && evt.newValue is Element element)
                {
                    Undo.RecordObject(card, "Change card element");
                    card.EditorSetElement(element);
                    EditorUtility.SetDirty(card);
                }
            });
            return field;
        }

        private void BindElementCell(VisualElement cell, int index)
        {
            EnumField field = (EnumField)cell;
            field.userData = filtered[index];
            field.SetValueWithoutNotify(filtered[index].Element);
        }

        private static PopupField<string> Popup(string label, List<string> options, Action<int> onChange)
        {
            PopupField<string> popup = new PopupField<string>(label, options, 0) { style = { marginRight = 6 } };
            popup.RegisterValueChangedCallback(_ => onChange(popup.index));
            return popup;
        }

        private void LoadCards()
        {
            allCards.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:GameCardData", new[] { CardsFolder }))
            {
                GameCardData card = AssetDatabase.LoadAssetAtPath<GameCardData>(AssetDatabase.GUIDToAssetPath(guid));
                if (card != null)
                    allCards.Add(card);
            }

            allCards.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        }

        private void Refresh()
        {
            filtered.Clear();
            filtered.AddRange(allCards.Where(Passes));
            SortFiltered();
            list?.RefreshItems();
            list?.ClearSelection();
            ShowInspectorEmpty();
            UpdateCount();
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

        private static int Compare(string column, GameCardData a, GameCardData b) => column switch
        {
            "deck" => DebugStartingDeck.Current.IsActive(a).CompareTo(DebugStartingDeck.Current.IsActive(b)),
            "rarity" => ((int)a.Rarity).CompareTo((int)b.Rarity),
            "element" => ((int)a.Element).CompareTo((int)b.Element),
            "type" => string.Compare(TypeName(a), TypeName(b), StringComparison.Ordinal),
            _ => string.Compare(CardTitle(a), CardTitle(b), StringComparison.OrdinalIgnoreCase),
        };

        private bool Passes(GameCardData card)
        {
            if (card == null)
                return false;
            if (rarityFilter > 0 && (int)card.Rarity != rarityFilter - 1)
                return false;
            if (elementFilter > 0 && (int)card.Element != elementFilter - 1)
                return false;
            if (typeFilter == 1 && card is not HeroCardData)
                return false;
            if (typeFilter == 2 && card is not ConstructionCardData)
                return false;
            if (!string.IsNullOrEmpty(search) && CardTitle(card).IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void UpdateCount()
        {
            if (countLabel == null)
                return;

            int active = filtered.Count(c => DebugStartingDeck.Current.IsActive(c));
            countLabel.text = $"{active} / {filtered.Count} in deck";
        }

        private void SetAll(bool value)
        {
            DebugStartingDeck deck = DebugStartingDeck.Current;
            Undo.RecordObject(deck, "Toggle cards in starting deck");
            foreach (GameCardData card in filtered)
                deck.EditorSetActive(card, value);

            list?.RefreshItems();
            UpdateCount();
        }

        private static void SetActive(GameCardData card, bool value)
        {
            DebugStartingDeck deck = DebugStartingDeck.Current;
            Undo.RecordObject(deck, "Toggle card in starting deck");
            deck.EditorSetActive(card, value);
        }

        private static string CardTitle(GameCardData card) => string.IsNullOrEmpty(card.Title) ? card.name : card.Title;

        private static string TypeName(GameCardData card)
            => card is HeroCardData ? "Hero" : card is ConstructionCardData ? "Construction" : card.GetType().Name;

        private static readonly string[] TypeOptions = { "All", "Heroes", "Constructions" };

        private static List<string> RarityOptions()
            => new List<string> { "All" }.Concat(Enum.GetNames(typeof(CardRarity))).ToList();

        private static List<string> ElementOptions()
            => new List<string> { "All" }.Concat(Enum.GetNames(typeof(Element))).ToList();
    }
}
