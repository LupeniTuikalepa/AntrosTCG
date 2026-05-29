using System;
using Helteix.Tools.TypeMapping;
using Helteix.Tools.Editor.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Helteix.Tools.Editor.TypeMapping
{
    [CustomPropertyDrawer(typeof(TypeRef))]
    public class TypeRefEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty guidProperty = property.FindPropertyRelative("guid");

            VisualElement container = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };

            ObjectField objectField = new ObjectField(property.displayName)
            {
                objectType = typeof(MonoScript),
                allowSceneObjects = false,
            };

            //objectField.Q<Label>().style.minWidth = 75;
            var attributes = fieldInfo.GetCustomAttributes(typeof(TypeRefOfAttribute), true);
            objectField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is not MonoScript monoScript)
                    return;

                if (attributes.Length > 0)
                {
                    var type = monoScript.GetClass();
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if (attributes[i] is not TypeRefOfAttribute att)
                            continue;

                        if (att.type.IsAssignableFrom(type))
                            continue;

                        objectField.SetValueWithoutNotify(evt.previousValue);
                        return;
                    }
                }

                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(monoScript, out string guid, out _))
                {
                    guidProperty.stringValue = guid;
                    property.serializedObject.ApplyModifiedProperties();
                }
            });

            if (attributes.Length != 0)
            {
                objectField.schedule
                    .Execute(() =>
                    {
                        if (DragAndDrop.objectReferences.Length == 0)
                        {
                            objectField.SetEnabled(true);
                            return;
                        }

                        if (DragAndDrop.objectReferences.Length > 1)
                        {
                            objectField.SetEnabled(false);
                            return;
                        }

                        var obj = DragAndDrop.objectReferences[0];
                        if (obj is not MonoScript monoScript)
                        {
                            objectField.SetEnabled(false);
                            return;
                        }

                        if (attributes.Length > 0)
                        {
                            var type = monoScript.GetClass();
                            objectField.SetEnabled(IsValid(attributes, type));
                            return;
                        }

                        objectField.SetEnabled(true);
                    })
                    .Every(10);
            }

            objectField.TrackPropertyValue(guidProperty, UpdateObjectField);
            UpdateObjectField(guidProperty);

            container.Add(objectField);
            objectField.RegisterCallbackOnce<AttachToPanelEvent>(ctx =>
            {
                VisualElement button = objectField.Q<VisualElement>(null, "unity-object-field__selector");
                if (button != null)
                {
                    button.SetEnabled(attributes.Length == 0);
                    if(attributes.Length > 0)
                        button.tooltip = $"Disable because a filter using {nameof(TypeRefOfAttribute)} is applied";
                }
            });
            return container;

            void UpdateObjectField(SerializedProperty ctx)
            {
                string guid = guidProperty.stringValue;
                Object result = null;
                if (!string.IsNullOrEmpty(guid))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    result = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                }

                objectField?.SetValueWithoutNotify(result);
            }
        }


        private static bool IsValid(object[] attributes, Type type)
        {
            bool isValid = true;
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is not TypeRefOfAttribute att)
                    continue;
                if (att.type.IsAssignableFrom(type))
                    continue;

                isValid = false;
                break;
            }

            return isValid;
        }
    }
}