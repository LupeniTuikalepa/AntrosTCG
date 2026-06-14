using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    public readonly struct CommandContext: IDisposable
    {
        public readonly BattlePhase battlePhase;

        public BattleGrid Grid => battlePhase.BattleGrid;

        public World World => battlePhase.world;

        private readonly List<object> commandPlayers;
        private readonly Dictionary<IGameCommand, ICommandPlayerGroup> pairings;

        public CommandContext(BattlePhase battlePhase, List<object> commandPlayers)
        {
            pairings = DictionaryPool<IGameCommand, ICommandPlayerGroup>.Get();
            this.battlePhase = battlePhase;
            this.commandPlayers = commandPlayers;
        }

        public IBattlePlayer GetBattlePlayer(int playerID) => battlePhase.GetPlayer(playerID);

        /// <summary>
        /// Get the group of command players that will react for a specific command.
        /// </summary>
        /// <param name="gameCommand"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool TryGetCommandPlayerGroup(IGameCommand gameCommand, out ICommandPlayerGroup group)
            => pairings.TryGetValue(gameCommand, out group);

        public void Register<T>(T command) where T : IGameCommand
        {
            CommandPlayerGroup<T> group = new(command);
            pairings[command]= group;

            foreach (object commandPlayer in commandPlayers)
            {
                if(commandPlayer is not ICommandPlayer<T> player)
                    continue;

                if (player.CanPlay(command))
                    group.Add(player);
            }
        }
        public static implicit operator World(CommandContext context) => context.World;

        public static implicit operator BattleGrid(CommandContext context) => context.Grid;

        public static implicit operator BattlePhase(CommandContext context) => context.battlePhase;

        void IDisposable.Dispose()
        {
            foreach (var value in pairings.Values)
                value.Dispose();

            DictionaryPool<IGameCommand, ICommandPlayerGroup>.Release(pairings);
        }
    }
}