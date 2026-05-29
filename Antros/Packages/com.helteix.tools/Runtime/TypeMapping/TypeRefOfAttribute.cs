using System;

namespace Helteix.Tools.TypeMapping
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TypeRefOfAttribute : Attribute
    {
        public readonly Type type;

        public TypeRefOfAttribute(Type type)
        {
            this.type = type;
        }
    }
}