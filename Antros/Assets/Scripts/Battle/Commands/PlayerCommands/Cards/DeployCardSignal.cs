using System;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands
{
    [Serializable]
    public class DeployCardSignal : PlayerCommandSignal<NoInfos>
    {
        [field: SerializeField]
        public BattleID CardId { get; private set; }

        [field: SerializeField]
        public HexCoordinates Destination { get; private set; }


        public DeployCardSignal(BattleID cardId, HexCoordinates destination, IBattlePlayer battlePlayer) : base(battlePlayer)
        {
            CardId = cardId;
            Destination = destination;
        }
    }
}