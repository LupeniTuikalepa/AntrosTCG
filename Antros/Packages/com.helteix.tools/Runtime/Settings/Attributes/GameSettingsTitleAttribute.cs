using System;
using UnityEngine;

namespace Helteix.Tools.Settings
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GameSettingsTitleAttribute : Attribute
    {
        public static Color DefaultColor { get; } = new Color(0.2f, 0.54f, 0.8f);

        public readonly string title;
        public readonly Color color;

        public GameSettingsTitleAttribute(string title)
        {
            this.title = title;
            color = DefaultColor;
        }

        public GameSettingsTitleAttribute(string title, string color)
        {
            this.title = title;
            if (ColorUtility.TryParseHtmlString(color, out Color c))
                this.color = c;
            else
                this.color = DefaultColor;
        }
    }
}