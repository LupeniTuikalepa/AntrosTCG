using ATCG.Passives.Datas;
using Helteix.Tools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Passives.UI
{
    public class DetailedPassiveUI : PassiveUI
    {
        [SerializeField]
        private TMP_Text label;
        [SerializeField]
        private TMP_Text description;

        protected override void SyncUI(PassiveData current)
        {
            if(label)
                label.text = current.Name;
            if(description)
                description.text = current.Description;
            base.SyncUI(current);
        }

        protected override void ClearUI()
        {
            if (label)
                label.text = string.Empty;
            if (description)
                description.text = string.Empty;
            base.ClearUI();
        }
    }
}