using System;

namespace Helteix.Tools.SerializedComponent
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class UseDefaultEditorDrawingInsideSerializedComponentsAttribute : Attribute
    {

    }
}