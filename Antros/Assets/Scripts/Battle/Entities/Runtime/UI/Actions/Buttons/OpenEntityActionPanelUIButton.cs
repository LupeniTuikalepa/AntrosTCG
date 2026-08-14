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
	        var build = panel.Build();
	        gameObject.SetActive(build);
	        return build;
        }

        protected override void OnClick(BaseEventData baseEventData)
        {
            Controller.Open(panel);
        }
    }
}