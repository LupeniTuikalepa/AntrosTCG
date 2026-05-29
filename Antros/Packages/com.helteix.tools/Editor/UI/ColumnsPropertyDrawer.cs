using ATCG;
using Helteix.Tools.Editor.Serialisation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor
{
    [CustomPropertyDrawer(typeof(Columns<>))]
    public class ColumnsPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var multiColumnListView = new MultiColumnListView
            {
                bindingPath = "columns",
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                allowAdd = false,
                allowRemove = false,
                reorderable = false,
                headerTitle = "Columns",
            };
            //multiColumnListView.columns.Add(new Column { bindingPath = "PropertyName", title = "Property", stretchable = true });
            multiColumnListView.columns.Add(
                new Column
                {
                    bindingPath = "Show".GetBackingFieldPath(),
                    title = "Is visible",
                    stretchable = false,
                    resizable = false,
                    width = new Length(80, LengthUnit.Pixel),
                    makeCell = () =>
                    {
                        ToolbarToggle toggle = new ToolbarToggle()
                        {
                            label = "Enable",
                            bindingPath = "Show".GetBackingFieldPath(),
                        };

                        return toggle;
                    }
                });
            multiColumnListView.columns.Add(
                new Column
                {
                    bindingPath = "Title".GetBackingFieldPath(),
                    title = "Title",
                    stretchable = false,
                    resizable = false,
                    width = new Length(80, LengthUnit.Pixel),
                    makeCell = () =>
                    {
                        Label label = new Label()
                        {
                            bindingPath = "Title".GetBackingFieldPath(),
                            style =
                            {
                                unityTextAlign = TextAnchor.MiddleCenter
                            }
                        };
                        label.SetEnabled(false);
                        return label;
                    }
                });
            multiColumnListView.columns.Add(new Column
            {
                bindingPath = "Width".GetBackingFieldPath(),
                title = "Width",
                stretchable = false,
                resizable = false,
                width = new Length(80, LengthUnit.Pixel)
            });
            multiColumnListView.columns.Add(new Column
            {
                bindingPath = "ValueUIPrefab".GetBackingFieldPath(),
                title = "Prefab",
                stretchable = false,
                resizable = false,
                width = new Length(100, LengthUnit.Percent)
            });

            return multiColumnListView;
        }
    }
}