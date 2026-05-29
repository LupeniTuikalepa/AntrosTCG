using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace ATCG
{
    [Serializable]
    public struct Columns<T> : ISerializationCallbackReceiver
    {
        #region Editor
        #if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly Dictionary<string, IProperty> ColumnsProperties;

        static Columns()
        {
            IPropertyBag<T> bag = PropertyBag.GetPropertyBag<T>();
            ColumnsProperties = new Dictionary<string, IProperty>();

            foreach (IProperty<T> property in bag.GetProperties())
            {
                if (property.HasAttribute<MultiColumnPropertyAttribute>())
                    ColumnsProperties.TryAdd(property.Name, property);
            }
        }

        private void RebuildColumns()
        {
            Column[] newColumns = new Column[ColumnsProperties.Count];
            int index = 0;
            foreach ((string key, IProperty property) in ColumnsProperties)
            {
                Column column = null;

                if (columns != null)
                {
                    for (int j = 0; j < columns.Length; j++)
                    {
                        if (columns[j].PropertyName == key)
                        {
                            column = columns[j];
                            break;
                        }
                    }
                }

                column ??= new Column()
                {
                    PropertyName = property.Name,
                    Show = true,
                    Width = -1,
                };

                MultiColumnPropertyAttribute attribute = property.GetAttribute<MultiColumnPropertyAttribute>();
                string title = string.IsNullOrEmpty(attribute.columnName) ? property.Name : attribute.columnName;
                column.Title = title;

                var prefab = column.ValueUIPrefab;
                if (prefab != null)
                {
                    Type valueType = property.DeclaredValueType();
                    Type componentType = typeof(IMultiColumnRowValueUI<>).MakeGenericType(valueType);

                    if (!prefab.GetComponent(componentType))
                    {
                        Debug.LogWarning(
                            $"No component inheriting from MultiColumnRowValueUI<{valueType.Name}> found on the prefab",
                            prefab);
                        column.ValueUIPrefab = null;
                    }
                }

                newColumns[index++] = column;
            }

            columns = newColumns;
        }




        #endif
        #endregion
        [SerializeField]
        private Column[] columns;

        internal Column this[int index] => columns[index];

        internal int Length => columns.Length;


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            RebuildColumns();
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {

        }
    }
}