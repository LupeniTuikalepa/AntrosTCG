using ATCG.Scenes;
using Eflatun.SceneReference;
using Helteix.Tools.Settings;
using UnityEngine;

namespace ATCG.Metrics
{
    [System.Serializable, AutoGenerateGameSettings, GameSettingsTitle("Scenes")]
    public class GameScenes : GameSettings<GameScenes>
    {
        [field: SerializeField]
        public SceneReference  MainMenu { get; private set; }
        [field: SerializeField]
        public SceneReference  Game { get; private set; }

        [field: SerializeField]
        internal SceneLoaderUI LoaderPrefab { get; private set; }
    }
}