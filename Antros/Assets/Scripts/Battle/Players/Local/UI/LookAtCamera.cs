using UnityEngine;
using UnityEngine.Rendering;

namespace ATCG.Battle.Entities.Runtime.UI
{
	[AddComponentMenu("ATCG/Gameplay/UI/LookAtCamera")]
	public class LookAtCamera : MonoBehaviour
	{
		private void OnEnable()
		{
			RenderPipelineManager.beginCameraRendering += OnCameraRender;
		}

		private void OnDisable()
		{
			RenderPipelineManager.beginCameraRendering -= OnCameraRender;
		}

		private void OnCameraRender(ScriptableRenderContext context, Camera camera)
		{
			
			if (camera.cameraType == CameraType.SceneView || camera.cameraType == CameraType.Preview)
				return;

			
			transform.LookAt(transform.position + camera.transform.forward);
		}
	}
}