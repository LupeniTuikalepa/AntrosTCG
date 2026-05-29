using System;

namespace Helteix.Tools.Settings
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GameSettingsPathAttribute : Attribute
    {
        public readonly string path;

        public GameSettingsPathAttribute(string path)
        {
            this.path = path;
        }
    }
}