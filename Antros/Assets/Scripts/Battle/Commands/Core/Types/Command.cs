using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Exceptions;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    [Serializable]
    public abstract partial class Command<TInfos> : IDisposable, ICommand
        where TInfos : struct, ICommandInfos
    {
        BattleID ICommand.Parent => parent;
        IReadOnlyList<BattleID> ICommand.Embeds => embeds;
        public int ResultHash => infos.GetHashCode();

        [field: SerializeField]
        public BattleID ID { get; private set; }
        [SerializeField]
        private List<BattleID> embeds;
        [SerializeField]
        private BattleID parent;
        [SerializeField]
        protected TInfos infos;

        protected Command()
        {
            embeds = ListPool<BattleID>.Get();
            infos = new TInfos();
            ID = BattleID.CreateNew();
        }

        void ICommand.Process(in CommandContext context)
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


        protected virtual void Init(in CommandContext context)
        {
        }

        protected virtual void Dispose(in CommandContext context)
        {
        }

        public TInfos GetInfos() => infos;

        protected abstract void Process(in CommandContext context);

        public void Embed<TCommand>(in CommandContext context)
            where TCommand : ICommand, new()
        {
            Embed(context, new TCommand());
        }

        public void Embed<TCommand>(in CommandContext context, TCommand command)
            where TCommand : ICommand
        {
            context.Register(command);

            embeds.Add(command.ID);
            command.SetParent(this);
            command.Process(in context);
        }


        void ICommand.SetParent(ICommand p)
        {
            parent = p.ID;
        }

        protected void Break(string message)
        {
            throw new BreakCommandException(message);
        }

        void IDisposable.Dispose()
        {
            OnDispose();
            ListPool<BattleID>.Release(embeds);
        }


        protected virtual void OnDispose()
        {
        }
    }
}