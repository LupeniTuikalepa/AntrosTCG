using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Exceptions;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.GameModes;
using Helteix.Singletons.MonoSingletons;
using Helteix.Singletons.MonoSingletons.Attributes;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    [DontDestroyOnLoad]
    public class GameCommandManager : MonoSingleton<GameCommandManager>
    {
        private List<object> commandsPlayers = new List<object>();

        public void ExecuteGameCommand<T>(T gameCommand, BattlePhase battlePhase) where T: IGameCommand
        {
            ExecuteGameCommandAsync(gameCommand, battlePhase).FireAndForget();
        }

        public async Awaitable ExecuteGameCommandAsync<T>(T gameCommand, BattlePhase battlePhase) where T: IGameCommand
        {
            using CommandContext context = new(battlePhase, commandsPlayers);

            context.Register(gameCommand);

            try
            {
                gameCommand.Process(in context);
            }
            catch (BreakCommandException breakCommandException)
            {
                Debug.Log(
@$"Game Command was canceled because of : {breakCommandException.Cause}");
            }

            CommandPlayerRunner<T> runner = new CommandPlayerRunner<T>(gameCommand);
            await runner.Run(context);
        }
        public void Register<T>(ICommandPlayer<T> player) where T : IGameCommand
        {
            commandsPlayers.Add(player);
        }

        public void Unregister<T>(ICommandPlayer<T> player) where T : IGameCommand
        {
            commandsPlayers.Remove(player);
        }



    }
}