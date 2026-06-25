using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Components.Status.Signals;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatus : MonoBehaviour, ICommandListener<StatusSignal>
    {
        private void OnEnable()
        {
            this.RegisterListener();
        }

        private void OnDisable()
        {
            this.UnregisterListener();
        }

        public async Awaitable Play(CommandListenerState state, CommandContext context, StatusSignal command)
        {
            await Awaitable.MainThreadAsync();
            state.CompleteAll(this);
            var infos = command.GetInfos();
            
            int id = infos.id;
            StatusAction action = infos.action;
        }
    }
}