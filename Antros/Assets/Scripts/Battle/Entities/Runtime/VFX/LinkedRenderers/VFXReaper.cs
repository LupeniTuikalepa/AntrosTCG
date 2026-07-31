using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.VFX
{
    /// <summary>
    /// Persistent host for particle instances handed off at a clip's end so they can fade
    /// out on their own timing and self-destruct — independent of the cutscene GameObject,
    /// which is destroyed the moment the cutscene ends. Without it an instance parented under
    /// the cutscene vanished instantly (no fade), and one parented elsewhere kept sitting in
    /// the scene after the track ended. Reap() detaches the instance onto this persistent
    /// object, stops new emission, lets whatever is already alive die, then destroys it.
    /// </summary>
    public sealed class VFXReaper : MonoBehaviour
    {
        private static VFXReaper instance;

        private static VFXReaper Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                var go = new GameObject(nameof(VFXReaper)) { hideFlags = HideFlags.HideAndDontSave };
                DontDestroyOnLoad(go);
                instance = go.AddComponent<VFXReaper>();
                return instance;
            }
        }

        // Keeps the instance's world pose and its already-live particles, cuts emission, and
        // destroys it once every particle is gone. Safe to call while the cutscene tears down.
        public static void Reap(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
                return;

            particleSystem.transform.SetParent(Instance.transform, true);
            FadeAndDestroy(particleSystem).ListenForExceptions();
        }

        private static async Awaitable FadeAndDestroy(ParticleSystem particleSystem)
        {
            // Leaves the Simulate/Pause freeze and resumes autonomous playback so the
            // remaining particles age and die on their own.
            particleSystem.Play(true);
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            while (particleSystem != null && particleSystem.particleCount > 0)
                await Awaitable.NextFrameAsync();

            if (particleSystem != null)
                particleSystem.DestroyGameObject();
        }
    }
}
