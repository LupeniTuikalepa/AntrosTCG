using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands
{
    [System.Serializable]
    public class SpawnHeroCommand : Command<NoInfos>
    {
        [field: SerializeField]
        public BattleID PlayerID { get; private set; }

        [field: SerializeField]
        public BattleID CardID { get; private set; }
        [field: SerializeField]
        public HexCoordinates Destination { get; private set; }
        [field: SerializeField]
        public BattleID SpawnID { get; private set; }

        public SpawnHeroCommand(IBattlePlayer player, HeroBattleCard heroBattleCard, HexCoordinates destination)
        {
            PlayerID = player.GetBattleID();
            CardID = heroBattleCard.ID;
            Destination = destination;

            SpawnID = BattleID.CreateNew();
        }

        protected override void Process(in CommandContext context)
        {
            if(!context.TryGetBattlePlayer(PlayerID, out var player))
                return;

            if(!player.TryGetCard(CardID, out IBattleCard card))
                return;

            if(card is not HeroBattleCard heroBattleCard)
                return;

            if (!SpawnID.IsValid)
            {
                SpawnID = BattleID.CreateNew();
                Debug.LogWarning("Processing command without everything setup. It could lead to some divergence behaviour online.");
            }

            HeroEntityAspect hero = HeroEntityAspect.CreateAspect(context.World, new HeroEntityAspect.Setup()
            {
                battleID = SpawnID,
                card = heroBattleCard,
                coordinates = Destination,
                grid = context.Grid,
            });

            Embed(in context, new MoveCommand(hero.EntityAddress, Destination));
        }
    }
}