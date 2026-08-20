using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Groups;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands
{
    public readonly struct CommandContext: IDisposable
    {
        public readonly BattlePhase battlePhase;

        public BattleGrid Grid => battlePhase.BattleGrid;

        public World World => battlePhase.world;

        private readonly Dictionary<ICommand, ICommandDirectorGroup> pairings;
        private readonly CommandTree commandTree;
        private readonly BattleID groupID;


        public CommandContext(BattlePhase battlePhase, CommandTree commandTree, BattleID groupID)
        {
            pairings = DictionaryPool<ICommand, ICommandDirectorGroup>.Get();
            this.battlePhase = battlePhase;
            this.commandTree = commandTree;
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
            return commandTree.TryGetCommand(battleID, out command);
        }

        public ICommand GetCommand(BattleID battleID) => commandTree.GetCommand(battleID);

        public bool TryGetGroup<T>(T gameCommand, out CommandDirectorGroup<T> group) where T : ICommand
        {
            if (pairings.TryGetValue(gameCommand, out var g) && g is CommandDirectorGroup<T> cpg)
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
        public bool TryGetGroup(ICommand command, out ICommandDirectorGroup group)
            => pairings.TryGetValue(command, out group);

        public void Register<T>(CommandContext context, T command) where T : ICommand
        {
            CommandDirectorGroup<T> group = new(command);
            pairings[command]= group;
            commandTree.AddCommand(command);

            CommandTrace.ReportCommandRegistered(CommandManager.CurrentGroupID, command);
            CommandManager.TriggerWatchers(context, command);

            foreach (ICommandDirector commandPlayer in CommandManager.Listeners)
            {
                if(commandPlayer is not ICommandDirector<T> player)
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

            DictionaryPool<ICommand, ICommandDirectorGroup>.Release(pairings);
        }

        public ICommand GetRoot() => commandTree.Root;
        public bool IsRoot(ICommand command) => commandTree.RootID == command.ID;
    }
}