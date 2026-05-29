using System;
using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG
{
    public abstract class MultiColumnRowUI<T> : UIItem<T>
    {
        private sealed class Visitor : PropertyVisitor
        {
            private readonly MultiColumnRowUI<T> rowUI;

            public Visitor(MultiColumnRowUI<T> rowUI)
            {
                this.rowUI = rowUI;
            }

            protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                var listUI = rowUI.listUI;

                if(!listUI.TryGetColumnForKey(property.Name, out var column))
                    return;

                GameObject instance = column.ValueUIPrefab.InstantiatePrefab(rowUI.GetRoot());
                if (instance.TryGetComponent(out IMultiColumnRowValueUI<TValue> valueUI))
                {
                    valueUI.SetValue(value);

                    LayoutElement layoutElement = valueUI.LayoutElement;
                    layoutElement.flexibleWidth = 0;
                    layoutElement.preferredWidth = column.Width;
                    layoutElement.minWidth = column.Width;

                    rowUI.columns.Add(property.Name, instance);
                }
            }
        }
        [SerializeField]
        private Transform root;

        private Visitor visitor;
        private MultiColumnListUI<T> listUI;

        private Dictionary<string, GameObject> columns;

        private void Awake()
        {
            columns = new Dictionary<string, GameObject>();
        }

        protected override void SyncUI(T current)
        {
            GetRoot().ClearChildren();
            columns.Clear();
            PropertyContainer.Accept(visitor, ref current);
        }


        protected override void ClearUI()
        {
            columns.Clear();
            GetRoot().ClearChildren();
        }

        public void SetColumnVisibility(string columnName, bool visibility)
        {
            if(columns.TryGetValue(columnName, out var column))
                column.SetActive(visibility);
        }

        private Transform GetRoot()
        {
            return (root ? root : transform);
        }
        public void SetListUI(MultiColumnListUI<T> newListUI)
        {
            listUI = newListUI;
            visitor = new Visitor(this);
        }

    }
}