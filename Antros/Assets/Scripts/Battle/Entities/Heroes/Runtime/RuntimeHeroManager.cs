using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Runtime;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Entities.Heroes.Runtime
{
    public class RuntimeHeroManager : MonoBehaviour,
        IRuntimeBattlePlayerComponent<LocalBattlePlayer>,
        IPhaseListener<SelectCellPhase>
    {
        [field: SerializeField]

        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }

        [SerializeField]
        private Transform heroContainer;

        private readonly Dictionary<HeroBattleCard, RuntimeHero> heroCards = new();

        [ShowInInspector, ReadOnly]
        public RuntimeHero SelectedCard { get; private set; }

        [ShowInInspector, ReadOnly]
        public Condition Selectable { get; private set; }

        private void Awake()
        {
            this.Register();

            Selectable = new Condition();
            Selectable.AddOnValueChangeCallback(ctx =>
            {
                if (SelectedCard && !ctx)
                    SelectedCard.UnSelect();
            });
        }


        void IPhaseListener<SelectCellPhase>.OnPhaseBegin(SelectCellPhase phase)
        {
            Selectable.AddCondition(phase.MainChannelKey, false);
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseEnd(SelectCellPhase phase)
        {
            Selectable.RemoveCondition(phase.MainChannelKey);
        }

        public void Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            /*
            BattleGrid battleGrid = RuntimeBattleGrid.BattleGrid;
            battleGrid.OnBattleCardDeployed += OnCardDeployed;
            */
        }

        public void Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            /*
            BattleGrid battleGrid = RuntimeBattleGrid.BattleGrid;
            battleGrid.OnBattleCardDeployed -= OnCardDeployed;
            */
        }

        private void OnCardDeployed(IBattleCard card)
        {
            switch (card)
            {
                case HeroBattleCard heroBattleCard:
                    if (heroCards.ContainsKey(heroBattleCard))
                        break;

                    GameObject instance = GameAssets.Current.HeroPawnPrefab.InstantiatePrefab(heroContainer);
                    if (instance.TryGetComponent(out RuntimeHero runtimeHeroBattleCard))
                    {
                        runtimeHeroBattleCard.Initialize(this);
                        //runtimeHeroBattleCard.Connect(HeroCard);

                        heroCards.Add(heroBattleCard, runtimeHeroBattleCard);
                    }

                    break;
            }
        }

        public void Select(RuntimeHero runtimeHero)
        {
            if (!Selectable) return;

            Unselect();

            SelectedCard = runtimeHero;
            SelectedCard.OnSelected();
        }

        public void Unselect()
        {
            if (SelectedCard)
                SelectedCard.OnDeselected();

            SelectedCard = null;
        }
    }
}