using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpawnMagicAbilityScript : MonoBehaviour {

	[System.Serializable]
	public class ShakeParameters {
		public bool shake;
		public List<float> delays = new List<float>();
	}

	public Text effectName;
	public GameObject cameras;
	public List<GameObject> VFXs = new List<GameObject> ();
	public List<ShakeParameters> VFXsShakeParameters = new List<ShakeParameters> ();

	private int count = 0;
	private GameObject effectToSpawn;
	private ShakeParameters effectShakeParameters;
	private List<Camera> camerasList = new List<Camera> ();
    private Camera singleCamera;
    private List<GameObject> spawnedVFX = new List<GameObject>();

    void Start () {

		if (cameras.transform.childCount > 0) {
			for (int i = 0; i < cameras.transform.childCount; i++) {
				camerasList.Add (cameras.transform.GetChild (i).gameObject.GetComponent<Camera> ());
			}
			if(camerasList.Count == 0){
				Debug.Log ("Please assign one or more Cameras in inspector");
			}
		} else {
			singleCamera = cameras.GetComponent<Camera> ();
			if (singleCamera != null)
				camerasList.Add (singleCamera);
			else
				Debug.Log ("Please assign one or more Cameras in inspector");
		}

		if (VFXs.Count > 0)
			effectToSpawn = VFXs [0];
		else
			Debug.Log ("No Effects added to the VFXs List");

		if(VFXsShakeParameters.Count > 0)
			effectShakeParameters = VFXsShakeParameters [0];
		else
			Debug.Log ("No Delays added to the ShakeDelays List");

		if (effectName != null)
			effectName.text = effectToSpawn.name;
	}

	void Update () {
		Keyboard keyboard = Keyboard.current;
		if (keyboard.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
			SpawnVFX ();
		if (keyboard.dKey.wasPressedThisFrame)
			Next ();
		if (keyboard.aKey.wasPressedThisFrame)
			Previous ();
		if (keyboard.cKey.wasPressedThisFrame)
			SwitchCamera ();
		if (keyboard.xKey.wasPressedThisFrame)
			ZoomIn ();
		if (keyboard.zKey.wasPressedThisFrame)
			ZoomOut ();
	}

	public void SpawnVFX () {
		if (effectShakeParameters.shake && cameras != null)
			StartCoroutine (ShakeDelay (effectShakeParameters.delays));

		GameObject vfxSpawned = Instantiate(effectToSpawn);
		spawnedVFX.Add(vfxSpawned);

		var ps = GetFirstPS (vfxSpawned);
	}

	public void Next () {
		count++;

		CleanSpawnedVFXList();

		if (count >= VFXs.Count)
		count = 0;

		for (int i = 0; i < VFXs.Count; i++){
			if (count == i) {
                effectToSpawn = VFXs [i];
				effectShakeParameters = VFXsShakeParameters [i];
			}
			if (effectName != null)	effectName.text = effectToSpawn.name;
		}
	}

	public void Previous () {
		count--;

		CleanSpawnedVFXList();

		if (count < 0)
			count = VFXs.Count-1;

		for (int i = 0; i < VFXs.Count; i++) {
			if (count == i) {
				effectToSpawn = VFXs [i];
				effectShakeParameters = VFXsShakeParameters [i];
			}
			if (effectName != null)	effectName.text = effectToSpawn.name;
		}
	}

	public void ZoomIn () {
		if (camerasList.Count > 0) {
			if (!camerasList [0].orthographic) {
				if (camerasList [0].fieldOfView < 101) {
					for (int i = 0; i < camerasList.Count; i++) {
						camerasList [i].fieldOfView += 5;
					}
				}
			} else {
				if (camerasList [0].orthographicSize < 10) {
					for (int i = 0; i < camerasList.Count; i++) {
						camerasList [i].orthographicSize += 0.5f;
					}
				}
			}
		}
	}

	public void ZoomOut () {
		if (camerasList.Count > 0) {
			if (!camerasList [0].orthographic) {
				if (camerasList [0].fieldOfView > 20) {
					for (int i = 0; i < camerasList.Count; i++) {
						camerasList [i].fieldOfView -= 5;
					}
				}
			} else {
				if (camerasList [0].orthographicSize > 4) {
					for (int i = 0; i < camerasList.Count; i++) {
						camerasList [i].orthographicSize -= 0.5f;
					}
				}
			}
		}
	}

	public void SwitchCamera () {
		if (camerasList.Count > 0) {
			for (int i = 0; i < camerasList.Count; i++) {
				if (camerasList [i].gameObject.activeSelf) {
					camerasList [i].gameObject.SetActive (false);
					if ((i + 1) == camerasList.Count) {
						camerasList [0].gameObject.SetActive (true);
						break;
					} else {
						camerasList [i + 1].gameObject.SetActive (true);
						break;
					}
				}
			}
		}
	}

	public ParticleSystem GetFirstPS (GameObject vfx){
		var ps = vfx.GetComponent<ParticleSystem> ();
		if (ps == null && vfx.transform.childCount > 0) {
			foreach (Transform t in vfx.transform) {
				ps = t.GetComponent<ParticleSystem> ();
				if(ps != null)
					return ps;
			}
		}
		return ps;
	}

	IEnumerator ShakeDelay (List<float> delay){
		for (int i = 0; i < delay.Count; i++) {
			yield return new WaitForSeconds (delay[i]);
			cameras.GetComponent<CameraShakeSimpleScript> ().ShakeCamera ();
		}
	}


	void CleanSpawnedVFXList()
    {
		for (int i = 0; i < spawnedVFX.Count; i++)
		{
			Destroy(spawnedVFX[i]);
		}

		spawnedVFX.Clear();
	}
}