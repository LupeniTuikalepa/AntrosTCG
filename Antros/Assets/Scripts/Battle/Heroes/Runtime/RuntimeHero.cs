using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Metrics;
using ATCG.HexGrids;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Heroes.Runtime
{
    public class RuntimeHero : MonoBehaviour
    {
        public Action<RuntimeHero> OnSelected;
        public Action<RuntimeHero> OnDeselected;

        [SerializeField]
        private TMP_Text heroName;

        [SerializeField]
        private SpriteRenderer outline;
        public HeroBattleCard Hero { get; private set; }

        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }

        public void Initialize(RuntimeBattleGrid battleGrid)
        {
            RuntimeBattleGrid = battleGrid;
        }

        public void Connect(HeroBattleCard card)
        {
            if(Hero != null)
                Disconnect();

            Hero = card;
            heroName.text = card.Title;

            card.OnCardMoved += OnCardMoved;

            if (RuntimeBattleGrid.TryGetBattleCellAt(card.Coordinates, out var cell))
            {
                transform.localScale = cell.GetTargetScale();
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            GameplayMetrics metrics = GameplayMetrics.Current;
            Color playerColor = metrics.GetPlayerColor(card.playerID, RuntimeBattleGrid.CurrentGameMode.PlayerCount);

            outline.color = playerColor;
        }


        public void Disconnect()
        {

        }

        private void OnCardMoved(HexCoordinates from, HexCoordinates to) => GoTo(to);

        private void GoTo(HexCoordinates coordinates)
        {
            if (RuntimeBattleGrid.TryGetBattleCellAt(coordinates, out var cell))
            {
                transform.localScale = cell.GetTargetScale();
                Tween.Position(transform, cell.transform.position, .25f, Ease.OutQuad);
            }
        }
    }
}