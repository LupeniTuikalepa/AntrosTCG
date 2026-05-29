using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.GameModes;
using Helteix.Singletons.MonoSingletons;
using Helteix.Singletons.MonoSingletons.Attributes;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    [DontDestroyOnLoad]
    public class GameCommandManager : MonoSingleton<GameCommandManager>
    {
        private List<object> commandsPlayers = new List<object>();

        public void ExecuteGameCommandAndForget<T>(T gameCommand, BattlePhase battlePhase) where T: GameCommand
        {
            _ = ExecuteGameCommand(gameCommand, battlePhase);
        }

        public async Awaitable ExecuteGameCommand<T>(T gameCommand, BattlePhase battlePhase) where T: IGameCommand
        {
            using GameCommandContext context = new(battlePhase, commandsPlayers);

            context.Register(gameCommand);
            gameCommand.Process(in context);

            await PlayCommandEffects(context, gameCommand);
        }
        public void Register<T>(ICommandPlayer<T> player) where T : GameCommand
        {
            commandsPlayers.Add(player);
        }

        public void Unregister<T>(ICommandPlayer<T> player) where T : GameCommand
        {
            commandsPlayers.Remove(player);
        }


        private async Awaitable PlayCommandEffects(GameCommandContext context, IGameCommand gameCommand)
        {
            if (!context.TryGetCommandPlayerContext(gameCommand, out ICommandPlayerContext player))
                return;

            Awaitable playable = player.Initiate(context);

            foreach (GameCommand embed in gameCommand.Embeds)
            {
                await player.WaitBeforePlayingEmbed(embed);

                await PlayCommandEffects(context, embed);

                await player.WaitAfterPlayingEmbed(embed);
            }

            await playable;
        }
    }
}