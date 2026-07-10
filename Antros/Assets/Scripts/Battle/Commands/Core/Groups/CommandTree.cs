using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Commands.Groups
{
    [Serializable]
    public class CommandTree : ISerializationCallbackReceiver
    {
        [SerializeReference]
        private List<ICommand> commands;

        [field: SerializeField]
        public BattleID RootID { get; private set; }

        public ICommand Root => GetCommand(RootID);

        private Dictionary<BattleID, ICommand> mapping;

        public CommandTree(ICommand root)
        {
            RootID = root.ID;
            mapping = new Dictionary<BattleID, ICommand>();
            commands = new List<ICommand>();
        }


        public void AddCommand(ICommand command)
        {
            commands.Add(command);
            mapping.Add(command.ID, command);
        }

        public bool TryGetCommand(BattleID battleID, out ICommand command)
        {
            return mapping.TryGetValue(battleID, out command);
        }

        public ICommand GetCommand(BattleID battleID)
        {
            return mapping.GetValueOrDefault(battleID);
        }


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            commands.Clear();
            foreach ((_, ICommand command) in mapping)
                commands.Add(command);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            mapping ??= new Dictionary<BattleID, ICommand>();
            mapping.Clear();
            foreach (var command in commands)
                mapping.Add(command.ID, command);
        }
    }
}