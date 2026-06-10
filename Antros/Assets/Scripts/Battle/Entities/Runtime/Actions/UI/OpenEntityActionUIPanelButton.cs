using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public class OpenEntityActionUIPanelButton : EntityActionUIButton
    {
        [SerializeField]
        private EntityActionUIPanel panel;

        protected override bool Build()
        {
            panel.Build();
            return !panel.IsEmpty();
        }

        protected override void OnClick(BaseEventData baseEventData)
        {
            Controller.Open(panel);
        }
    }
}