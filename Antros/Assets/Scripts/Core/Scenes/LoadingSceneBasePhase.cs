using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ATCG.Metrics;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ATCG.Scenes
{
    public class LoadingScenePhase : IPhase<bool>
    {
        public readonly int[] scenesIndices;

        public float Progress
        {
            get
            {
                float sum = 0f;
                for (int i = 0; i < scenesProcesses.Count; i++)
                    sum += scenesProcesses[i].progress;

                return sum / scenesProcesses.Count;
            }
        }

        private List<AsyncOperation> scenesProcesses;

        private Scene loadingScene;
        private SceneLoaderUI loaderUI;

        public LoadingScenePhase(int mainScene, params int[] additionalScenes)
        {
            scenesIndices = new int[additionalScenes.Length + 1];
            scenesIndices[0] = mainScene;

            for (int i = 0; i < additionalScenes.Length; i++)
                scenesIndices[i + 1] = additionalScenes[i];

            scenesProcesses = new ();
        }


        async Awaitable IPhase<bool>.Initialize(CancellationToken token)
        {
            loadingScene = SceneManager.CreateScene("LoadingScene");
            loaderUI = Object.Instantiate(GameScenes.Current.LoaderPrefab, new InstantiateParameters()
            {
                scene = loadingScene,
            });


            await loaderUI.OnPhaseBegin(this);
            SceneManager.SetActiveScene(loadingScene);
        }

        async Awaitable<bool> IPhase<bool>.Execute(CancellationToken token)
        {
            int loadedSceneCount = SceneManager.loadedSceneCount;
            Scene[] loadedScenes = new Scene[loadedSceneCount];
            for (int i = 0; i < loadedSceneCount; i++)
                loadedScenes[i] = SceneManager.GetSceneAt(i);

            for (int i = 0; i < loadedSceneCount; i++)
            {
                if(loadedScenes[i] != loadingScene)
                    await SceneManager.UnloadSceneAsync(loadedScenes[i]);
            }
            scenesProcesses.Clear();
            for (int i = 0; i < scenesIndices.Length; i++)
            {
                int sceneIndex = scenesIndices[i];
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
                if (op == null)
                    continue;

                scenesProcesses.Add(op);
            }

            while (scenesProcesses.Any(ctx => !ctx.isDone))
                await Awaitable.NextFrameAsync(token);

            await Awaitable.EndOfFrameAsync(token);
            scenesProcesses.Clear();

            Scene newActiveScene = SceneManager.GetSceneByBuildIndex(scenesIndices[0]);
            SceneManager.SetActiveScene(newActiveScene);
            return true;
        }


        async Awaitable IPhase<bool>.Dispose(CancellationToken token)
        {
            await loaderUI.OnPhaseBegin(this);
            await SceneManager.UnloadSceneAsync(loadingScene);
        }
    }
}