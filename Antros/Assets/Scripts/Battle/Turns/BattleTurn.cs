using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities.Components;
using UnityEngine;

namespace ATCG.Battle.Turns
{
    [Serializable]
    public struct BattleTurn
    {
        [field: SerializeField]
        public string TurnID { get; private set; }

        [field: SerializeField]
        public BattleID PlayerID { get; private set; }

        [SerializeReference]
        private List<ICommand> commands;

        public BattleTurn(string turnID, BattleID playerID)
        {
            TurnID = turnID;
            PlayerID = playerID;
            commands = new List<ICommand>();
        }

        public void RegisterCommand(ICommand command)
        {
            commands.Add(command);
        }
    }
}