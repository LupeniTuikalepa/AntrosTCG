using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.GameModes;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    public class GameCommandManager
    {
        private readonly BattlePhase battlePhase;

        private List<object> commandsPlayers;

        public GameCommandManager(BattlePhase battlePhase)
        {
            this.battlePhase = battlePhase;
        }

        public void ExecuteGameCommandAndForget<T>(T gameCommand) where T: GameCommand
        {
            _ = ExecuteGameCommand(gameCommand);
        }

        public void Register<T>(ICommandPlayer<T> player) where T : GameCommand
        {
            commandsPlayers.Add(player);
        }

        public void Unregister<T>(ICommandPlayer<T> player) where T : GameCommand
        {
            commandsPlayers.Remove(player);
        }

        public async Awaitable ExecuteGameCommand<T>(T gameCommand) where T: GameCommand
        {
            using GameCommandContext context = new(battlePhase, commandsPlayers);

            context.Register(gameCommand);
            gameCommand.Process(in context);

            await PlayCommandEffects(context, gameCommand);
        }

        private async Awaitable PlayCommandEffects(GameCommandContext context, GameCommand gameCommand)
        {
            if (!context.TryGetCommandPlayerContext(gameCommand, out ICommandPlayerContext player))
                return;

            Awaitable playable = player.Initiate(context);

            foreach (GameCommand embed in gameCommand.Embeds)
            {
                await player.WaitBeforePlayingEmbed(embed);

                await PlayCommandEffects(context, embed);

                await player.WaitBeforePlayingEmbed(embed);
            }

            await playable;
        }
    }
}