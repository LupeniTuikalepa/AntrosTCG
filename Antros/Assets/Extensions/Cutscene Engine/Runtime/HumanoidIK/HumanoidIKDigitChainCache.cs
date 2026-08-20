using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CutsceneEngine
{
    internal static class HumanoidIKDigitChainCache
    {
        sealed class AnimatorCache
        {
            readonly struct ToeRootEntry
            {
                public readonly Transform Root;
                public readonly float ReferenceRootX;
                public readonly int SiblingIndex;

                public ToeRootEntry(
                    Transform root,
                    float referenceRootX,
                    int siblingIndex)
                {
                    Root = root;
                    ReferenceRootX = referenceRootX;
                    SiblingIndex = siblingIndex;
                }
            }

            readonly Animator _animator;
            readonly Transform[][][] _chains = new Transform[4][][];
            readonly int[] _toeRootChildCounts = { -1, -1, -1, -1 };

            readonly List<ToeRootEntry> _orderedToeRoots = new List<ToeRootEntry>(5);
            Avatar _avatar;
            HumanoidIKReferencePose _referencePose;

            public AnimatorCache(Animator animator)
            {
                _animator = animator;
                _avatar = animator.avatar;
                HumanoidIKReferencePose.TryCreate(animator, out _referencePose);
            }

            public Transform[][] GetChains(HumanoidIKTarget target)
            {
                if (!_animator || _animator.avatar != _avatar)
                {
                    _avatar = _animator ? _animator.avatar : null;
                    _referencePose = null;
                    if (_animator)
                    {
                        HumanoidIKReferencePose.TryCreate(_animator, out _referencePose);
                    }
                    Array.Clear(_chains, 0, _chains.Length);
                    for (var i = 0; i < _toeRootChildCounts.Length; i++)
                    {
                        _toeRootChildCounts[i] = -1;
                    }
                }

                var index = (int)target;
                if (index < 0 || index >= _chains.Length || !HumanoidIKUtility.IsUsableHumanoid(_animator))
                {
                    return Empty;
                }

                if (_chains[index] == null ||
                    HumanoidIKUtility.IsFoot(target) && !IsFootCacheCurrent(target, _chains[index]))
                {
                    _chains[index] = BuildChains(target);
                }

                return _chains[index];
            }

            Transform[][] BuildChains(HumanoidIKTarget target)
            {
                if (HumanoidIKUtility.IsHand(target)) return BuildHandChains(target);
                var targetIndex = (int)target;

                var toes = GetToeRoot(target);
                if (!toes) return Empty;
                _toeRootChildCounts[targetIndex] = toes.childCount;
                if (toes.childCount < 2)
                {
                    return Empty;
                }

                CollectOrderedToeRoots(toes, target);
                var chainCount = Mathf.Min(_orderedToeRoots.Count, 5);
                var chains = new Transform[chainCount][];
                for (var i = 0; i < chainCount; i++)
                {
                    chains[i] = BuildChildChain(_orderedToeRoots[i].Root);
                }

                return chains;
            }

            Transform[][] BuildHandChains(HumanoidIKTarget target)
            {
                var chains = new List<Transform[]>(5);
                for (var digitIndex = 0; digitIndex < 5; digitIndex++)
                {
                    if (!HumanoidIKUtility.TryGetHandDigitBoneIds(target, digitIndex, out var boneIds)) continue;

                    var chain = new Transform[boneIds.Length];
                    var hasAnyBone = false;
                    for (var jointIndex = 0; jointIndex < boneIds.Length; jointIndex++)
                    {
                        chain[jointIndex] = _animator.GetBoneTransform(boneIds[jointIndex]);
                        if (chain[jointIndex]) hasAnyBone = true;
                    }

                    if (hasAnyBone) chains.Add(chain);
                }

                return chains.Count > 0 ? chains.ToArray() : Empty;
            }

            bool IsFootCacheCurrent(HumanoidIKTarget target, Transform[][] chains)
            {
                var toes = GetToeRoot(target);
                if (!toes) return chains.Length == 0;
                if (toes.childCount < 2)
                {
                    return chains.Length == 1 && chains[0].Length > 0 && chains[0][0] == toes;
                }

                var targetIndex = (int)target;
                if (_toeRootChildCounts[targetIndex] != toes.childCount) return false;

                var expectedCount = Mathf.Min(toes.childCount, 5);
                if (chains.Length != expectedCount) return false;

                for (var i = 0; i < expectedCount; i++)
                {
                    var chain = chains[i];
                    var root = chain != null && chain.Length > 0 ? chain[0] : null;
                    if (!root || root.parent != toes) return false;

                    for (var previousIndex = 0; previousIndex < i; previousIndex++)
                    {
                        if (chains[previousIndex][0] == root) return false;
                    }

                    var current = root;
                    for (var jointIndex = 1; jointIndex < chain.Length; jointIndex++)
                    {
                        current = current && current.childCount > 0 ? current.GetChild(0) : null;
                        if (chain[jointIndex] != current) return false;
                    }
                }

                return true;
            }

            void CollectOrderedToeRoots(Transform toes, HumanoidIKTarget target)
            {
                _orderedToeRoots.Clear();
                var allHaveReferencePositions = _referencePose != null;
                for (var i = 0; i < toes.childCount; i++)
                {
                    var root = toes.GetChild(i);
                    var hasReferencePosition = TryGetReferenceRootX(root, out var referenceRootX);
                    allHaveReferencePositions &= hasReferencePosition;
                    _orderedToeRoots.Add(new ToeRootEntry(root, referenceRootX, i));
                }

                // A partial reference ordering can be non-transitive. If any branch is
                // absent from the Avatar skeleton, preserve the deterministic sibling order.
                if (!allHaveReferencePositions) return;

                var isLeftFoot = target == HumanoidIKTarget.LeftFoot;
                for (var i = 1; i < _orderedToeRoots.Count; i++)
                {
                    var current = _orderedToeRoots[i];
                    var insertionIndex = i - 1;
                    while (insertionIndex >= 0 &&
                           CompareToeRootOrder(
                               current.ReferenceRootX,
                               current.SiblingIndex,
                               _orderedToeRoots[insertionIndex].ReferenceRootX,
                               _orderedToeRoots[insertionIndex].SiblingIndex,
                               isLeftFoot) < 0)
                    {
                        _orderedToeRoots[insertionIndex + 1] = _orderedToeRoots[insertionIndex];
                        insertionIndex--;
                    }

                    _orderedToeRoots[insertionIndex + 1] = current;
                }
            }

            bool TryGetReferenceRootX(Transform toeRoot, out float referenceRootX)
            {
                referenceRootX = 0f;
                if (_referencePose == null ||
                    !_referencePose.TryGetRelativeMatrix(_animator.transform, toeRoot, out var matrix))
                {
                    return false;
                }

                referenceRootX = matrix.MultiplyPoint3x4(Vector3.zero).x;
                return true;
            }

            Transform GetToeRoot(HumanoidIKTarget target)
            {
                return HumanoidIKUtility.GetToeRoot(_animator, target);
            }

            static Transform[] BuildChildChain(Transform root)
            {
                var chain = new Transform[3];
                chain[0] = root;

                var current = root;
                for (var i = 1; i < chain.Length; i++)
                {
                    if (!current || current.childCount == 0) break;
                    current = current.GetChild(0);
                    chain[i] = current;
                }

                return chain;
            }
        }

        static readonly Transform[][] Empty = Array.Empty<Transform[]>();
        static readonly ConditionalWeakTable<Animator, AnimatorCache> Caches =
            new ConditionalWeakTable<Animator, AnimatorCache>();

        public static Transform[][] GetChains(Animator animator, HumanoidIKTarget target)
        {
            return animator ? Caches.GetValue(animator, key => new AnimatorCache(key)).GetChains(target) : Empty;
        }

        internal static int CompareToeRootOrder(
            float aReferenceRootX,
            int aSiblingIndex,
            float bReferenceRootX,
            int bSiblingIndex,
            bool isLeftFoot)
        {
            const float LateralTolerance = 0.000001f;
            var lateralDelta = aReferenceRootX - bReferenceRootX;
            if (Mathf.Abs(lateralDelta) > LateralTolerance)
            {
                var lateralOrder = aReferenceRootX.CompareTo(bReferenceRootX);
                return isLeftFoot ? -lateralOrder : lateralOrder;
            }

            return aSiblingIndex.CompareTo(bSiblingIndex);
        }
    }
}
