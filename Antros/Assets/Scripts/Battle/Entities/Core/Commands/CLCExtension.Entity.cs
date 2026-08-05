using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;

namespace ATCG.Battle.Entities.Commands
{
    public static partial class CommandListenerComponentExtension
    {
        public static void ListenForEntityCommand<T>(this EntityAddress address,
            params CommandListenerComponent<T>.Callback[] callbacks) where T : IEntityCommand
        {
            ListenForEntityCommand(address, address, callbacks);
        }
        
        public static void ListenForEntityCommand<T>(this EntityAddress address,
            CLCKey key,
            params CommandListenerComponent<T>.Callback[] callbacks
            ) where T : IEntityCommand
        {

            ListenForCommand(address, callbacks, 
                (in CommandContext context, in T command) => 
                    command.TargetEntityAddress(context.World) == address,
                key);
        }
    }
}