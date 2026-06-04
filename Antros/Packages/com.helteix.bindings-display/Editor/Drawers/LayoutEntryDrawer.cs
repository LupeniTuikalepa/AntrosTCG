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
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,
                fixedItemHeight = 75,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                style =
                {
                    maxHeight = 750
                }
            };

            FillColumnView(multiColumnListView.columns, property, bindingProperty);

            PropertyField prefabProperty = new PropertyField(property.FindBackingFieldPropertyRelative(nameof(LayoutEntry.BindingPrefab)));
            SerializedProperty spriteVariantsCountProperty = property.serializedObject.FindBackingFieldProperty(nameof(BindingDisplaySettings.SpriteVariantsCount));
            multiColumnListView.TrackPropertyValue(spriteVariantsCountProperty, _ =>
            {
                multiColumnListView.Unbind();
                FillColumnView(multiColumnListView.columns, property, bindingProperty);
                multiColumnListView.Bind(bindingProperty.serializedObject);
            });

            container.Add(multiColumnListView);
            container.Add(prefabProperty);

            multiColumnListView.BindProperty(bindingProperty);
            container.Bind(bindingProperty.serializedObject);
            return container;
        }

        private void FillColumnView(Columns columns, SerializedProperty property, SerializedProperty bindingProperty)
        {
            columns.Clear();

            // Colonne "Match"
            columns.Add(new Column()
            {
                title = "Control",
                stretchable = true,
                resizable = false,
                optional = false,
                makeCell = () =>
                {
                    IMGUIContainer controlPathContainer = new IMGUIContainer()
                    {
                        name = "controlPathContainer",
                        style =
                        {
                            flexGrow = 0,
                            flexShrink = 1,
                        }
                    };
                    return controlPathContainer;
                },
                bindCell = (element, index) =>
                {

                    SetCellParentStyle(element);
                    //Debug.Log("binding cell Match");
                    SerializedProperty itemProperty = bindingProperty.GetArrayElementAtIndex(index);
                    SerializedProperty controlPath = itemProperty.FindBackingFieldPropertyRelative(nameof(BindingEntry.ControlPath));

                    var picker = new InputControlPickerState();
                    InputControlPathEditor editor = new InputControlPathEditor(controlPath, picker, () =>
                    {
                        Debug.Log($"Control selected: {controlPath.stringValue}");
                        // Force l'application des modifications
                        controlPath.serializedObject.ApplyModifiedProperties();
                    }, GUIContent.none);

                    string devicePath = property.FindBackingFieldPropertyRelative(nameof(LayoutEntry.Layout)).stringValue;
                    editor.SetControlPathsToMatch(new string[] { $"<{devicePath}>/*"});

                    IMGUIContainer controlPathContainer = (IMGUIContainer)element;
                    controlPathContainer.onGUIHandler = () =>
                    {
                        editor.OnGUI();
                    };
                },
                unbindCell = (element, index) =>
                {
                    //Debug.Log("Unbinding Match");
                    TextField regexField = element.Q<TextField>("regexField");
                    IMGUIContainer controlPathContainer = element.Q<IMGUIContainer>("controlPathContainer");

                    regexField?.Unbind();
                    if (controlPathContainer != null)
                        controlPathContainer.onGUIHandler = null;
                }
            });

            // Colonne "Override prefab"
            columns.Add(new Column()
            {
                title = "Override prefab",
                resizable = false,
                stretchable = true,
                optional = true,
                makeCell = () => new ObjectField() {
                    objectType = typeof(GameObject),
                    style =
                    {
                        paddingRight = 5,
                        paddingLeft = 5,
                    }
                },
                bindCell = (element, index) =>
                {
                    SetCellParentStyle(element);
                    var objectField = element as ObjectField;
                    SerializedProperty itemProperty = bindingProperty.GetArrayElementAtIndex(index);
                    SerializedProperty overridePrefab = itemProperty.FindPropertyRelative(nameof(BindingEntry.OverridePrefab).GetBackingFieldPath());
                    objectField.BindProperty(overridePrefab);
                },
                unbindCell = (element, index) =>
                {
                    (element as ObjectField)?.Unbind();
                }
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

                // Colonnes Icon variants - SANS bindingPath sur la colonne
                for (int i = 0; i < context.SpriteVariantCount; i++)
                {
                    int capturedIndex = i; // Capture pour le closure
                    columns.Add(new Column()
                    {
                        title = $"Icon variant {i}",
                        resizable = false,
                        stretchable = true,
                        optional = false,
                        makeCell = () =>
                        {
                            VisualElement container = new VisualElement()
                            {
                                style =
                                {
                                    flexDirection = FlexDirection.Column,
                                    flexShrink = 1,
                                    backgroundColor = Color.gray2,
                                    marginBottom = 5,
                                    marginTop = 5,
                                    marginLeft = 10,
                                    marginRight = 10,
                                    borderBottomLeftRadius = 5,
                                    borderBottomRightRadius = 5,
                                    borderTopLeftRadius = 5,
                                    borderTopRightRadius = 5,
                                }
                            };

                            ObjectField objectField = new ObjectField()
                            {
                                name = "objectField",
                                objectType = typeof(Sprite),
                                allowSceneObjects = false,
                                style =
                                {
                                    paddingLeft = 5,
                                    paddingRight = 5,
                                },
                            };

                            Image image = new Image()
                            {
                                name = "image",
                                style =
                                {
                                    height = 60
                                }
                            };

                            container.Add(image);
                            container.Add(objectField);

                            return container;
                        },
                        bindCell = (element, index) =>
                        {
                            //Debug.Log($"binding cell Icon variant {capturedIndex}");
                            SerializedProperty itemProperty = bindingProperty.GetArrayElementAtIndex(index);
                            SerializedProperty iconsArray = itemProperty
                                .FindBackingFieldPropertyRelative(nameof(BindingEntry.Icons))
                                .FindPropertyRelative("icons");
                            SerializedProperty iconProperty = iconsArray.GetArrayElementAtIndex(capturedIndex);

                            ObjectField objectField = element.Q<ObjectField>("objectField");
                            Image image = element.Q<Image>("image");

                            objectField.BindProperty(iconProperty);

                            objectField.RegisterValueChangedCallback(ctx =>
                            {
                                _ = SetPreview(ctx.newValue, image);
                            });

                            // Set initial preview
                            if (iconProperty.objectReferenceValue != null)
                            {
                                _ = SetPreview(iconProperty.objectReferenceValue, image);
                            }
                        },
                        unbindCell = (element, index) =>
                        {
                            //Debug.Log($"Unbinding Icon variant {capturedIndex}");
                            ObjectField objectField = element.Q<ObjectField>("objectField");
                            objectField?.Unbind();
                        }
                    });
                }
            }
            else
            {
                // Colonne Icons fallback - SANS bindingPath
                columns.Add(new Column()
                {
                    title = "Icons",
                    resizable = false,
                    stretchable = true,
                    makeCell = () => new PropertyField(),
                    bindCell = (element, index) =>
                    {
                        var propertyField = element as PropertyField;
                        SerializedProperty itemProperty = bindingProperty.GetArrayElementAtIndex(index);
                        SerializedProperty icons = itemProperty.FindPropertyRelative($"{nameof(BindingEntry.Icons).GetBackingFieldPath()}.icons");
                        propertyField.BindProperty(icons);
                    },
                    unbindCell = (element, index) =>
                    {
                        (element as PropertyField)?.Unbind();
                    }
                });
            }
        }

        private static void SetCellParentStyle(VisualElement element)
        {
            element.parent.style.flexGrow = 1;
            element.parent.style.justifyContent = Justify.Center;
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