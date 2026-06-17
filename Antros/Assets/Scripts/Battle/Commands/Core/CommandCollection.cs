using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    [Serializable]
    public class CommandCollection : IDisposable, ISerializationCallbackReceiver
    {
        [SerializeReference]
        private List<ICommand> commands;

        [field: SerializeField]
        public BattleID RootID { get; private set; }

        public ICommand Root => GetCommand(RootID);

        private Dictionary<BattleID, ICommand> mapping;

        public CommandCollection(ICommand root) : this()
        {
            RootID = root.ID;
        }

        public CommandCollection()
        {
            mapping = DictionaryPool<BattleID, ICommand>.Get();
            commands = ListPool<ICommand>.Get();
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
            foreach ((BattleID battleID, ICommand command) in mapping)
                commands.Add(command);

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            mapping ??= DictionaryPool<BattleID, ICommand>.Get();

            mapping.Clear();
            foreach (var command in commands)
                mapping.Add(command.ID, command);
        }

        void IDisposable.Dispose()
        {
            DictionaryPool<BattleID, ICommand>.Release(mapping);
            ListPool<ICommand>.Release(commands);
        }
    }
}