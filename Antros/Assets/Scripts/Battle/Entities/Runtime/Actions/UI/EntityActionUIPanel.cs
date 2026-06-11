using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public class EntityActionUIPanel : EntityActionUIElement
    {
        private List<EntityActionUIElement> elements;

        [SerializeField]
        private CanvasGroup canvasGroup;

        protected override void Awake()
        {
            elements = new List<EntityActionUIElement>();
            base.Awake();
        }

        private void Start()
        {

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            CollectButtons();
        }

        protected void CollectButtons()
        {
            EntityActionUIElement[] buttons = GetComponentsInChildren<EntityActionUIElement>();
            elements.Clear();
            for (int i = 0; i < buttons.Length; i++)
            {
                EntityActionUIElement element = buttons[i];
                if(element != this)
                    elements.Add(element);
            }
        }
        public virtual bool IsEmpty() => elements == null
                                         || elements.Count == 0
                                         || elements.TrueForAll(ctx => !ctx.IsActive);

        public override bool Build()
        {
            bool hasActiveElement = false;
            foreach (var element in elements)
            {
                    hasActiveElement |= element.Build();
            }

            return hasActiveElement;
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