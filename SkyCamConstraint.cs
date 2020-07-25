using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProceduralSkyMod
{
	class SkyCamConstraint : MonoBehaviour
	{
		public Transform mainCam;

		private void Start ()
		{
			mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
		}

		void Update ()
		{
			transform.rotation = mainCam.rotation;
		}
	}
}
