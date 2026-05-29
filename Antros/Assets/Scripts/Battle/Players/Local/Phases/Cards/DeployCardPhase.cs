using System.Threading;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases.Cards
{
    public class DeployCardPhase : Phase<HexCoordinates>
    {
        protected override async Awaitable<HexCoordinates> Execute(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}