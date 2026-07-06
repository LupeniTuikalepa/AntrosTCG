using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Passive data container placed on the cutscene test-environment prefab: a plain
    /// table of {channel name -> bound object} used only while authoring cutscenes in
    /// the editor stage. It holds no logic and depends on nothing — a MonoBehaviour
    /// solely because it must carry scene/prefab object references. All binding logic
    /// lives editor-side. Adding a new channel needs no recompile of this type: the
    /// editor's "Populate from CutsceneChannels" fills the missing rows.
    /// </summary>
    public sealed class DebugCutsceneRig : MonoBehaviour
    {
        [Serializable]
        public struct ChannelBinding
        {
            public string channelName;
            [SceneObjectsOnly]
            public UnityEngine.Object reference;
        }

        [SerializeField]
        private List<ChannelBinding> bindings = new();

        public IReadOnlyList<ChannelBinding> Bindings => bindings;

        public bool TryGet(string channelName, out UnityEngine.Object reference)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].channelName == channelName)
                {
                    reference = bindings[i].reference;
                    return reference != null;
                }
            }
            reference = null;
            return false;
        }
    }
}