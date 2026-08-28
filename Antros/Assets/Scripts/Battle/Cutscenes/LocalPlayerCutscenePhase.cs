using System.Threading;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.UI;
using ATCG.Cutscenes;
using UnityEngine;

namespace ATCG.Battle.Cutscenes
{
    public sealed class LocalPlayerCutscenePhase : LocalPlayerPhase<bool>, ILocalHUDPhase<LocalPlayerCutscenePhase>
    {
        private readonly Cutscene cutscene;

        public LocalPlayerCutscenePhase(LocalBattlePlayer localBattlePlayer, Cutscene cutscene) : base(localBattlePlayer)
        {
            this.cutscene = cutscene;
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            await cutscene.Play(token);
            return true;
        }
    }
}
