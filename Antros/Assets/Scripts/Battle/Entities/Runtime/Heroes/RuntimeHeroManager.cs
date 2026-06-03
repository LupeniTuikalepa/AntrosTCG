using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public class RuntimeHeroManager : MonoBehaviour, ICommandPlayer<DeployCardCommand>
    {
        [SerializeField]
        private RuntimeEntityManager runtimeEntityManager;

        public async Awaitable Play(DeployCardCommand command)
        {
            await Awaitable.MainThreadAsync();
        }
    }
}