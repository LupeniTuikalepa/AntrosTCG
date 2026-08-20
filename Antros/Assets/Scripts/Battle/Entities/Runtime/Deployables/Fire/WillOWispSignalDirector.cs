using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Databases;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATCG.Battle.Entities.Runtime.Deployables.Fire
{
    public class WillOWispSignalDirector : MonoEntitySignalDirector
    {
        [SerializeField]
        private Transform vfxPrefab;
        
        [SerializeField]
        private Transform vfxContainer;

        public override async Awaitable Play(CommandDirectorState state, CommandContext context, EntityCommandSignal command)
        {
            vfxContainer.ClearChildren();

            vfxPrefab.InstantiatePrefab(vfxContainer);

            state.CompleteWindUp(this);
            
            await Awaitable.WaitForSecondsAsync(0.3f);
            
            state.CompleteFollowThrough(this);
        }
    }
}