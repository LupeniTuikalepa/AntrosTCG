using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Helteix.Tools.UI
{

    public abstract class UIList<T, TUI> : MonoBehaviour
        where TUI : UIItem<T>
    {
        public event Action<IUIListSource<T>> OnSourceConnected;
        public event Action<IUIListSource<T>> OnSourceDisconnected;
        public event Action<T, TUI> OnItemAdded;
        public event Action<T, TUI> OnItemRemoved;

        [SerializeField]
        private TUI prefab;

        [SerializeField]
        private GameObject dividerPrefab;

        [SerializeField]
        private Transform root;

        protected IUIListSource<T> CurrentSource { get; private set; }

        private Dictionary<T, TUI> items;
        private Dictionary<TUI, Transform> dividers;

        public IReadOnlyCollection<TUI> UIItems
        {
            get
            {
                if (items == null)
                    return Array.Empty<TUI>();

                return items.Values;
            }
        }

        private void Awake()
        {
            items = new Dictionary<T, TUI>();
            dividers = new Dictionary<TUI, Transform>();

            Clear();

            CustomAwake();
        }

        private void OnDestroy()
        {
            Disconnect();
            CustomOnDestroy();
        }

        protected virtual void CustomAwake() { }
        protected virtual void CustomOnDestroy() { }


        public void Connect(IEnumerable<T> source) => Connect(new EnumerableSource<T>(source));

        public virtual void Connect(IUIListSource<T> listSource)
        {
            if(CurrentSource != null)
                Disconnect();

            CurrentSource = listSource;
            CurrentSource.ItemAdded += CurrentSourceOnItemAdded;
            CurrentSource.ItemRemoved += CurrentSourceOnItemRemoved;

            foreach (var existing in listSource.Items)
                CurrentSourceOnItemAdded(existing);

            OnSourceConnected?.Invoke(listSource);
        }

        public virtual void Disconnect()
        {
            if(CurrentSource == null)
                return;

            Clear();

            CurrentSource.ItemAdded -= CurrentSourceOnItemAdded;
            CurrentSource.ItemRemoved -= CurrentSourceOnItemRemoved;
            OnSourceDisconnected?.Invoke(CurrentSource);

            CurrentSource = null;
        }

        protected virtual void CurrentSourceOnItemAdded(T item)
        {
            if (TryGetUIFor(item, out TUI ui))
            {
                ui.Disconnect();
                ui.Connect(item);
                return;
            }
            TUI p = GetPrefabFor(item);
            Transform r = GetRootFor(item);
            TUI instance = InstantiatePrefabFor(p, r);
            items.Add(item, instance);

            if(items.Count > 1 && dividerPrefab)
            {
                var divider = Instantiate(dividerPrefab, r);
                int targetIndex = instance.transform.GetSiblingIndex();
                divider.transform.SetSiblingIndex(targetIndex);
                dividers.Add(instance, divider.transform);
            }

            instance.Connect(item);
            OnItemAdded?.Invoke(item, instance);
        }

        protected virtual void CurrentSourceOnItemRemoved(T item)
        {
            if (!items.Remove(item, out TUI ui))
                return;

            ui.Disconnect();
            OnItemRemoved?.Invoke(item, ui);

            if (dividers.Remove(ui, out Transform value))
                Destroy(value.gameObject);

            DestroyInstance(ui);
        }

        protected virtual TUI GetPrefabFor(T item) => prefab;

        protected virtual void Clear()
        {
            using (ListPool<T>.Get(out var list))
            {
                list.AddRange(items.Keys);
                foreach(var key in list)
                    CurrentSourceOnItemRemoved(key);
            }

            (root ? root : transform).ClearChildren();
            items.Clear();
        }
        protected virtual Transform GetRootFor(T item) => root ? root : transform;
        protected virtual TUI InstantiatePrefabFor(TUI p, Transform t) => p.InstantiatePrefab(t);

        protected virtual void DestroyInstance(TUI p) => p.DestroyGameObject();
        public bool TryGetUIFor(T item, out TUI ui) => items.TryGetValue(item, out ui);

    }
}