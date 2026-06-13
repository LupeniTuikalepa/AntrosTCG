using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Exceptions;
using ATCG.Battle.GameModes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    [Serializable]
    public abstract class GameCommand<TInfos> : IDisposable, IGameCommand where TInfos : struct

    {
    public IReadOnlyList<IGameCommand> Embeds => embeds;

    [field: SerializeReference]
    public IGameCommand Parent { get; private set; }

    [ShowInInspector]
    private List<IGameCommand> embeds;

    public int ResultHash => infos.GetHashCode();

    protected TInfos infos;

    protected GameCommand()
    {
        embeds = ListPool<IGameCommand>.Get();
        infos = new TInfos();
    }

    void IGameCommand.Process(in GameCommandContext context)
    {
        try
        {
            Init(in context);

            Process(in context);
        }
        finally
        {
            Dispose(in context);
        }
    }

    protected virtual void Init(in GameCommandContext context)
    {
    }

    protected virtual void Dispose(in GameCommandContext context)
    {

    }

    public TInfos GetInfos() => infos;

    protected abstract void Process(in GameCommandContext context);

    public void Embed<TCommand>(in GameCommandContext context)
        where TCommand : IGameCommand, new()
    {
        Embed(context, new TCommand());
    }

    public void Embed<TCommand>(in GameCommandContext context, TCommand command)
        where TCommand : IGameCommand
    {
        context.Register(command);

        embeds.Add(command);
        command.SetParent(this);
        command.Process(in context);
    }


    void IGameCommand.SetParent(IGameCommand parent)
    {
        Parent = parent;
    }

    public IEnumerable<IGameCommand> GetChildren()
    {
        foreach (IGameCommand command in embeds)
        {
            yield return command;

            foreach (IGameCommand embed in command.GetChildren())
                yield return embed;
        }
    }

    public IEnumerable<TCommand> GetChildren<TCommand>() where TCommand : IGameCommand
    {
        foreach (IGameCommand subEvent in GetChildren())
            if (subEvent is TCommand t)
                yield return t;
    }

    public bool HasAnyChildrenOfType<TCommand>(out TCommand firstFound) where TCommand : IGameCommand
    {
        foreach (IGameCommand subEvent in GetChildren())
            if (subEvent is TCommand t)
            {
                firstFound = t;
                return true;
            }

        firstFound = default;
        return false;
    }

    public bool HasAnyAncestorOfType<TCommand>(out TCommand firstFound) where TCommand : IGameCommand
    {
        foreach (IGameCommand e in GetAncestors())
            if (e is TCommand t)
            {
                firstFound = t;
                return true;
            }

        firstFound = default;
        return false;
    }

    public IEnumerable<TCommand> GetAncestorsOfType<TCommand>() where TCommand : IGameCommand
    {
        foreach (IGameCommand entityEvent in GetAncestors())
            if (entityEvent is TCommand t)
                yield return t;
    }

    public IEnumerable<IGameCommand> GetAncestors()
    {
        IGameCommand parent = Parent;
        while (parent != null)
        {
            yield return parent;
            parent = Parent.Parent;
        }
    }

    protected void Break(string message)
    {
        throw new BreakCommandException(message);
    }

    void IDisposable.Dispose()
    {
        OnDispose();
        ListPool<IGameCommand>.Release(embeds);
        foreach (IGameCommand subEvent in embeds)
            ((IDisposable)subEvent).Dispose();

    }


    protected virtual void OnDispose()
    {
    }

    }
}