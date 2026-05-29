using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.Common
{
    public static class LTXEditorExtensions
    {
        public static void ShowWithLTXUss(this VisualElement visualElement)
        {
            visualElement.AddToClassList("panel-enable");
            visualElement.RemoveFromClassList("panel-disable");
        }

        public static void HideWithLTXUss(this VisualElement visualElement)
        {
            visualElement.RemoveFromClassList("panel-enable");
            visualElement.AddToClassList("panel-disable");
        }
        public static void ShowManually(this VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.Flex;
        }

        public static void HideManually(this VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.None;
        }
    }
}