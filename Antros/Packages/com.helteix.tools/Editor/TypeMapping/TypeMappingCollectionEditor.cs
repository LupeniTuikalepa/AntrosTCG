using Helteix.Tools.TypeMapping;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.TypeMapping
{
    [CustomEditor(typeof(TypeMappingCollection))]
    public class TypeMappingCollectionEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement visualElement = new VisualElement();
            InspectorElement.FillDefaultInspector(visualElement, serializedObject, this);

            visualElement.SetEnabled(false);

            return visualElement;
        }
    }
}