using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Commands
{
    public static partial class CommandListenerComponentExtension
    {
        public static void ListenForPlayerCommand<T>(this EntityAddress address,
            params CommandListenerComponent<T>.Callback[] callbacks) where T : IPlayerCommand
        {
            ListenForPlayerCommand(address, true, callbacks);
        }
        
        public static void ListenForPlayerCommand<T>(this EntityAddress address,
            bool listenAlly,
            params CommandListenerComponent<T>.Callback[] callbacks) where T : IPlayerCommand
        {
            ListenForPlayerCommand(address, address, listenAlly, callbacks);
        }

        public static void ListenForPlayerCommand<T>(this EntityAddress address,
            CLCKey key,
            bool listenAlly,
            params CommandListenerComponent<T>.Callback[] callbacks) where T : IPlayerCommand
        {
            ListenForCommand(address, callbacks, 
                (in CommandContext context, in T command) => 
                {
                    var battlePhase = context.battlePhase;
                    var commandPlayer = command.GetPlayer(battlePhase);
                    var isAlly = address.IsAlly(commandPlayer);
                    
                    return listenAlly ? isAlly : !isAlly;
                }, 
                key);
        }
    } 
}