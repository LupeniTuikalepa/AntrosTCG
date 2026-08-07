using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities.Components;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Commands
{
    [AutoStaticsCleanup]
    public readonly partial struct CommandListenerComponent<T> : 
        IEntityComponent where T : ICommand
    {
        public delegate void Callback(in CommandContext context, in T command);
        public delegate bool Accept(in CommandContext context, in T command);
        
        private class Listener : ICommandListener<T>
        {
            public void Trigger(CommandContext context, T command)
            {
                foreach (var world in World.ActiveWorlds)
                {
                    foreach (var componentRef in world.Query<CommandListenerComponent<T>>())
                    {
                        componentRef.GetValue().Trigger(context, command);
                    }
                }
            }
        }
        
        private static int increment;
        private static Listener listener;
        
        static CommandListenerComponent()
        {
            increment = 0;
        } 
        
        private readonly List<CLCWrapper<T>> wrappers;
        
        public int Count => wrappers.Count;

        public CommandListenerComponent(IEnumerable<CLCWrapper<T>> wrappers)
        {
            this.wrappers = ListPool<CLCWrapper<T>>.Get();
            this.wrappers.AddRange(wrappers);
            
            increment++;
            if (listener == null)
            {
                listener = new Listener();
                listener.Register();
            }
        }

        public void AddCLCWrapper(CLCWrapper<T> callback) => wrappers.Add(callback);
        public bool RemoveCLCWrapper(CLCWrapper<T> callback) => wrappers.Remove(callback);

        public void RemoveWithKey(CLCKey key)
        {
            using (ListPool<CLCWrapper<T>>.Get(out var removeWrappers))
            {
                foreach (var wrapper in wrappers)
                {
                    if (wrapper.HasSameKey(key))
                        removeWrappers.Add(wrapper);
                }

                foreach (var removeWrapper in removeWrappers)
                {
                    RemoveCLCWrapper(removeWrapper);
                }
            }
        }

        private void Trigger(CommandContext context, T command)
        {
            foreach (var wrapper in wrappers)
            {
                if(wrapper.Accept(context,command))
                    wrapper.Trigger(context, command);
            }
        }

        void IEntityComponent.Dispose()
        {
            increment--;
            if (increment <= 0 && listener != null)
            {
                increment = 0;
                listener.Unregister();
                listener = null;
            }
            
            ListPool<CLCWrapper<T>>.Release(wrappers);
        }
    }
}