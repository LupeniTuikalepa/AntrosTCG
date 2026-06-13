using System;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Metrics;
using Linework.SurfaceFill;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle
{
    public class SetupPlayerOutline : RuntimeLocalPlayerComponent
    {
        [SerializeField]
        private SurfaceFillSettings  settings;

        [SerializeField, InlineEditor(InlineEditorModes.FullEditor, Expanded = true)]
        private Fill fill;

        [SerializeField, Range(0, 5)]
        private float emission;


        private void Awake()
        {
            fill = Instantiate(fill);
            fill.Cleanup();
        }

        private void OnEnable()
        {
            settings.Fills.Add(fill);
            fill.Cleanup();
            settings.Changed();
            fill.SetActive(true);
        }

        private void OnDisable()
        {
            settings.Fills.Remove(fill);
            fill.Cleanup();
            fill.SetActive(false);
            settings.Changed();
        }

        private void OnValidate()
        {
            if (settings.Fills.Contains(fill) && Player != null)
            {
                Refresh(Player);
            }
        }

        private void Refresh(LocalBattlePlayer player)
        {
            Color color = Player.GetPlayerColor() * Mathf.Pow(2f, emission);
            fill.primaryColor = color;
            fill.RenderingLayer = RenderingLayerMask.GetMask($"Player{player.ID + 1}");
            settings.Changed();
        }

        protected override void Connect(LocalBattlePlayer player)
        {
            Refresh(player);
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {

        }

    }
}