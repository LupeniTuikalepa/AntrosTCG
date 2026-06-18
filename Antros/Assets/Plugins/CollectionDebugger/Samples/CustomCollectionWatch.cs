using CollectionDebugger.Core;
using UnityEngine;

public class CustomCollectionWatch : CollectionWatchBase
{
    public CustomCollectionWatch(string label, Transform[] array) : base(label)
    {
        customArray = array;
    }

    private readonly Transform[] customArray;
    protected override int GetCount() => customArray.Length;

    protected override void FillEntries(WatchEntry[] entries)
    {
        for (int i = 0; i < GetCount(); i++)
        {
            entries[i] = new WatchEntry(i, customArray[i]);
        }
    }
}