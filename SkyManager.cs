using System.Collections;
using UnityEngine;

namespace ProceduralSkyMod
{
	public class SkyManager : MonoBehaviour
	{
		// TODO: solve cloud hopping problem

		private Color ambientDay = new Color(.282f, .270f, .243f, 1f);
		private Color ambientNight = new Color(.079f, .079f, .112f, 1f);
		private Color defaultFog, nightFog;

		public float latitude = 0f;

		private Vector3 worldPos;

		private Transform skyboxNight;
		private Transform moonBillboard;

		public Transform SkyboxNight {
			get => skyboxNight;
			set
			{
				skyboxNight = value;
				skyboxNight.localRotation = Quaternion.Euler(new Vector3(-latitude, 0, 0));
			}
		}

		public Transform MoonBillboard
		{
			get => moonBillboard;
			set
			{
				moonBillboard = value;
				moonBillboard.localRotation = Quaternion.Euler(new Vector3(-latitude + 23.4f + 5.14f + 180f, 0, 0));
			}
		}

		public Light Sun { get; set; }
		public Material StarMaterial { get; set; }
		public Material SkyMaterial { get; set; }
		public Material CloudMaterial { get; set; }

		public Transform SkyCam { get; set; }
		public Transform CloudPlane { get; set; }


		float pY, cY, delta, dMin, dMax;

		void Start ()
		{
			defaultFog = RenderSettings.fogColor;
			nightFog = new Color(defaultFog.r * 0.1f, defaultFog.g * 0.1f, defaultFog.b * 0.1f, 1f);
			Debug.Log(string.Format("Fog: d = {0}, n = {1}", defaultFog, nightFog));

			StartCoroutine(CloudChanger());

			// DEBUG
			pY = PlayerManager.PlayerTransform.position.y;
			cY = PlayerManager.PlayerCamera.transform.position.y;
			delta = dMin = dMax = cY - pY;
			// END DEBUG

			SkyCam.localPosition = Vector3.zero + Vector3.up * delta;
			CloudPlane.localPosition = SkyCam.localPosition;
		}

		void Update ()
		{
			skyboxNight.Rotate(Vector3.forward, 0.01f, Space.Self);
			moonBillboard.Rotate(Vector3.forward, -0.01f, Space.Self);

			worldPos = PlayerManager.PlayerTransform.position - WorldMover.currentMove;
			transform.position = new Vector3(worldPos.x * .001f, 0, worldPos.z * .001f);

			Vector3 sunPos = Sun.transform.position;
			Sun.intensity = Mathf.Clamp01(sunPos.y);

			StarMaterial.SetFloat("_Visibility", (-Sun.intensity + 1) * .1f);

			SkyMaterial.SetFloat("_Exposure", Mathf.Lerp(.01f, 1f, Sun.intensity));

			CloudMaterial.SetFloat("_CloudBright", Mathf.Lerp(.02f, .9f, Sun.intensity));
			CloudMaterial.SetFloat("_CloudGradient", Mathf.Lerp(.45f, .2f, Sun.intensity));

			RenderSettings.fogColor = Color.Lerp(nightFog, defaultFog, Sun.intensity);
			RenderSettings.ambientSkyColor = Color.Lerp(ambientNight, ambientDay, Sun.intensity);

			cloudCurrent = Mathf.Lerp(cloudCurrent, cloudTarget, Time.deltaTime * 0.1f);
			CloudMaterial.SetFloat("_ClearSky", cloudCurrent);

			// DEBUG
			pY = PlayerManager.PlayerTransform.position.y;
			cY = PlayerManager.PlayerCamera.transform.position.y;
			delta = cY - pY;

			SkyCam.localPosition = Vector3.zero + Vector3.up * delta;

			if (delta < dMin) dMin = delta;
			if (delta > dMax) dMax = delta;

			Debug.Log(string.Format(
				"CAM > delta {0}, min: {1}, max: {2}", 
				delta, dMin, dMax));
		}

		void OnDisable ()
		{
			StopCoroutine(CloudChanger());
		}

		float cloudTarget = 2, cloudCurrent = 1;
		private IEnumerator CloudChanger ()
		{
			while (true)
			{
				yield return new WaitForSeconds(30);
				// .5 to 5 to test it
				cloudTarget = Mathf.Clamp(Random.value * 5, .5f, 5f);
#if DEBUG
				Debug.Log(string.Format("New Cloud Target of {0}, current {1}", cloudTarget, cloudCurrent));
#endif
			}
		}
	}
}
