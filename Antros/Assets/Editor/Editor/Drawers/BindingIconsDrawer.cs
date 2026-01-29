using Helteix.ControlDisplay.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace Helteix.ControlDisplay.Editor.Editor.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BindingIcons))]
    public class BindingIconsDrawer : PropertyDrawer
    {

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };

            return container;
        }
    }
}