using System;
using System.Collections.Generic;

namespace Helteix.Tools.UI
{
    public interface IUIListSource<out T>
    {
        public event Action<T> ItemAdded;
        public event Action<T> ItemRemoved;

        public IEnumerable<T> Items { get; }
    }
}