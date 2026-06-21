
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace ATCG.Battle.Players.Local.Runtime.Cameras
{
    public class OrbitalRecentering : MonoBehaviour
    {
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
        [SerializeField] private float recenterTime = 0.5f;

        public void Recenter()
        {
            StopAllCoroutines();
            StartCoroutine(RecenterRoutine());
        }

        private IEnumerator RecenterRoutine()
        {
            EnableRecentering(ref orbitalFollow.HorizontalAxis, true);
            EnableRecentering(ref orbitalFollow.VerticalAxis, true);

            orbitalFollow.HorizontalAxis.TriggerRecentering();
            orbitalFollow.VerticalAxis.TriggerRecentering();

            yield return new WaitForSeconds(recenterTime);

            EnableRecentering(ref orbitalFollow.HorizontalAxis, false);
            EnableRecentering(ref orbitalFollow.VerticalAxis, false);
        }

        private void EnableRecentering(ref InputAxis axis, bool enabled)
        {
            var rec = axis.Recentering;
            rec.Enabled = enabled;
            rec.Wait = 0f;
            rec.Time = recenterTime;
            axis.Recentering = rec;
        }
    }
}