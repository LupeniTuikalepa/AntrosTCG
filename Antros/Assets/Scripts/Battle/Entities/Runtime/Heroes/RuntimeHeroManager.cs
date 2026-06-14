using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Players;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public class RuntimeHeroManager : MonoBehaviour, ICommandPlayer<DeployCardCommand>
    {
        [SerializeField]
        private RuntimeEntityManager runtimeEntityManager;

        public async Awaitable Play(CommandPlayerState state, DeployCardCommand command)
        {
            
        }
    }
}