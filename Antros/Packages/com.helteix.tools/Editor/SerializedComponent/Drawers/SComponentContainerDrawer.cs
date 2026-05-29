using System;
using System.Reflection;
using Helteix.Tools.Editor.SerializedComponent.UIToolkit;
using Helteix.Tools.SerializedComponent;
using Helteix.Tools.SerializedComponent.Containers;
using UnityEditor;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.SerializedComponent.Drawers
{
    [CustomPropertyDrawer(typeof(SComponentContainer<>))]
    public class SComponentContainerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Type fieldType = fieldInfo.FieldType;

            Type typeConstraint = fieldType.GetGenericArguments()[0];

            string pathConstraint = string.Empty;
            bool showNonCompatible = false;

            foreach (var o in fieldInfo.GetCustomAttributes())
            {
                if (o is FilterPathSComponentsAttribute filterPathSComponentsAttribute)
                    pathConstraint = filterPathSComponentsAttribute.pathConstraint;
                if (o is ShowNonCompatibleSComponentsAttribute )
                    showNonCompatible = true;

            }


            return new SComponentElement(property, typeConstraint, pathConstraint, showNonCompatible);
        }
    }
}