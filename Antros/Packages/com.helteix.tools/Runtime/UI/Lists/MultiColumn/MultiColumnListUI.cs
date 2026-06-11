using System;
using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace ATCG
{
    public class MultiColumnListUI<T> : UIList<T, MultiColumnRowUI<T>>, ISerializationCallbackReceiver
    {
        [SerializeField, Space]
        private Transform headerRoot;
        [SerializeField]
        public MultiRowHeaderUI headerPrefab;

        [SerializeField]
        private Columns<T> columns;

        [SerializeField, HideInInspector]
        private MultiRowHeaderUI[] headers;



        internal bool TryGetColumnForKey(string propertyName, out Column column)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].PropertyName == propertyName)
                {
                    column = columns[i];
                    return true;
                }
            }

            column = null;
            return false;
        }

        public void ChangeColumnVisibility(string propertyName, bool visible)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].PropertyName == propertyName)
                {
                    columns[i].Show = visible;
                    RefreshColumnsVisibility();
                    return;
                }
            }
        }
        protected override MultiColumnRowUI<T> InstantiatePrefabFor(MultiColumnRowUI<T> p, Transform t)
        {
            MultiColumnRowUI<T> rowUI = base.InstantiatePrefabFor(p, t);
            rowUI.SetListUI(this);
            return rowUI;
        }


        public async Awaitable RefreshColumnsVisibilityEndOfFrame()
        {
            await Awaitable.EndOfFrameAsync();
            RefreshColumnsVisibility();
        }
        public void RefreshColumnsVisibility()
        {
            foreach (var item in UIItems)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    Column column = columns[i];
                    item.SetColumnVisibility(column.PropertyName, column.Show);
                    headers[i].SetVisibility(column.Show);
                }
            }

        }

        private void OnValidate()
        {
            _ = RebuildHeaders();
            _ = RefreshColumnsVisibilityEndOfFrame();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {

        }

        private async Awaitable RebuildHeaders()
        {
            await Awaitable.EndOfFrameAsync();
            headerRoot.ClearChildren();
            headers = new MultiRowHeaderUI[columns.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                MultiRowHeaderUI instance = headerPrefab.InstantiatePrefab(headerRoot);
                instance.Text.SetText(column.Title);

                LayoutElement layoutElement = instance.LayoutElement;
                layoutElement.flexibleWidth = 0;
                layoutElement.preferredWidth = column.Width;
                layoutElement.minWidth = column.Width;

                headers[i] = instance;
            }
        }
    }
}