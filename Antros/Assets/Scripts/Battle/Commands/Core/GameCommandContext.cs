using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    public readonly struct GameCommandContext: IDisposable
    {
        public readonly BattlePhase battlePhase;

        public BattleGrid Grid => battlePhase.BattleGrid;

        public World World => battlePhase.world;

        private readonly List<object> commandPlayers;
        private readonly Dictionary<GameCommand, ICommandPlayerContext> pairings;

        public GameCommandContext(BattlePhase battlePhase, List<object> commandPlayers)
        {
            pairings = DictionaryPool<GameCommand, ICommandPlayerContext>.Get();
            this.battlePhase = battlePhase;
            this.commandPlayers = commandPlayers;
        }

        public IBattlePlayer GetBattlePlayer(int playerID) => battlePhase.GetPlayer(playerID);
        public bool TryGetCommandPlayerContext(GameCommand gameCommand, out ICommandPlayerContext commandPlayerContext)
            => pairings.TryGetValue(gameCommand, out commandPlayerContext);

        public void Register<T>(T command) where T : GameCommand
        {
            CommandPlayerContext<T> playerContext = new(command);
            pairings[command]= playerContext;

            foreach (object commandPlayer in commandPlayers)
            {
                if (commandPlayer is ICommandPlayer<T> player && player.CanPlay(command))
                    playerContext.Add(player);
            }
        }
        public static implicit operator World(GameCommandContext context) => context.World;

        public static implicit operator BattleGrid(GameCommandContext context) => context.Grid;

        public static implicit operator BattlePhase(GameCommandContext context) => context.battlePhase;

        void IDisposable.Dispose()
        {
            foreach (var value in pairings.Values)
                value.Dispose();

            DictionaryPool<GameCommand, ICommandPlayerContext>.Release(pairings);
        }
    }
}