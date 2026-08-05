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
            ListenForPlayerCommand(address, address, callbacks);
        }

        public static void ListenForPlayerCommand<T>(this EntityAddress address,
            CLCKey key,
            params CommandListenerComponent<T>.Callback[] callbacks) where T : IPlayerCommand
        {
            ListenForCommand(address, callbacks, 
                (in CommandContext context, in T command) => 
                {
                    if (!address.TryGetComponentRO<BelongsToPlayerComponent>(out var componentRef)) 
                        return false;
                    
                    var battlePhase = context.battlePhase;
                    return command.GetPlayer(battlePhase) == componentRef.GetPlayer(battlePhase);
                }, 
                key);
        }
    } 
}