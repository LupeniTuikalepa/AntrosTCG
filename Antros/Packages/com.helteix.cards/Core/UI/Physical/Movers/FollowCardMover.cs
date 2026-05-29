using System;
using UnityEngine;

namespace Helteix.Cards.UI.Physical.Movers
{
    [Serializable]
    public class FollowCardMover : ICardUIMover
    {
        public virtual int Priority => 1;

        [Header("Base")]
        public float damping;

        [Header("Offsets")]
        public Vector2 positionOffset = Vector2.zero;
        public Quaternion rotationOffset = Quaternion.identity;
        public Vector2 scaleOffset = Vector2.zero;
        public Vector2 sizeOffset = Vector2.zero;

        [Header("Multipliers")]
        public float positionDampingMultiplier = 1;
        public float rotationDampingMultiplier = 1;
        public float scaleDampingMultiplier = 1;
        public float sizeDampingMultiplier = 1;

        public FollowCardMover(float damping)
        {
            this.damping = damping;
        }


        public virtual void MoveCard(CardHolderUI holderUI, ICardUI cardUI)
        {
            RectTransform cardUITransform = cardUI.RectTransform;
            RectTransform target = GetTargetTransform(holderUI);
            float delta = damping * Time.deltaTime;

            Move(holderUI, cardUITransform, target, delta);
            Rotate(holderUI, cardUITransform, target, delta);
            Scale(holderUI, cardUITransform, target, delta);

            Resize(holderUI, cardUITransform, target, delta);
        }

        private void Resize(CardHolderUI holderUI, RectTransform cardUITransform, RectTransform target,
            float delta)
        {
            float targetWidth = Mathf.Lerp(cardUITransform.rect.width + sizeOffset.x, target.rect.width, delta * sizeDampingMultiplier);
            float targetHeight = Mathf.Lerp(cardUITransform.rect.height + sizeOffset.y, target.rect.height, delta * sizeDampingMultiplier);

            cardUITransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            cardUITransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        }

        protected virtual void Scale(CardHolderUI holderUI, RectTransform cardUITransform, RectTransform target, float delta)
        {
            cardUITransform.localScale = Vector3.Lerp(cardUITransform.localScale, target.localScale + (Vector3)scaleOffset, delta * scaleDampingMultiplier);
        }

        protected virtual void Rotate(CardHolderUI holderUI, RectTransform cardUITransform, RectTransform target,
            float delta)
        {
            cardUITransform.rotation = Quaternion.Lerp(cardUITransform.rotation, target.rotation * rotationOffset, delta * rotationDampingMultiplier);
        }

        protected virtual void Move(CardHolderUI holderUI, RectTransform cardUITransform, RectTransform target,
            float delta)
        {
            cardUITransform.position = Vector3.Lerp(cardUITransform.position, target.position + (Vector3)positionOffset, delta * positionDampingMultiplier);
        }

        protected virtual RectTransform GetTargetTransform(CardHolderUI holderUI)
        {
            return holderUI.RectTransform;
        }
    }
}