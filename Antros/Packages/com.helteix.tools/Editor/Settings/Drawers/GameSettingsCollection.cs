using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.Settings.Drawers
{
    public class GameSettingsCollection: VisualElement
    {
        private Button deleteButton;
        private Button createButton;
        private Button setAsDefaultButton;

        private TabView tabView;
        private Dictionary<ScriptableObject, Tab> tabDictionary;

        public Type SettingsType { get; internal set; }


        public GameSettingsCollection(Type settingsType)
        {
            SettingsType = settingsType;
            tabDictionary = new();
            tabView = new TabView()
            {
                reorderable = false,
            };
            Add(tabView);
            //tabView.activeTabChanged += OnActiveTabChanged;
            VisualElement buttonsContainer = new VisualElement()
            {
                style =
                {
                    position = Position.Absolute,
                    right = 0,
                    flexDirection = FlexDirection.Row,
                    flexShrink = 1,
                }
            };

            setAsDefaultButton = new Button(SetAsDefaultCurrent)
            {
                iconImage = Background.FromTexture2D(EditorGUIUtility.IconContent("Favorite").image as Texture2D),
                tooltip = "Set as default",
                style =
                {
                    flexShrink = 0,
                    flexGrow = 1,
                    width = 30,
                }
            };
            createButton = new Button(Create)
            {
                iconImage = Background.FromTexture2D(EditorGUIUtility.IconContent("d_CreateAddNew").image as Texture2D),
                tooltip = "Duplicate settings asset",
                style =
                {
                    flexShrink = 0,
                    flexGrow = 1,
                    width = 30,
                }
            };
            deleteButton = new Button(DeleteCurrent)
            {
                iconImage = Background.FromTexture2D(EditorGUIUtility.IconContent("Remove").image as Texture2D),
                tooltip = "Delete settings asset",
                style =
                {
                    flexShrink = 0,
                    flexGrow = 1,
                    width = 30,
                }
            };

            buttonsContainer.Add(setAsDefaultButton);
            buttonsContainer.Add(createButton);
            buttonsContainer.Add(deleteButton);

            tabView.contentViewport.Add(buttonsContainer);

            RegisterCallback<AttachToPanelEvent>(AttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(DetachedFromPanel);

            BuildTabs();
            UpdateHeaders();

            tabView.activeTabChanged += OnActiveTabChanges;
            OnActiveTabChanges(null, tabView.activeTab);
        }

        private void AttachedToPanel(AttachToPanelEvent evt)
        {
            GameSettingsAssetDatabase.OnSettingsWillBeDeleted += OnSettingsWillBeDeleted;
            GameSettingsEditorPrefs.instance.OnNewActiveSettings += OnNewActiveSettings;
        }

        private void DetachedFromPanel(DetachFromPanelEvent evt)
        {
            GameSettingsEditorPrefs.instance.OnNewActiveSettings -= OnNewActiveSettings;
            GameSettingsAssetDatabase.OnSettingsWillBeDeleted -= OnSettingsWillBeDeleted;
        }


        private void OnNewActiveSettings(ScriptableObject scriptableObject, Type type)
        {
            if (type == SettingsType)
            {
                UpdateHeaders();
                OnActiveTabChanges(tabView.activeTab, tabView.activeTab);
            }
        }

        private void OnActiveTabChanges(Tab from, Tab to)
        {
            foreach ((ScriptableObject scriptableObject, Tab tab) in tabDictionary)
            {
                if (tab == to)
                    setAsDefaultButton.SetEnabled(!GameSettingsEditorPrefs.instance.IsDefault(scriptableObject));
            }
        }


        private void BuildTabs()
        {
            ScriptableObject[] settings = GameSettingsAssetDatabase.GetOfType(SettingsType).ToArray();
            tabView.Clear();
            tabDictionary.Clear();

            for (int i = 0; i < settings.Length; i++)
            {
                ScriptableObject scriptableObject = settings[i];
                if(scriptableObject == null)
                    continue;

                var tab = new Tab(scriptableObject.name, EditorGUIUtility.FindTexture("Favorite"))
                {
                    closeable = false,
                };

                InspectorElement inspectorElement = new InspectorElement(scriptableObject);
                inspectorElement.SetEnabled(AssetDatabase.GetAssetPath(scriptableObject).StartsWith("Assets"));
                tab.Add(inspectorElement);

                tabView.Add(tab);

                VisualElement header = tabView.GetTabHeader(i);
                header.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) => { }));

                tabDictionary.Add(scriptableObject, tab);
                header.RegisterCallback((ContextualMenuPopulateEvent evt) =>
                {
                    evt.menu.AppendAction("Set as default", (x) => SetAsDefault(scriptableObject));
                    evt.menu.AppendAction("Delete", (x) => Delete(scriptableObject));
                    evt.menu.AppendAction("Duplicate", (x) => Create());
                });
            }
        }

        private void UpdateHeaders()
        {
            foreach ((ScriptableObject scriptableObject, Tab tab) in tabDictionary)
            {
                bool isDefault = GameSettingsEditorPrefs.instance.IsDefault(scriptableObject);

                tab.tabHeader.style.unityFontStyleAndWeight = isDefault ? FontStyle.Bold : FontStyle.Normal;
                tab.tabHeader.Q<Image>().style.display = isDefault ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnSettingsWillBeDeleted(ScriptableObject so)
        {
            if (tabDictionary.TryGetValue(so, out var tab))
                tabView.Remove(tab);
        }

        private void DeleteCurrent()
        {
            foreach ((ScriptableObject scriptableObject, Tab tab) in tabDictionary)
            {
                if (tab == tabView.activeTab)
                {
                    Delete(scriptableObject);
                    return;
                }
            }
        }

        private void SetAsDefaultCurrent()
        {
            foreach ((ScriptableObject scriptableObject, Tab tab) in tabDictionary)
            {
                if (tab == tabView.activeTab)
                {
                    SetAsDefault(scriptableObject);
                    return;
                }
            }
        }

        private void Delete(ScriptableObject scriptableObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(scriptableObject);
            AssetDatabase.DeleteAsset(assetPath);
        }

        private void Create()
        {
            GameSettingsEditorPrefs.instance.CreateNewSettings(SettingsType);
        }

        private void SetAsDefault(ScriptableObject scriptableObject)
        {
            GameSettingsEditorPrefs.instance.SetAsDefault(scriptableObject, SettingsType);
        }
    }
}