using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helteix.ControlDisplay.Data;
using Helteix.Tools.Editor.Common;
using Helteix.Tools.Editor.Serialisation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;
using UnityEngine.UIElements;

namespace Helteix.ControlDisplay.Editor.Editor.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LayoutEntry))]
    public class LayoutEntryDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement()
            {
                style =
                {
                    paddingBottom = 15,
                    paddingTop = 15,
                    paddingLeft = 25,
                    paddingRight = 25,
                    backgroundColor = Color.gray2
                }
            };

            SerializedProperty bindingProperty = property.FindBackingFieldPropertyRelative(nameof(LayoutEntry.BindingEntry));


            MultiColumnListView multiColumnListView = new MultiColumnListView(new Columns()
            {
                resizable = false,
                stretchMode = Columns.StretchMode.GrowAndFill,
            })
            {
                allowAdd = true,
                allowRemove = true,
                showAddRemoveFooter = true,
                showBoundCollectionSize = false,
                reorderable = true,
                reorderMode = ListViewReorderMode.Simple,
                showBorder = true,
                showFoldoutHeader = false,
                fixedItemHeight = 75,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                //bindingPath = bindingProperty.propertyPath,
            };
            FillColumnView(multiColumnListView.columns, property, bindingProperty);

            PropertyField fallbackProperty = new PropertyField(property.FindBackingFieldPropertyRelative(nameof(LayoutEntry.FallbackIcons)));
            PropertyField prefabProperty = new PropertyField(property.FindBackingFieldPropertyRelative(nameof(LayoutEntry.BindingPrefab)));
            SerializedProperty spriteVariantsCountProperty = property.serializedObject.FindBackingFieldProperty(nameof(BindingDisplaySettings.SpriteVariantsCount));
            multiColumnListView.TrackPropertyValue(spriteVariantsCountProperty, _ =>
            {
                multiColumnListView.Unbind();
                FillColumnView(multiColumnListView.columns, property, bindingProperty);
                multiColumnListView.Bind(bindingProperty.serializedObject);
            });

            container.Add(multiColumnListView);
            container.Add(fallbackProperty);
            container.Add(prefabProperty);

            multiColumnListView.BindProperty(bindingProperty);
            container.Bind(bindingProperty.serializedObject);
            return container;
        }

        private void FillColumnView(Columns columns, SerializedProperty property, SerializedProperty bindingProperty)
        {
            columns.Clear();
            columns.Add(new Column()
            {
                title = "Is Regex",
                width = 60,
                bindingPath = "match.isRegex",
                resizable = false,
                optional = true,
            });
            columns.Add(new Column()
            {
                title = "Match",
                stretchable = true,
                resizable = false,
                optional = false,
                makeCell = () =>
                {
                    // Utilise un VisualElement simple, pas de BindableElement avec bindingPath
                    VisualElement container = new VisualElement();

                    TextField regexField = new TextField()
                    {
                        name = "regexField" // Ajoute un nom pour faciliter la recherche
                    };

                    IMGUIContainer controlPathContainer = new IMGUIContainer()
                    {
                        name = "controlPathContainer"
                    };

                    container.Add(regexField);
                    container.Add(controlPathContainer);
                    return container;
                },
                bindCell = (element, index) =>
                {
                    Debug.Log("binding cell");
                    SerializedProperty itemProperty = bindingProperty.GetArrayElementAtIndex(index);
                    SerializedProperty match = itemProperty.FindPropertyRelative("match");
                    SerializedProperty isRegex = match.FindPropertyRelative("isRegex");
                    SerializedProperty regexPattern = match.FindPropertyRelative("regexPattern");
                    SerializedProperty controlPath = match.FindPropertyRelative("controlPath");

                    TextField regexField = element.Q<TextField>("regexField");
                    IMGUIContainer controlPathContainer = element.Q<IMGUIContainer>("controlPathContainer");

                    // Bind manuellement le TextField
                    regexField.BindProperty(regexPattern);

                    var picker = new InputControlPickerState();
                    InputControlPathEditor editor = new InputControlPathEditor(controlPath, picker, () =>
                    {
                        Debug.Log("a");
                    });
                    editor.SetExpectedControlLayout(property
                        .FindBackingFieldPropertyRelative(nameof(LayoutEntry.Layout)).stringValue);

                    controlPathContainer.onGUIHandler = () =>
                    {
                        editor.OnGUI();
                    };

                    if (isRegex.boolValue)
                    {
                        Debug.Log("la");
                        regexField.ShowManually();
                        controlPathContainer.HideManually();
                    }
                    else
                    {
                        Debug.Log("ici");
                        regexField.HideManually();
                        controlPathContainer.ShowManually();
                    }
                },
                unbindCell = (element, index) =>
                {
                    Debug.Log("Unbinding");

                    // Important : nettoie les bindings et handlers
                    TextField regexField = element.Q<TextField>("regexField");
                    IMGUIContainer controlPathContainer = element.Q<IMGUIContainer>("controlPathContainer");

                    regexField?.Unbind();
                    if (controlPathContainer != null)
                        controlPathContainer.onGUIHandler = null;
                }
            });

            columns.Add(new Column()
            {
                title = "Override prefab",
                bindingPath = nameof(BindingEntry.OverridePrefab).GetBackingFieldPath(),
                resizable = false,
                stretchable = true,
                optional = true,
            });

            SerializedObject propertySerializedObject = property.serializedObject;
            if (BindingDisplayContextProvider.TryGetContext(propertySerializedObject, out var context))
            {
                for (int i = 0; i < bindingProperty.arraySize; i++)
                {
                    SerializedProperty element = bindingProperty.GetArrayElementAtIndex(i);
                    SerializedProperty iconsProperty = element
                        .FindBackingFieldPropertyRelative(nameof(BindingEntry.Icons))
                        .FindPropertyRelative("icons");

                    iconsProperty.arraySize = context.SpriteVariantCount;
                }

                bindingProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                for (int i = 0; i < context.SpriteVariantCount; i++)
                {
                    string bindingPath = $"{nameof(BindingEntry.Icons).GetBackingFieldPath()}";
                    int index = i;
                    columns.Add(new Column()
                    {
                        title = $"Icon variant {i}",
                        bindingPath = bindingPath,
                        resizable = false,
                        stretchable = true,
                        optional = false,
                        bindCell = (_,_) => Debug.Log("frr"),
                        makeCell = () =>
                        {
                            BindableElement bindableElement = new BindableElement()
                            {
                                style =
                                {
                                    flexDirection = FlexDirection.Column,
                                    flexShrink = 1,
                                }
                            };
                            ObjectField objectField =  new ObjectField()
                            {
                                objectType = typeof(Sprite),
                                bindingPath = $"icons.Array.data[{index}]",
                                allowSceneObjects = false,
                                style =
                                {
                                    paddingLeft = 5,
                                    paddingRight = 5,
                                },
                            };

                            Image image = new Image()
                            {
                                style =
                                {
                                    height = 60
                                }
                            };
                            objectField.RegisterValueChangedCallback(ctx =>
                            {
                                _ = SetPreview(ctx.newValue, image);
                            });

                            bindableElement.Add(objectField);
                            bindableElement.Add(image);

                            return bindableElement;
                        },
                    });
                }
            }
            else
            {
                columns.Add(new Column()
                {
                    title = "Icons",
                    bindingPath = $"{nameof(BindingEntry.Icons).GetBackingFieldPath()}.icons",
                    resizable = false,
                    stretchable = true,
                });
            }
        }

        private async Awaitable SetPreview(Object asset, Image image)
        {
            Texture2D assetPreview = AssetPreview.GetAssetPreview(asset);
            while (assetPreview == null)
            {
                await Task.Delay(30);
                assetPreview = AssetPreview.GetAssetPreview(asset);
            }

            if(image != null)
                image.image = assetPreview;
        }
    }
}