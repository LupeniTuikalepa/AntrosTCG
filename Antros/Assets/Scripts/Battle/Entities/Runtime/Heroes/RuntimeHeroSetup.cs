using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Components;
using ATCG.Metrics;
using Helteix.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
	public class RuntimeHeroSetup : MonoBehaviour,IRuntimeEntityComponent<HeroEntityAspect>
	{
		[SerializeField, BoxGroup("Setup")]
		private Transform modelRoot;

		[SerializeField, BoxGroup("Setup")]
		private LinkedRendererMapper animation;

		private GameObject spawnedModelInstance;

		public void Connect(HeroEntityAspect aspect, RuntimeEntity<HeroEntityAspect> runtimeEntity)
		{
			var herodata = aspect.HeroCard.Data;

			if(herodata!=null)
			{
				modelRoot.ClearChildren();

				spawnedModelInstance = Instantiate(herodata.HeroPawnPrefab, modelRoot);
				spawnedModelInstance.transform.localPosition = Vector3.zero;
				spawnedModelInstance.transform.localRotation = Quaternion.identity;

				Animator modelAnimator = spawnedModelInstance.GetComponentInChildren<Animator>();
				animation.SetAnimator(modelAnimator);

				modelAnimator.applyRootMotion = false;
			}
		}

		public void Disconnect(HeroEntityAspect aspect, RuntimeEntity<HeroEntityAspect> runtimeEntity)
		{

		}
	}
}