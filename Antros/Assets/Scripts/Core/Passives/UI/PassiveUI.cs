using ATCG.Elements;
using ATCG.Passives.Datas;
using Helteix.Tools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Passives.UI
{
    public class PassiveUI : UIItem<PassiveData>
    {
        [SerializeField]
        private Image icon;
        [SerializeField]
        private Image background;

        protected override void SyncUI(PassiveData current)
        {
            if(icon)
                icon.sprite = current.Icon;
            if (background)
                background.color = current.Color;
        }

        protected override void ClearUI()
        {
            if (icon)
                icon.sprite = null;
        }
    }
}