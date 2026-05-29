using System;
using System.Collections.Generic;
using ATCG;
using Unity.Properties;
using UnityEngine;

namespace Helteix.Tools.Samples.MultiColumn
{
    public class SampleMultiColumnListUI : MultiColumnListUI<SampleItem>
    {
        protected override void CustomAwake()
        {
            base.CustomAwake();
            List<SampleItem> items = new List<SampleItem>()
            {
                new SampleItem(50, "First", false),
                new SampleItem(999, "Second", true),
                new SampleItem(15, "Third", false),
                new SampleItem(37964, "Forth", true),
            };

            Connect(items);
        }
    }
}