using System;
using Helteix.Tools.Settings;
using Helteix.Tools.TypeMapping;
using Sirenix.OdinInspector;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Metrics
{
    [AutoGenerateGameSettings, GameSettingsPath("Antros/Game Assets")]
    public class GameAssets : GameSettings<GameAssets>
    {
        [field: SerializeField, BoxGroup("Controls"), PreviewField]
        public Sprite SwitchGamepad { get; private set; }
        [field: SerializeField, BoxGroup("Controls"), PreviewField]
        public Sprite PSGamepad { get; private set; }
        [field: SerializeField, BoxGroup("Controls"), PreviewField]
        public Sprite XboxGamepad { get; private set; }
        [field: SerializeField, BoxGroup("Controls"), PreviewField]
        public Sprite DefaultGamepad { get; private set; }

        [field: SerializeField, BoxGroup("Controls"), PreviewField]
        public Sprite Keyboard { get; private set; }

        [field: SerializeField, BoxGroup("Inputs")]
        public InputActionAsset PlayerControls { get; private set; }

        [field: SerializeField, BoxGroup("Prefabs")]
        public GameObject HeroPawnPrefab { get; private set; }
    }
}