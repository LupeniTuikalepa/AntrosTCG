using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    [Serializable]
    public abstract class GameCommand : IDisposable
    {
        public IReadOnlyList<GameCommand> Embeds => embeds;

        [field: SerializeReference]
        public GameCommand Parent { get; private set; }

        [ShowInInspector]
        private List<GameCommand> embeds;

        protected GameCommand()
        {
            embeds = ListPool<GameCommand>.Get();
        }

        public abstract void Process(in GameCommandContext context);

        public void Embed<T>(in GameCommandContext context) where T : GameCommand, new()
        {
            Embed(context, new T());
        }

        public void Embed<T>(in GameCommandContext context, T command) where T : GameCommand
        {
            context.Register(command);

            embeds.Add(command);
            command.Parent = this;
            command.Process(in context);
        }

        public IEnumerable<GameCommand> GetChildren()
        {
            foreach (GameCommand command in embeds)
            {
                yield return command;

                foreach (GameCommand embed in command.GetChildren())
                    yield return embed;
            }
        }

        public IEnumerable<T> GetChildren<T>() where T : GameCommand
        {
            foreach (GameCommand subEvent in GetChildren())
                if (subEvent is T t)
                    yield return t;
        }

        public bool HasAnyChildrenOfType<T>(out T firstFound)
        {
            foreach (GameCommand subEvent in GetChildren())
                if (subEvent is T t)
                {
                    firstFound = t;
                    return true;
                }

            firstFound = default;
            return false;
        }

        public bool HasAnyAncestorOfType<T>(out T firstFound) where T : GameCommand
        {
            foreach (GameCommand e in GetAncestors())
                if (e is T t)
                {
                    firstFound = t;
                    return true;
                }

            firstFound = null;
            return false;
        }

        public IEnumerable<T> GetAncestorsOfType<T>() where T : GameCommand
        {
            foreach (GameCommand entityEvent in GetAncestors())
                if (entityEvent is T t)
                    yield return t;
        }

        public IEnumerable<GameCommand> GetAncestors()
        {
            GameCommand parent = Parent;
            while (parent != null)
            {
                yield return parent;
                parent = Parent.Parent;
            }
        }
        void IDisposable.Dispose()
        {
            OnDispose();
            ListPool<GameCommand>.Release(embeds);
            foreach (GameCommand subEvent in embeds)
                ((IDisposable)subEvent).Dispose();

        }

        protected virtual void OnDispose()
        {
        }

    }
}