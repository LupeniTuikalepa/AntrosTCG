using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public class OpenEntityActionPanelUIButton : EntityActionUIButton
    {
        [SerializeField]
        private EntityActionUIPanel panel;

        public override bool Build()
        {
            return panel.Build();
        }

        protected override void OnClick(BaseEventData baseEventData)
        {
            Controller.Open(panel);
        }

    }
}