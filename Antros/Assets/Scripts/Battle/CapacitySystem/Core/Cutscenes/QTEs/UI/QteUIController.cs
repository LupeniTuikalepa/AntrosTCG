using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Directors;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs.UI
{
    public class QteUIController :RuntimeLocalPlayerComponent, IPhaseListener<CastCapacityPhase>
    {
        [SerializeField]
        private QteUI prefab;
        [SerializeField]
        private Transform parent;

        private Camera RenderCam => RuntimeLocalBattlePlayer.Camera.Component.OutputCamera;

        private readonly Dictionary<Qte, QteUI> uiInstances = new Dictionary<Qte, QteUI>();

        private void Awake()
        {
            parent.ClearChildren();
        }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        void IPhaseListener<CastCapacityPhase>.OnPhaseBegin(CastCapacityPhase phase)
        {
            if (phase.TryGetCapacityDirector(RuntimeLocalBattlePlayer, out CapacityDirector capacityDirector))
            {
                CapacityCutscene cutscene = capacityDirector.cutscene;

                cutscene.OnQteWindowOpened += OnQteOpened;
                cutscene.OnQteWindowClosed += OnQteClosed;
                cutscene.OnQteResolved += OnQteResolved;
            }
        }

        void IPhaseListener<CastCapacityPhase>.OnPhaseEnd(CastCapacityPhase phase)
        {
            if (phase.TryGetCapacityDirector(RuntimeLocalBattlePlayer, out CapacityDirector capacityDirector))
            {
                CapacityCutscene cutscene = capacityDirector.cutscene;

                cutscene.OnQteWindowOpened -= OnQteOpened;
                cutscene.OnQteWindowClosed -= OnQteClosed;
                cutscene.OnQteResolved -= OnQteResolved;
            }
        }

        protected override void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {

        }

        protected override void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {

        }


        private void OnQteOpened(CastCapacityPhase capacityPhase, Qte qte)
        {
            var ui = Instantiate(prefab, parent);
            uiInstances.Add(qte, ui);

            RuntimeEntityManager runtimeEntityManager = RuntimeLocalBattlePlayer.RuntimeEntityManager;
            if(capacityPhase.HasCaster && runtimeEntityManager.TryGetRuntimeEntity(capacityPhase.caster, out IRuntimeEntity runtimeEntity))
                ui.Connect(qte, runtimeEntity, RenderCam);
            else
                ui.Connect(qte, null, RenderCam);
        }

        private void OnQteResolved(CastCapacityPhase capacityPhase, Qte qte)
        {
            if (uiInstances.TryGetValue(qte, out var ui))
            {
                ui.Resolve();
            }
        }

        private void OnQteClosed(CastCapacityPhase capacityPhase, Qte qte)
        {
            if (uiInstances.Remove(qte, out var ui))
            {
                ui.Disconnect();
            }
        }


    }
}