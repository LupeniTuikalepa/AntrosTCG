using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ATCG.Scenes
{
    public class GameSceneController
    {
        public SceneReference ActiveSceneReference { get; private set; }

        public SceneReference[] AdditionalSceneReference { get; private set; }

        internal GameSceneController()
        {

        }


        public async Awaitable<Scene> LoadScenesWithLoadingScreen(SceneReference activeScene, params SceneReference[] additionalScenes)
        {

            int[] additionalScenesIndices = new int[additionalScenes.Length];
            for (int i = 0; i < additionalScenes.Length; i++)
                additionalScenesIndices[i] = additionalScenes[i].BuildIndex;

            LoadingScenePhase loadingScenePhase = new(activeScene.BuildIndex, additionalScenesIndices);

            ActiveSceneReference = activeScene;
            AdditionalSceneReference = additionalScenes;

            await loadingScenePhase.Run();

            return activeScene.LoadedScene;
        }

        public bool IsSceneLoaded(SceneReference sceneReference, out Scene scene)
        {
            scene = SceneManager.GetSceneByPath(sceneReference.Path);
            return scene.IsValid() && scene.isLoaded;
        }

    }
}