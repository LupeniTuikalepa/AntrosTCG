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
        [SerializeField]
        private Transform vfxPrefab;
        
        [SerializeField]
        private Transform vfxContainer;
        
        public override void Trigger(CommandContext context, EntityCommandSignal command)
        {
            foreach (Transform child in vfxContainer)
                    Destroy(child.gameObject);

            Instantiate(vfxPrefab, vfxContainer);
        }
    }
}