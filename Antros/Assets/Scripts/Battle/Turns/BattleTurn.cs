using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using UnityEngine;

namespace ATCG.Battle.Turns
{
    [Serializable]
    public struct BattleTurn
    {
        [field: SerializeField]
        public string TurnID { get; private set; }

        [field: SerializeField]
        public int PlayerID { get; private set; }

        [SerializeReference]
        private List<GameCommand> commands;

        public BattleTurn(string turnID, int playerID)
        {
            TurnID = turnID;
            PlayerID = playerID;
            commands = new List<GameCommand>();
        }

        public void RegisterCommand(GameCommand command)
        {
            commands.Add(command);
        }
    }
}