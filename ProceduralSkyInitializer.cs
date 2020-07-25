using System.Linq;
using System.IO;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ProceduralSkyMod
{
	public class ProceduralSkyInitializer : MonoBehaviour
	{
		private Light dirLight;
		private Camera mainCam;

		public void Init ()
		{
#if DEBUG
			Debug.Log(">>> >>> >>> Cybex_ProceduralSkyMod : Initializer Starting Setup...");
			Debug.Log(">>> >>> >>> Loading Asset Bundle...");
#endif
			// Load the asset bundle
			AssetBundle assets = AssetBundle.LoadFromFile(Main.Path + "Resources/dynamicskymod");
			GameObject bundle = assets.LoadAsset<GameObject>("Assets/Prefabs/BundleResources.prefab");
			assets.Unload(false);

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Skybox Material...");
#endif
			// Set skybox material
			Material skyMaterial = bundle.transform.Find("Sky").GetComponent<MeshRenderer>().sharedMaterial;

			skyMaterial.SetColor("_SkyTint", new Color(.5f, .5f, .5f, 1));
			skyMaterial.SetColor("_GroundColor", new Color(.369f, .349f, .341f, 1));

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Directional Light...");
#endif
			// Find directional light and setup
			GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < roots.Length; i++)
			{
				if (roots[i].name == "Directional Light")
				{
					//dirLight = roots[i].GetComponent<Light>();
					roots[i].gameObject.SetActive(false);
					break;
				}
			}

			dirLight = new GameObject() { name = "Sun" }.AddComponent<Light>();

			dirLight.type = LightType.Directional;
			dirLight.shadows = LightShadows.Soft;
			dirLight.shadowStrength = 0.9f;

			// X mode to realtime (only in editor possible)
			// intensity to 1 (obsolete with new light)
			// indirect multi to 1 (obsolete with new light)

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Render Settings...");
#endif
			// Set render settings
			RenderSettings.sun = dirLight;

			RenderSettings.skybox = skyMaterial;
			skyMaterial.SetFloat("_AtmosphereThickness", 1.1666f);
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
			RenderSettings.defaultReflectionResolution = 128;

			QualitySettings.realtimeReflectionProbes = true;

			RenderSettings.fog = false;

			// realtime GI
			// auto generate lighting

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Dynamic Sky Master...");
#endif
			// Setup dynamic sky
			GameObject dsMaster = new GameObject() { name = "DynamicSkyMod" };
			dsMaster.transform.Reset();
			SkyManager skyManager = dsMaster.AddComponent<SkyManager>();
			skyManager.latitude = 44.7872f;

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Cameras...");
#endif
			// main cam
			mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			mainCam.clearFlags = CameraClearFlags.Skybox;
			mainCam.cullingMask = 0;
			mainCam.depth = -1;
			// env cam
			Camera envCam = new GameObject() { name = "EnvCam" }.AddComponent<Camera>();
			envCam.transform.SetParent(mainCam.transform);
			envCam.transform.ResetLocal();
			envCam.clearFlags = CameraClearFlags.Depth;
			envCam.cullingMask = -1;
			envCam.cullingMask &= ~(1 << 31);
			envCam.depth = 1;
			envCam.fieldOfView = mainCam.fieldOfView;
			envCam.nearClipPlane = mainCam.nearClipPlane;
			envCam.farClipPlane = mainCam.farClipPlane;
			//envCam.enabled = false;
			// sky cam
			Camera skyCam = new GameObject() { name = "SkyCam" }.AddComponent<Camera>();
			skyCam.transform.SetParent(dsMaster.transform);
			skyCam.transform.ResetLocal();
			skyCam.gameObject.AddComponent<SkyCamConstraint>();
			skyCam.clearFlags = CameraClearFlags.Depth;
			skyCam.cullingMask = 0;
			skyCam.cullingMask |= 1 << 31;
			skyCam.depth = 0;
			skyCam.fieldOfView = mainCam.fieldOfView;
			skyCam.nearClipPlane = mainCam.nearClipPlane;
			skyCam.farClipPlane = 100;

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Cloud Plane...");
#endif
			GameObject cloudPlane = new GameObject();

			MeshFilter filter = cloudPlane.AddComponent<MeshFilter>();
			filter.sharedMesh = bundle.transform.Find("CloudPlane").GetComponent<MeshFilter>().sharedMesh;
			MeshRenderer renderer = cloudPlane.AddComponent<MeshRenderer>();
			renderer.sharedMaterial = bundle.transform.Find("CloudPlane").GetComponent<MeshRenderer>().sharedMaterial;

			cloudPlane.transform.SetParent(dsMaster.transform);
			cloudPlane.transform.ResetLocal();
			cloudPlane.layer = 31;

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Skybox Night...");
#endif
			GameObject dsSkyboxNight = new GameObject() { name = "SkyboxNight" };
			dsSkyboxNight.transform.SetParent(dsMaster.transform);
			dsSkyboxNight.transform.ResetLocal();

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Sun Position...");
#endif
			dirLight.transform.SetParent(dsSkyboxNight.transform);
			dirLight.transform.ResetLocal();
			dirLight.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
			// assign skyboxNight after sun is positioned to get correct sun rotation
			skyManager.SkyboxNight = dsSkyboxNight.transform;

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Starbox...");
#endif
			GameObject starBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
			starBox.GetComponent<MeshRenderer>().sharedMaterial = bundle.transform.Find("StarBox").GetComponent<MeshRenderer>().sharedMaterial;
			starBox.transform.SetParent(dsSkyboxNight.transform);
			starBox.transform.ResetLocal();
			starBox.transform.localRotation = Quaternion.Euler(new Vector3(0, 68.5f, 28.9f));
			starBox.transform.localScale = Vector3.one * 20;
			starBox.layer = 31;
			starBox.gameObject.SetActive(false);

#if DEBUG
			Debug.Log(">>> >>> >>> Setting Up Moon Billboard...");
#endif
			GameObject moonBillboard = new GameObject() { name = "MoonBillboard" };

			filter = moonBillboard.AddComponent<MeshFilter>();
			filter.sharedMesh = bundle.transform.Find("Moon").GetComponent<MeshFilter>().sharedMesh;
			renderer = moonBillboard.AddComponent<MeshRenderer>();
			renderer.sharedMaterial = bundle.transform.Find("Moon").GetComponent<MeshRenderer>().sharedMaterial;

			moonBillboard.transform.SetParent(dsMaster.transform);
			moonBillboard.transform.ResetLocal();
			moonBillboard.transform.localScale = Vector3.one * 5;
			moonBillboard.layer = 31;
			skyManager.MoonBillboard = moonBillboard.transform;

#if DEBUG
			Debug.Log(">>> >>> >>> Cybex_ProceduralSkyMod : Initializer Finished Setup...");
#endif
		}
	}
}
