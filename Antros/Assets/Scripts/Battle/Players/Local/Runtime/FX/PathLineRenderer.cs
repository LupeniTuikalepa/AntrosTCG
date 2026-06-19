using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids;
using Helteix.Tools;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;
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
            phase.OnPathChanged += OnPathChanged;
            startingPoint = phase.startingPoint;
            ShowPath().ListenForExceptions();
        }

        protected override void OnPhaseEnd(CreatePathPhase phase)
        {
            HidePath().ListenForExceptions();
            phase.OnPathChanged -= OnPathChanged;
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

        private void OnPathChanged(IEnumerable<HexCoordinates> result)
        {
            using var hexPathfinder = new HexPathfinder(10000);
            using (ListPool<HexCoordinates>.Get(out var fullPath))
            using (ListPool<HexCoordinates>.Get(out var segment))
            {
                var from = startingPoint;

                foreach (var to in result)
                {
                    segment.Clear();
                    hexPathfinder.FindPath(from, to, segment, RuntimeBattlePlayer.RuntimeBattleGrid.BattleGrid);
                    fullPath.AddRange(segment);
                    from = to;
                }

                lineRenderer.positionCount = fullPath.Count + 1;

                if (RuntimeBattlePlayer.RuntimeBattleGrid.TryGetBattleCellAt(startingPoint, out var startingCell))
                    lineRenderer.SetPosition(0, startingCell.transform.position + Vector3.up * yOffset);

                for (var i = 0; i < fullPath.Count; i++)
                {
                    var coord = fullPath[i];
                    if (RuntimeBattlePlayer.RuntimeBattleGrid.TryGetBattleCellAt(coord, out var cell))
                        lineRenderer.SetPosition(i + 1, cell.transform.position + Vector3.up * yOffset);
                }
            }
        }
    }
}