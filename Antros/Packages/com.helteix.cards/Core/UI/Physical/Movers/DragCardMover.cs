using System;
using Helteix.Cards.UI.Utility;
using UnityEngine;

namespace Helteix.Cards.UI.Physical.Movers
{
    [Serializable]
    public class DragCardMover : ICardUIMover
    {
        public int Priority { get; internal set; } = 100;
        public RectTransform Container { get; internal set; }
        public RectTransform DraggedTarget { get; internal set; }

        [SerializeField]
        private float damping;

        [SerializeField, Min(0)]
        private float scalingFactor = .6f;
        [SerializeField, Min(0)]
        private float speedRotationStrength = 35f;

        
        public DragCardMover(float damping)
        {
            this.damping = damping;
        }


        public void MoveCard(CardHolderUI holderUI, ICardUI cardUI)
        {
            if(DraggedTarget == null)
                return;

            Transform cardUITransform = cardUI.transform;
            float delta = damping * Time.deltaTime;

            Vector3 position = DraggedTarget.position;

            Debug.DrawLine(cardUI.transform.position, position, Color.red);
            Vector3 direction = position - cardUITransform.position;
            Vector3 axis = Vector3.Cross(direction, -holderUI.transform.forward);

            float directionMagnitude = Mathf.Clamp(direction.magnitude, 0, 100);
            Quaternion speedRotation = Quaternion.AngleAxis(directionMagnitude * Time.deltaTime * speedRotationStrength, axis);

            cardUITransform.position = Vector3.Lerp(cardUITransform.position, position, delta);
            cardUITransform.rotation = Quaternion.Lerp(cardUITransform.rotation, DraggedTarget.rotation * speedRotation, delta);
            cardUITransform.localScale = Vector3.MoveTowards(cardUITransform.localScale, holderUI.RectTransform.localScale * scalingFactor, delta);

            //PhysicalCardUtilities.ConstraintRectInsideAnother(DraggedTarget, Container);
        }
    }
}