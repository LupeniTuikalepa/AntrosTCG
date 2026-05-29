using System;
using UnityEngine;

namespace Helteix.Tools.UI
{
    public abstract class UIItem<TItem> : MonoBehaviour
    {
        public TItem Current { get; internal set; }

        internal void Connect(TItem item)
        {
            Disconnect();
            Current = item;
            SyncUI(Current);
        }

        internal void Disconnect()
        {
            Current = default;
            ClearUI();
        }

        protected void OnDestroy()
        {
            Disconnect();
        }

        protected abstract void SyncUI(TItem current);
        protected abstract void ClearUI();
    }
}