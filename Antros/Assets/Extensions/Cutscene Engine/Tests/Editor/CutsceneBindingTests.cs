using System.Collections.Generic;
using CutsceneEngine;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor.Tests
{
    public sealed class CutsceneBindingTests
    {
        readonly List<Object> _objectsToDestroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var i = _objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (_objectsToDestroy[i]) Object.DestroyImmediate(_objectsToDestroy[i]);
            }

            _objectsToDestroy.Clear();
        }

        [Test]
        public void ReplaceBindings_ConvertsTargetGameObjectForEachTrackBindingType()
        {
            var directorObject = Track(new GameObject("Director"));
            var director = directorObject.AddComponent<PlayableDirector>();
            var cutscene = directorObject.AddComponent<Cutscene>();
            cutscene.director = director;

            var original = Track(new GameObject("Original"));
            var originalAnimator = original.AddComponent<Animator>();
            var target = Track(new GameObject("Target"));
            var targetAnimator = target.AddComponent<Animator>();

            var timeline = Track(ScriptableObject.CreateInstance<TimelineAsset>());
            var animationTrack = timeline.CreateTrack<AnimationTrack>();
            var animationClip = Track(new AnimationClip());
            animationTrack.CreateClip(animationClip);
            var humanoidIKTrack = timeline.CreateTrack<HumanoidIKTrack>();
            humanoidIKTrack.CreateClip<HumanoidIKClip>();
            var lookAtTrack = timeline.CreateTrack<LookAtTrack>();
            lookAtTrack.CreateClip<LookAtClip>();

            director.playableAsset = timeline;
            director.SetGenericBinding(animationTrack, originalAnimator);
            director.SetGenericBinding(humanoidIKTrack, originalAnimator);
            director.SetGenericBinding(lookAtTrack, originalAnimator);

            cutscene.ReplaceBindings(original, target);

            Assert.That(director.GetGenericBinding(animationTrack), Is.SameAs(targetAnimator));
            Assert.That(director.GetGenericBinding(humanoidIKTrack), Is.SameAs(targetAnimator));
            Assert.That(director.GetGenericBinding(lookAtTrack), Is.SameAs(targetAnimator));
        }

        [Test]
        public void ReplaceActorBindings_RebindsMappedChildAndRestoresOriginalBindings()
        {
            var directorObject = Track(new GameObject("Director"));
            var director = directorObject.AddComponent<PlayableDirector>();
            var cutscene = directorObject.AddComponent<Cutscene>();
            cutscene.director = director;

            var preview = Track(new GameObject("Preview"));
            var previewAnimator = preview.AddComponent<Animator>();
            var previewAppearance = Track(new GameObject("Preview Appearance"));
            previewAppearance.transform.SetParent(preview.transform);
            var previewHair = Track(new GameObject("Authored Hair"));
            previewHair.transform.SetParent(previewAppearance.transform);
            previewHair.AddComponent<SkinnedMeshRenderer>();

            var actor = Track(new GameObject("Runtime Actor"));
            var actorAnimator = actor.AddComponent<Animator>();
            var actorModel = Track(new GameObject("Runtime Model"));
            actorModel.transform.SetParent(actor.transform);
            var actorHair = Track(new GameObject("Fringe Renderer"));
            actorHair.transform.SetParent(actorModel.transform);
            actorHair.AddComponent<SkinnedMeshRenderer>();

            var timeline = Track(ScriptableObject.CreateInstance<TimelineAsset>());
            var animationTrack = timeline.CreateTrack<AnimationTrack>();
            animationTrack.CreateClip(Track(new AnimationClip()));
            var colorTrack = timeline.CreateTrack<ColorTrack>();
            colorTrack.CreateClip<ColorClip>();

            director.playableAsset = timeline;
            director.SetGenericBinding(animationTrack, previewAnimator);
            director.SetGenericBinding(colorTrack, previewHair);

            var previewBindings = new List<CutsceneActorPartBinding>
            {
                new CutsceneActorPartBinding(previewHair, "hair")
            };
            var actorBindings = new List<CutsceneActorPartBinding>
            {
                new CutsceneActorPartBinding(actorHair, "hair")
            };

            Assert.That(CutsceneActorPartLookup.TryCreate(previewBindings, preview.transform,
                out var previewLookup, out var previewError), Is.True, previewError);
            Assert.That(CutsceneActorPartLookup.TryCreate(actorBindings, actor.transform,
                out var actorLookup, out var actorError), Is.True, actorError);

            var snapshots = new List<CutsceneTrackBindingSnapshot>();
            cutscene.ReplaceActorBindings(preview, previewLookup, actor, actorLookup, snapshots);

            Assert.That(director.GetGenericBinding(animationTrack), Is.SameAs(actorAnimator));
            Assert.That(director.GetGenericBinding(colorTrack), Is.SameAs(actorHair));
            Assert.That(snapshots, Has.Count.EqualTo(2));

            cutscene.RestoreBindings(snapshots);

            Assert.That(director.GetGenericBinding(animationTrack), Is.SameAs(previewAnimator));
            Assert.That(director.GetGenericBinding(colorTrack), Is.SameAs(previewHair));
        }

        [Test]
        public void ColorMixer_ProcessFrameFollowsChangedPlayerDataAndRestoresPreviousTarget()
        {
            var preview = Track(new GameObject("Preview Hair"));
            var previewRenderer = preview.AddComponent<SpriteRenderer>();
            previewRenderer.color = Color.red;
            var actor = Track(new GameObject("Actor Hair"));
            var actorRenderer = actor.AddComponent<SpriteRenderer>();
            actorRenderer.color = Color.blue;

            var graph = PlayableGraph.Create();
            try
            {
                var playable = ScriptPlayable<ColorMixerBehaviour>.Create(graph, 0);
                var behaviour = playable.GetBehaviour();

                behaviour.ProcessFrame(playable, default, preview);
                Assert.That(behaviour.spriteRenderer, Is.SameAs(previewRenderer));

                previewRenderer.color = Color.green;
                behaviour.ProcessFrame(playable, default, actor);

                Assert.That(previewRenderer.color, Is.EqualTo(Color.red));
                Assert.That(behaviour.spriteRenderer, Is.SameAs(actorRenderer));
                Assert.That(actorRenderer.color, Is.EqualTo(Color.blue));
            }
            finally
            {
                graph.Destroy();
            }
        }

        T Track<T>(T target) where T : Object
        {
            _objectsToDestroy.Add(target);
            return target;
        }
    }
}
