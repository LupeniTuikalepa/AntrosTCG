using System.Collections.Generic;
using ATCG.Elements;
using ATCG.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Small modal to scaffold a new capacity: name, element, and a list of steps. On Create it hands
    /// off to CapacityGenerator, which writes the scripts (a [WithStep] + an Execute{Step} stub per
    /// declared step), then builds the assets after the recompile.
    /// </summary>
    public sealed class NewCapacityModal : EditorWindow
    {
        private string capacityName = string.Empty;
        private Element element = Element.Fire;
        private readonly List<string> steps = new();

        private VisualElement stepsContainer;

        public static void Open()
        {
            NewCapacityModal window = CreateInstance<NewCapacityModal>();
            window.titleContent = new GUIContent("New Capacity");
            window.minSize = new Vector2(380, 280);
            window.ShowModalUtility();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = root.style.paddingBottom = 10;
            root.style.paddingLeft = root.style.paddingRight = 10;

            root.Add(new Label("Create a new capacity")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 8 }
            });

            TextField nameField = new TextField("Name") { value = capacityName };
            nameField.RegisterValueChangedCallback(e => capacityName = e.newValue);
            root.Add(nameField);

            EnumField elementField = new EnumField("Element", element);
            elementField.RegisterValueChangedCallback(e => element = (Element)e.newValue);
            root.Add(elementField);

            root.Add(new Label("Steps")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 8 }
            });

            stepsContainer = new VisualElement();
            root.Add(stepsContainer);
            RebuildSteps();

            root.Add(new Button(() => { steps.Add(string.Empty); RebuildSteps(); }) { text = "＋ Add step" });

            root.Add(new Label("If left empty, one step named after the capacity is created.")
            {
                style = { opacity = 0.6f, whiteSpace = WhiteSpace.Normal, marginTop = 2 }
            });

            Label error = new Label
            {
                style = { color = new Color(0.9f, 0.4f, 0.4f), whiteSpace = WhiteSpace.Normal, marginTop = 4, display = DisplayStyle.None }
            };
            root.Add(error);

            VisualElement buttons = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 10 }
            };
            buttons.Add(new Button(Close) { text = "Cancel" });
            buttons.Add(new Button(() =>
            {
                if (CapacityGenerator.BeginCreate(capacityName, element, steps, out string message))
                {
                    Close();
                }
                else
                {
                    error.text = message;
                    error.style.display = DisplayStyle.Flex;
                }
            })
            { text = "Create" });
            root.Add(buttons);

            nameField.Focus();
        }

        private void RebuildSteps()
        {
            stepsContainer.Clear();
            for (int i = 0; i < steps.Count; i++)
            {
                int index = i;
                VisualElement row = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2 }
                };

                TextField field = new TextField { value = steps[index], style = { flexGrow = 1 } };
                field.RegisterValueChangedCallback(e => steps[index] = e.newValue);
                row.Add(field);

                row.Add(new Button(() => { steps.RemoveAt(index); RebuildSteps(); }) { text = "✕", style = { width = 22 } });
                stepsContainer.Add(row);
            }
        }
    }
}
