using UnityEngine;

namespace ProceduralSkyMod
{
	public class SkyManager : MonoBehaviour
	{
		public float latitude = 0f;

		private Transform skyboxNight;
		private Transform moonBillboard;

		private Transform mainCam;

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

		void Start ()
		{
			//skyboxNight = transform.Find("SkyboxNight");
			//moonBillboard = transform.Find("MoonBillboard");
			mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;

			//skyboxNight.rotation = Quaternion.Euler(new Vector3(-latitude, 0, 0));
			//moonBillboard.rotation = Quaternion.Euler(new Vector3(-latitude + 23.4f + 5.14f + 180f, 0, 0));
		}

		void Update ()
		{
			skyboxNight.Rotate(Vector3.forward, 0.05f, Space.Self);
			moonBillboard.Rotate(Vector3.forward, -0.05f, Space.Self);

			transform.position = new Vector3(mainCam.position.x * 0.1f, 0, mainCam.position.z * 0.1f);
		}
	}
}
