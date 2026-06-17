using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids;
using Helteix.Tools;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATCG.Battle
{
    public class PathLineRenderer : LocalPlayerMonoPhaseListener<CreatePathPhase>
    {
        [SerializeField]
        private LineRenderer lineRenderer;

        [SerializeField]
        private float yOffset = 0.5f;
        
        [SerializeField]
        private float fadeDuration = 0.5f;
        
        private float lineThickness;

        private HexCoordinates startingPoint;

        private void Awake()
        {
            lineThickness = lineRenderer.widthMultiplier;
        }

        protected override void OnPhaseBegin(CreatePathPhase phase)
        {
            base.OnPhaseBegin(phase);
            lineRenderer.positionCount = 0;
            phase.OnPathChanged += PhaseOnOnPathChanged;
            startingPoint = phase.startingPoint;
            ShowPath().FireAndForget();
        }

        protected override void OnPhaseEnd(CreatePathPhase phase)
        {
            HidePath().FireAndForget();
            phase.OnPathChanged -= PhaseOnOnPathChanged;
            base.OnPhaseEnd(phase);
        }

        private async Awaitable ShowPath()
        {
            lineRenderer.enabled = true;
            Tween.CompleteAll(lineRenderer);
            await Tween.Custom(lineRenderer, 0, lineThickness, duration: fadeDuration, OnValueChange);
        }

        private async Awaitable HidePath()
        {
            Tween.CompleteAll(lineRenderer);
            await Tween.Custom(lineRenderer, lineThickness, 0, duration: fadeDuration, OnValueChange);
            lineRenderer.enabled = false;
        }

        private static void OnValueChange(LineRenderer target, float newValue)
        {
            target.widthMultiplier = newValue;
        }

        private void PhaseOnOnPathChanged(IEnumerable<HexCoordinates> path)
        {
            var array = path.ToArray();
            var lenght = array.Length;
            lineRenderer.positionCount = lenght + 1;
            if (RuntimeBattlePlayer.RuntimeBattleGrid.TryGetBattleCellAt(startingPoint, out var startingCell))
                lineRenderer.SetPosition(0, startingCell.transform.position + Vector3.up * yOffset);
            
            for (var i = 0; i < lenght; i++)
            {
                var coord = array[i];
                if (RuntimeBattlePlayer.RuntimeBattleGrid.TryGetBattleCellAt(coord, out var cell))
                    lineRenderer.SetPosition(i + 1, cell.transform.position + Vector3.up * yOffset);
                
            }
        }
    }
}