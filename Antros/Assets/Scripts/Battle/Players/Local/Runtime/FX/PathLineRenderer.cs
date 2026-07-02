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
    public class PathLineRenderer : LocalPlayerMonoPhaseListener<ICreatePathPhase>
    {
        [SerializeField]
        private LineRenderer lineRenderer;

        [SerializeField]
        private float yOffset = 0.5f;

        [SerializeField]
        private float fadeDuration = 0.5f;

        private float lineThickness;


        private void Awake()
        {
            lineThickness = lineRenderer.widthMultiplier;
        }

        protected override void OnPhaseBegin(ICreatePathPhase phase)
        {
            base.OnPhaseBegin(phase);
            lineRenderer.positionCount = 0;
            phase.OnPathChanged += OnPathChanged;
            ShowPath().ListenForExceptions();
        }

        protected override void OnPhaseEnd(ICreatePathPhase phase)
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

        private void OnPathChanged(ICreatePathPhase createPathPhase)
        {
            using (ListPool<HexCoordinates>.Get(out var fullPath))
            {
                fullPath.AddRange(createPathPhase.CurrentPath);
                fullPath.AddRange(createPathPhase.TemporaryPath);

                lineRenderer.positionCount = fullPath.Count;

                for (var i = 0; i < fullPath.Count; i++)
                {
                    var coord = fullPath[i];
                    var pathPosition = RuntimeBattlePlayer.RuntimeBattleGrid.GetPositionAt(coord);
                    lineRenderer.SetPosition(i, pathPosition + Vector3.up * yOffset);
                }
            }
        }
    }
}