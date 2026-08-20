using System;
using System.Collections.Generic;
using UnityEngine;

namespace CutsceneEngine
{
    /// <summary>
    /// Maps an actor-local GameObject to a hierarchy-independent semantic ID.
    /// </summary>
    [Serializable]
    public struct CutsceneActorPartBinding
    {
        [SerializeField, Tooltip("The actor-local GameObject represented by this part ID.")]
        GameObject target;
        [SerializeField, Tooltip("A stable, case-sensitive ID shared by the preview and runtime actor.")]
        string id;

        public GameObject Target => target;
        public string Id => id;

        public CutsceneActorPartBinding(GameObject target, string id)
        {
            this.target = target;
            this.id = id;
        }
    }

    internal sealed class CutsceneActorPartLookup
    {
        readonly Dictionary<GameObject, string> _idByTarget;
        readonly Dictionary<string, GameObject> _targetById;

        CutsceneActorPartLookup(Dictionary<GameObject, string> idByTarget,
            Dictionary<string, GameObject> targetById)
        {
            _idByTarget = idByTarget;
            _targetById = targetById;
        }

        internal static bool TryCreate(IReadOnlyList<CutsceneActorPartBinding> bindings, Transform actorRoot,
            out CutsceneActorPartLookup lookup, out string error)
        {
            if (bindings == null) bindings = Array.Empty<CutsceneActorPartBinding>();

            var idByTarget = new Dictionary<GameObject, string>();
            var targetById = new Dictionary<string, GameObject>(StringComparer.Ordinal);

            for (var i = 0; i < bindings.Count; i++)
            {
                var entry = bindings[i];
                var target = entry.Target;
                var id = entry.Id == null ? string.Empty : entry.Id.Trim();

                if (!target)
                {
                    lookup = null;
                    error = $"Part binding at index {i} has no target.";
                    return false;
                }

                if (string.IsNullOrEmpty(id))
                {
                    lookup = null;
                    error = $"Part binding for \"{target.name}\" has an empty ID.";
                    return false;
                }

                if (actorRoot && target.transform != actorRoot && !target.transform.IsChildOf(actorRoot))
                {
                    lookup = null;
                    error = $"Part \"{target.name}\" is not inside actor \"{actorRoot.name}\".";
                    return false;
                }

                if (idByTarget.ContainsKey(target))
                {
                    lookup = null;
                    error = $"Part target \"{target.name}\" is mapped more than once.";
                    return false;
                }

                if (targetById.ContainsKey(id))
                {
                    lookup = null;
                    error = $"Part ID \"{id}\" is mapped more than once.";
                    return false;
                }

                idByTarget.Add(target, id);
                targetById.Add(id, target);
            }

            lookup = new CutsceneActorPartLookup(idByTarget, targetById);
            error = null;
            return true;
        }

        internal bool TryGetId(GameObject target, out string id)
        {
            return _idByTarget.TryGetValue(target, out id);
        }

        internal bool TryGetTarget(string id, out GameObject target)
        {
            return _targetById.TryGetValue(id, out target);
        }
    }
}
