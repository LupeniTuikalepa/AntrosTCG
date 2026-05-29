using UnityEngine;

namespace Helteix.Cards.UI.Physical.Drag
{
    public struct DragResult<TCard> where TCard : ICard
    {
        public ICardDropTarget<TCard> Target { get; internal set; }
        public bool Accepted { get; internal set; }
        public Vector3 ScreenPosition { get; internal set; }
    }
}