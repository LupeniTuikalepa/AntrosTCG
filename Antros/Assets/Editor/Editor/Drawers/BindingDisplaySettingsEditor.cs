using System;
using System.Collections.Generic;
using Helteix.ControlDisplay.Data;
using Helteix.Tools.Editor.Serialisation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Helteix.ControlDisplay.Editor.Editor.Editor.Drawers
{
    [CustomEditor(typeof(BindingDisplaySettings))]
    public class BindingDisplaySettingsEditor : UnityEditor.Editor
    {
        private BindingDisplayContextProvider contextProvider;

        public static List<string> SupportedLayouts = new List<string>()
        {
            "XInputControllerWindows",
            "SwitchProControllerHID",
            "DualShockGamepad",
            "Keyboard",
            "Gamepad",
            "Mouse",
            "Touchscreen",
        };

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement container = new VisualElement();
            SerializedProperty devices = serializedObject.FindBackingFieldProperty(nameof(BindingDisplaySettings.Devices));
            EnsureValidDeviceArray(devices);

            VisualElement settings = new VisualElement()
            {
                style =
                {
                    backgroundColor = Color.gray2,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                }
            };

            SerializedProperty spriteVariantsCountProperty = serializedObject.FindBackingFieldProperty(nameof(BindingDisplaySettings.SpriteVariantsCount));
            SerializedProperty copyListenersProperty = serializedObject.FindBackingFieldProperty(nameof(BindingDisplaySettings.CopyListenersListBeforeCallbacks));
            settings.Add(new PropertyField(spriteVariantsCountProperty));
            settings.Add(new PropertyField(copyListenersProperty));

            container.Add(settings);
            Label label = new Label("Devices")
            {
                style =
                {
                    paddingBottom = 5,
                    paddingTop = 5,
                    paddingLeft = 10,
                    paddingRight = 10,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            container.Add(label);
            TabView deviceTabView = new TabView()
            {

            };

            /*
            deviceTabView.TrackPropertyValue(spriteVariantsCountProperty, ctx =>
            {
                deviceTabView.Clear();
                BuildTabView(devices, deviceTabView);
            });*/

            BuildTabView(devices, deviceTabView);
            container.Add(deviceTabView);
            container.Bind(serializedObject);

            return container;
        }

        private static void BuildTabView(SerializedProperty devices, TabView deviceTabView)
        {
            for (int i = 0; i < devices.arraySize; i++)
            {
                SerializedProperty element = devices.GetArrayElementAtIndex(i);
                Tab tab = new Tab(element.FindBackingFieldPropertyRelative(nameof(LayoutEntry.Layout)).stringValue);
                tab.Add(new PropertyField(element));
                deviceTabView.Add(tab);
            }
        }

        private void OnEnable()
        {
            contextProvider = BindingDisplayContextProvider.AddContextProvider(serializedObject);
        }

        private void OnDisable()
        {
            BindingDisplayContextProvider.RemoveContext(serializedObject);
        }

        private void EnsureValidDeviceArray(SerializedProperty arrayProperty)
        {
            using (DictionaryPool<string, LayoutEntry>.Get(out var validIndices))
            {
                for (int i = 0; i < arrayProperty.arraySize; i++)
                {
                    SerializedProperty arrayElement = arrayProperty.GetArrayElementAtIndex(i);
                    if (arrayElement.boxedValue is LayoutEntry layoutEntry)
                        validIndices.TryAdd(layoutEntry.Layout, layoutEntry);
                }

                arrayProperty.ClearArray();

                arrayProperty.arraySize = SupportedLayouts.Count;
                for (int i = 0; i < arrayProperty.arraySize; i++)
                {
                    SerializedProperty arrayElementAtIndex = arrayProperty.GetArrayElementAtIndex(i);
                    string targetLayout = SupportedLayouts[i];
                    if (validIndices.TryGetValue(targetLayout, out var entry))
                        arrayElementAtIndex.boxedValue = entry;
                    else
                    {
                        arrayElementAtIndex.boxedValue = new LayoutEntry();
                        arrayElementAtIndex.FindBackingFieldPropertyRelative(nameof(LayoutEntry.Layout)).stringValue = targetLayout;
                    }
                }

                arrayProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}