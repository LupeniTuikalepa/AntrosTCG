using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public class EntityActionUIPanel : EntityActionUIElement
    {
        private List<EntityActionUIButton> panelButtons;

        [SerializeField]
        private CanvasGroup canvasGroup;


        protected override void Awake()
        {
            base.Awake();
            panelButtons = new();
        }

        private void Start()
        {
            OnClose();
            CollectButtons();
        }

        protected void CollectButtons()
        {
            EntityActionUIButton[] buttons = GetComponentsInChildren<EntityActionUIButton>();
            panelButtons.Clear();
            for (int i = 0; i < buttons.Length; i++)
            {
                EntityActionUIButton button = buttons[i];
                panelButtons.Add(button);
            }
        }
        public virtual bool IsEmpty() => panelButtons == null
                                         || panelButtons.Count == 0
                                         || panelButtons.TrueForAll(ctx => !ctx.IsActive);

        public virtual void Build()
        {
            foreach (EntityActionUIButton button in panelButtons)
            {
                button.BuildButton();
            }
        }

        public virtual async Awaitable OnOpen()
        {
            canvasGroup.blocksRaycasts = true;
            Tween.StopAll(canvasGroup);
            await Tween.Alpha(canvasGroup, 1, .15f, Ease.OutExpo);
        }

        public virtual async Awaitable OnClose()
        {
            canvasGroup.blocksRaycasts = false;
            Tween.StopAll(canvasGroup);
            await Tween.Alpha(canvasGroup, 0, .15f, Ease.OutExpo);
        }

        public void CloseLast() => Controller.CloseLast();
    }
}