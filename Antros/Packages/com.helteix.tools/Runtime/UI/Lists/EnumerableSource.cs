using System;
using System.Collections.Generic;

namespace Helteix.Tools.UI
{
    public struct EnumerableSource<T> : IUIListSource<T>
    {

        event Action<T> IUIListSource<T>.ItemAdded
        {
            add {  }
            remove {  }
        }

        public event Action<T> ItemRemoved
        {
            add {  }
            remove {  }
        }

        public IEnumerable<T> Items { get; }

        public EnumerableSource(IEnumerable<T> items)
        {
            Items = items;
        }
    }
}