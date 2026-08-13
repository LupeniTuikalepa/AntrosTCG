using System.Collections.Generic;
using ATCG.Battle.Commands;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Commands
{
    public static partial class CommandListenerComponentExtension
    {
        public static void ListenForCommand<T>(this EntityAddress address,
            CLCKey key,
            params CommandListenerComponent<T>.Callback[] callbacks) where T : ICommand
        {
            ListenForCommand(address, callbacks, 
                (in CommandContext context, in T command) => true, 
                key);
        }
        
        private static void ListenForCommand<T>(this EntityAddress address, 
            IEnumerable<CommandListenerComponent<T>.Callback> callbacks,
            CommandListenerComponent<T>.Accept accept,
            CLCKey key) where T : ICommand
        {
            if (address.TryGetComponent<CommandListenerComponent<T>>(out var componentRef))
            {
                foreach (var callback in callbacks)
                {
                    var wrapper = new CLCWrapper<T>(callback, accept, key);
                    componentRef.GetValue().AddCLCWrapper(wrapper);
                }
            }
            else
            {
                using (ListPool<CLCWrapper<T>>.Get(out var wrappers))
                {
                    foreach (var callback in callbacks)
                    {
                        var wrapper = new CLCWrapper<T>(callback, accept);
                        wrappers.Add(wrapper);
                    }
                    var component = new CommandListenerComponent<T>(wrappers, address.world);
                    address.AddOrSetComponent(component);
                }
            }
        }

        public static void MuteCommand<T>(this EntityAddress address) where T : ICommand
        {
            MuteCommand<T>(address, address);
        }
        
        public static void MuteCommand<T>(this EntityAddress address,
            CLCKey key) where T : ICommand
        {
            if(!address.TryGetComponent<CommandListenerComponent<T>>(out var componentRef))
                return;
            
            ref var component = ref componentRef.GetValue();
            component.RemoveWithKey(key);
            
            if (component.Count <= 0) 
                address.RemoveComponent<CommandListenerComponent<T>>();
        }
    }
}