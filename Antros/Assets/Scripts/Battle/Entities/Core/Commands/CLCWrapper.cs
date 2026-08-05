using System;
using ATCG.Battle.Commands;

namespace ATCG.Battle.Entities.Commands
{
    public readonly struct CLCWrapper<T> : IEquatable<CLCWrapper<T>> where T : ICommand
    {
        private readonly CommandListenerComponent<T>.Callback callback;
        private readonly CommandListenerComponent<T>.Accept accept;
        private readonly CLCKey key;

        public CLCWrapper(CommandListenerComponent<T>.Callback callback 
            ,CommandListenerComponent<T>.Accept accept, 
            CLCKey key = default)
        {
            this.callback = callback;
            this.accept = accept;
            this.key = key;
        }
        
        public bool Accept(CommandContext context, T command) => accept?.Invoke(context, command) ?? true;
        public void Trigger(CommandContext context, T command) => callback?.Invoke(context, command);
        public bool HasSameKey(CLCKey key) => key.Equals(this.key);

        public bool Equals(CLCWrapper<T> other)
        {
            return Equals(callback, other.callback) && Equals(accept, other.accept);
        }

        public override bool Equals(object obj)
        {
            return obj is CLCWrapper<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(callback, accept);
        }
    }
}