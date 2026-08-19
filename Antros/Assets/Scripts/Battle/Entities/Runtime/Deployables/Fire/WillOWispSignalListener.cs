using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Databases;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATCG.Battle.Entities.Runtime.Deployables.Fire
{
    public class WillOWispSignalListener : MonoEntitySignalListener
    {
        public override void Trigger(CommandContext context, EntityCommandSignal command)
        {
            
        }
    }
}