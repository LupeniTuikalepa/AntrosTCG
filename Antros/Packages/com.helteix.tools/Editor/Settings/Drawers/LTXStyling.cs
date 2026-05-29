using UnityEngine;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.Settings.Drawers
{
    public static class LTXStyling
    {
        public static void StyleSettingsContentWithTitle(this VisualElement element, string title, Color color)
        {
            StyleLength padding = 10;
            StyleLength margin = 10;
            element.style.paddingBottom = padding;
            element.style.paddingTop = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
            element.style.marginBottom = margin;
            element.style.marginTop = margin;
            element.style.marginLeft = margin;
            element.style.marginRight = margin;

            element.Insert(0, new Label(title)
            {
                style =
                {
                    fontSize = 19,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingBottom = 3,
                    marginBottom = 10,
                    marginLeft = 0,
                    marginRight = 0,
                    borderBottomWidth = 3,
                    borderBottomColor = color
                }
            });
        }
    }
}