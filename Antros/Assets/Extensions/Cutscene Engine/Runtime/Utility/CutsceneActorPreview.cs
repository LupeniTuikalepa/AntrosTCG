using System.Collections.Generic;
using UnityEngine;

namespace CutsceneEngine
{
    /// <summary>
    /// Placeholder that is bound to the Timeline. At runtime it clones a matching origin and rebinds the track.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Cutscene Engine/Cutscene Actor Preview (Cutscene Engine)")]
    public class CutsceneActorPreview : MonoBehaviour
    {
        [SerializeField] bool deactivateOnAwake = true;
        [SerializeField] string key = "actor1";
        [SerializeField] Cutscene cutscene;
        [SerializeField] Animator avatarAnimator;
        [SerializeField] List<CutsceneActorPartBinding> partBindings = new List<CutsceneActorPartBinding>();

        readonly List<CutsceneTrackBindingSnapshot> _bindingSnapshots = new List<CutsceneTrackBindingSnapshot>();
        CutsceneActor _boundActor;

        public string Key => key;

        /// <summary> Preview-local targets indexed by semantic IDs shared with the runtime actor. </summary>
        public IReadOnlyList<CutsceneActorPartBinding> PartBindings => partBindings;

        void Reset()
        {
            avatarAnimator = GetComponent<Animator>();
            cutscene = GetComponentInParent<Cutscene>();
        }

        void Awake()
        {
            if (!avatarAnimator) avatarAnimator = GetComponent<Animator>();
            if (!cutscene) cutscene = GetComponentInParent<Cutscene>();

            if (cutscene != null)
            {
                cutscene.onStateChanged += OnCutsceneStateChanged;
                if (cutscene.state == CutsceneState.Playing)
                {
                    TryCloneAndRebind();
                }
            }
            
            if (deactivateOnAwake)
            {
                gameObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (cutscene != null)
            {
                cutscene.onStateChanged -= OnCutsceneStateChanged;
            }
        }

        void OnCutsceneStateChanged(CutsceneState state)
        {
            if (state == CutsceneState.Playing)
            {
                TryCloneAndRebind();
            }
            else if (state == CutsceneState.None)
            {
                ResetBinding();
            }
        }

        internal void TryCloneAndRebind()
        {
            if (!avatarAnimator)
            {
                Debug.LogWarning($"[{nameof(CutsceneActorPreview)}] Missing Animator reference for avatar \"{name}\".", this);
                return;
            }

            if (!cutscene || !cutscene.director)
            {
                Debug.LogWarning($"[{nameof(CutsceneActorPreview)}] Missing Cutscene/PlayableDirector reference on \"{name}\".", this);
                return;
            }

            var origin = CutsceneActor.Find(key);
            if (!origin)
            {
                Debug.LogWarning($"[{nameof(CutsceneActorPreview)}] Unable to find {nameof(CutsceneActor)} with key \"{key}\".", this);
                return;
            }

            if (!CutsceneActorPartLookup.TryCreate(partBindings, transform, out var previewParts, out var previewError))
            {
                Debug.LogWarning($"[{nameof(CutsceneActorPreview)}] Invalid part bindings on preview \"{name}\": {previewError}", this);
                return;
            }

            if (!CutsceneActorPartLookup.TryCreate(origin.PartBindings, origin.transform, out var actorParts, out var actorError))
            {
                Debug.LogWarning($"[{nameof(CutsceneActorPreview)}] Invalid part bindings on actor \"{origin.name}\": {actorError}", origin);
                return;
            }

            if (_bindingSnapshots.Count > 0)
            {
                cutscene.RestoreBindings(_bindingSnapshots);
                _bindingSnapshots.Clear();
            }

            origin.InitializeTransform(transform.position, transform.rotation);
            cutscene.ReplaceActorBindings(gameObject, previewParts, origin.gameObject, actorParts, _bindingSnapshots);
            _boundActor = origin;
            gameObject.SetActive(false);
        }

        internal void ResetBinding()
        {
            if (cutscene)
            {
                cutscene.RestoreBindings(_bindingSnapshots);
            }

            _bindingSnapshots.Clear();

            var origin = _boundActor;
            _boundActor = null;
            if (origin) origin.OnResetBinding();
        }
    }
}
