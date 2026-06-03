using System;
using System.Collections.Generic;
using System.Threading;
using Helteix.Cards.UI.Physical.Movers;
using Helteix.Cards.UI.Utility;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Helteix.Cards.UI.Physical.Drag
{
    public class CardDragPhase<TCard> : Phase<DragResult<TCard>> where TCard : ICard
    {
        private struct DragRaycast
        {
            public ICardDropTarget<TCard> target;
            public float depth;
        }

        public struct Setup
        {
            public CardUI<TCard> cardUI;
            public EventSystem eventSystem;
            public Camera camera;
            public RectTransform draggingParent;
            public bool is3D;
        }

        private const float RAY_RADIUS = .25f;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly RaycastHit[] Hits = new RaycastHit[32];

        public event Action<CardDragPhase<TCard>> OnUpdated;
        public event Action<CardDragPhase<TCard>> OnDropped;

        public CardHolderUI HolderUI => cardUI.HolderUI;
        public Camera Camera => camera == null ? Camera.main : camera;

        public readonly Camera camera;
        public readonly CardUI<TCard> cardUI;
        public readonly EventSystem eventSystem;

        public ICardDropTarget<TCard> Current { get; private set; }

        public Vector3 ScreenPosition { get; internal set; }
        public Vector2 ScreenDelta { get; internal set; }

        public bool IsDragging { get; internal set; }
        public RectTransform DragTarget { get; private set; }
        public RectTransform DraggingParent { get; private set; }

        public bool Is3D { get; private set; }

        internal CardDragPhase(Setup setup)
        {
            DraggingParent = setup.draggingParent;
            Is3D = setup.is3D;
            cardUI = setup.cardUI;
            eventSystem = setup.eventSystem;
            camera = setup.camera;
        }


        protected override async Awaitable Initialize(CancellationToken token)
        {
            IsDragging = true;

            GameObject target = new GameObject($"DragTarget for {cardUI.name}", typeof(RectTransform));

            RectTransform draggingParent = DraggingParent;
            if (draggingParent == null)
                draggingParent = HolderUI.CardUI.CardCanvas.transform as RectTransform;

            target.transform.SetParent(draggingParent);
            DragTarget = target.GetComponent<RectTransform>();
            DragTarget.sizeDelta = Vector2.zero;
            DragTarget.localScale = Vector3.one;
            DragTarget.localRotation = Quaternion.identity;

            await base.Initialize(token);
        }

        protected override async Awaitable<DragResult<TCard>> Execute(CancellationToken token)
        {
            DragCardMover dragCardMover = new DragCardMover(25)
            {
                Container = DraggingParent,
                DraggedTarget = DragTarget,
            };
            cardUI.HolderUI.AddMover(dragCardMover);

            while (IsDragging)
            {
                token.ThrowIfCancellationRequested();

                //Handle other canvas
                if (DragTarget)
                {
                    switch (cardUI.CardCanvas.renderMode)
                    {
                        case RenderMode.ScreenSpaceOverlay:
                            DragTarget.position = GetConstraintScreenPosition();
                            break;
                        default:
                            RectTransformUtility.ScreenPointToWorldPointInRectangle(DragTarget.parent as RectTransform,
                                GetConstraintScreenPosition(), Camera, out Vector3 worldPosition);
                            DragTarget.position = worldPosition;
                            break;
                    }
                }

                ICardDropTarget<TCard> last = Current;

                FindNewTarget();

                if (Current != last)
                {
                    last?.OnCardExit(cardUI);
                    Current?.OnCardEnter(cardUI);
                }

                Current?.OnCardHover(cardUI);
                OnUpdated?.Invoke(this);

                await Awaitable.NextFrameAsync(token);
            }

            var dragResult = new DragResult<TCard>()
            {
                Accepted = true,
                ScreenPosition = GetConstraintScreenPosition(),
                Target = Current,
            };

            cardUI.CollectionUI.OnCardDrop(cardUI.HolderUI, dragResult);
            Current?.OnCardDrop(cardUI);

            cardUI.HolderUI.RemoveMover(dragCardMover);
            OnDropped?.Invoke(this);

            return dragResult;
        }

        protected override async Awaitable Dispose(CancellationToken token)
        {
            IsDragging = false;
            Object.Destroy(DragTarget.gameObject);

            await base.Dispose(token);
        }



        private void FindNewTarget()
        {
            Current = null;
            EventSystem system = eventSystem == null ? EventSystem.current : eventSystem;

            using (ListPool<DragRaycast>.Get(out List<DragRaycast> dragRaycasts))
            {
                using (ListPool<RaycastResult>.Get(out List<RaycastResult> uiResults))
                {
                    List<BaseRaycaster> raycasters = RaycasterManager.GetRaycasters();
                    PointerEventData pointerEventData = new PointerEventData(system)
                    {
                        position = GetConstraintScreenPosition(),
                        pointerDrag = cardUI.gameObject,
                    };

                    foreach (var raycaster in raycasters)
                    {
                        if (raycaster.eventCamera == camera)
                            raycaster.Raycast(pointerEventData, uiResults);
                    }

                    foreach (RaycastResult result in uiResults)
                    {
#if HELTEIX_CARD_DEBUG
                        Debug.Log($"Drag UI raycast on {result.gameObject.name}", result.gameObject);
#endif
                        if (!result.gameObject.TryGetComponent(out ICardDropTarget<TCard> dropTarget))
                            continue;
#if HELTEIX_CARD_DEBUG
                        Debug.Log($"{result.gameObject.name} is compatible", result.gameObject);
#endif
                        dragRaycasts.Add(new DragRaycast()
                        {
                            target = dropTarget,
                            depth = result.depth
                        });
                    }

                    dragRaycasts.Sort((a, b) => a.depth.CompareTo(b.depth));
                    ProcessResults(dragRaycasts);

                    if (Current != null || uiResults.Count != 0)
                        return;
                }
/*
                if (system.IsPointerOverGameObject())
                {
#if HELTEIX_CARD_DEBUG
                    Debug.Log("Drag UI raycast on UI");
#endif
                    return;
                }*/

                dragRaycasts.Clear();
                if (Is3D)
                {
                    Ray ray = Camera.ScreenPointToRay(GetConstraintScreenPosition());
                    int count = Physics.SphereCastNonAlloc(ray.origin, RAY_RADIUS, ray.direction, Hits);
                    for (int i = 0; i < count; i++)
                    {
                        RaycastHit hit = Hits[i];
#if HELTEIX_CARD_DEBUG
                        Debug.Log($"Drag physics raycast on {hit.collider.name}", hit.collider);
#endif
                        if (hit.collider.TryGetComponent(out ICardDropTarget<TCard> dropTarget))
                        {

#if HELTEIX_CARD_DEBUG
                        Debug.Log($"{hit.collider.name} is compatible", hit.collider);
#endif
                            dragRaycasts.Add(new DragRaycast()
                            {
                                target = dropTarget,
                                depth = hit.distance
                            });
                        }
                    }
                }
                else
                {
                    Vector3 worldPosition = Camera.ScreenToWorldPoint(GetConstraintScreenPosition());
                    using (ListPool<Collider2D>.Get(out List<Collider2D> colliders))
                    {
                        int count = Physics2D.OverlapCircle(worldPosition, RAY_RADIUS, ContactFilter2D.noFilter, colliders);
                        for (int i = 0; i < count; i++)
                        {
                            Collider2D collider2D = colliders[i];
                            if (collider2D.TryGetComponent(out ICardDropTarget<TCard> dropTarget))
                            {
#if HELTEIX_CARD_DEBUG
                                Debug.Log($"{collider2D.gameObject.name} is compatible", collider2D.gameObject);
#endif
                                dragRaycasts.Add(new DragRaycast()
                                {
                                    target = dropTarget,
                                    depth = collider2D.transform.position.z,
                                });
                            }
                        }
                    }
                }

                ProcessResults(dragRaycasts);

            }
        }

        private void ProcessResults(List<DragRaycast> dragRaycasts)
        {
            if (dragRaycasts.Count == 0)
                return;

            int lastPriority = -1;
            foreach (DragRaycast raycast in dragRaycasts)
            {
                ICardDropTarget<TCard> target = raycast.target;
                if (lastPriority < target.Priority && target.Accepts(cardUI))
                    Current = target;
            }
        }

        public Vector3 GetConstraintScreenPosition()
        {
            Vector3 afterConstraint = PhysicalCardUtilities.ConstraintScreenPosInsideRect(ScreenPosition,
                DragTarget.parent as RectTransform,
                cardUI.CardCanvas);

            return afterConstraint;
        }

    }
}