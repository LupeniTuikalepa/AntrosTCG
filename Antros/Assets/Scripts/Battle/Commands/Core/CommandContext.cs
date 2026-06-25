using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ATCG.Battle.Commands.Core
{
    public readonly struct CommandContext: IDisposable
    {
        public readonly BattlePhase battlePhase;

        public BattleGrid Grid => battlePhase.BattleGrid;

        public World World => battlePhase.world;

        public readonly BattleID groupID;
        private readonly List<ICommandListener> commandListener;
        private readonly Dictionary<ICommand, ICommandListenerGroup> pairings;
        private readonly CommandCollection commandCollection;


        public CommandContext(BattlePhase battlePhase, List<ICommandListener> commandListener, CommandCollection commandCollection, BattleID groupID)
        {
            pairings = DictionaryPool<ICommand, ICommandListenerGroup>.Get();
            this.battlePhase = battlePhase;
            this.commandListener = commandListener;
            this.commandCollection = commandCollection;
            this.groupID = groupID;
        }

        public bool TryGetBattlePlayer(BattleID playerID, out IBattlePlayer player)
        {
            player = GetBattlePlayer(playerID);
            return player != null;
        }

        public IBattlePlayer GetBattlePlayer(BattleID playerID) => battlePhase.GetPlayer(playerID);


        public bool TryGetCommand(BattleID battleID, out ICommand command)
        {
            return commandCollection.TryGetCommand(battleID, out command);
        }

        public ICommand GetCommand(BattleID battleID) => commandCollection.GetCommand(battleID);

        public bool TryGetGroup<T>(T gameCommand, out CommandListenerGroup<T> group) where T : ICommand
        {
            if (pairings.TryGetValue(gameCommand, out var g) && g is CommandListenerGroup<T> cpg)
            {
                group = cpg;
                return true;
            }

            group = null;
            return false;
        }

        /// <summary>
        /// Get the group of command players that will react for a specific command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool TryGetGroup(ICommand command, out ICommandListenerGroup group)
            => pairings.TryGetValue(command, out group);

        public void Register<T>(T command) where T : ICommand
        {
            CommandListenerGroup<T> group = new(command);
            pairings[command]= group;
            commandCollection.AddCommand(command);

            foreach (ICommandListener commandPlayer in commandListener)
            {
                if(commandPlayer is not ICommandListener<T> player)
                    continue;

                if (player.CanPlay(command))
                {
                    group.Add(player);
                }
            }
        }


        public static implicit operator World(CommandContext context) => context.World;

        public static implicit operator BattleGrid(CommandContext context) => context.Grid;

        public static implicit operator BattlePhase(CommandContext context) => context.battlePhase;

        void IDisposable.Dispose()
        {
            foreach (var value in pairings.Values)
                value.Dispose();

            DictionaryPool<ICommand, ICommandListenerGroup>.Release(pairings);
        }

        public ICommand GetRoot() => commandCollection.Root;
        public bool IsRoot(ICommand command) => commandCollection.RootID == command.ID;
    }
}