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
		public Camera main;
		public Camera sky;
		public Camera env;

		void Update ()
		{
			env.transform.rotation = sky.transform.rotation = main.transform.rotation;
			env.fieldOfView = sky.fieldOfView = main.fieldOfView;
		}
	}
}
