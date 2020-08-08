using System;
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
		public float longitude = 0f;

		private const float millisecondsPer15Degrees = 240000;
		private DateTime dayStart;
		private DateTime yearEnd;
		private int DaysInYear
        {
			get => new DateTime(yearEnd.Year, yearEnd.Month, yearEnd.Day).DayOfYear;
        }

		private Vector3 worldPos;

		private Transform sunMover;
		private Transform skyboxNight;
		private Transform moonBillboard;

		public Transform SunMover {
			get => sunMover;
			set => sunMover = value;
        }

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
				moonBillboard.localRotation = Quaternion.Euler(new Vector3(-latitude + 23.4f + 5.14f, 0, 0));
			}
		}

		public Light Sun { get; set; }
		public Material StarMaterial { get; set; }
		public Material SkyMaterial { get; set; }
		public Material CloudMaterial { get; set; }

		public Transform SkyCam { get; set; }
		public Transform CloudPlane { get; set; }
		public Transform EnvCam { get; set; }

		void Start ()
		{
			defaultFog = RenderSettings.fogColor;
			nightFog = new Color(defaultFog.r * 0.1f, defaultFog.g * 0.1f, defaultFog.b * 0.1f, 1f);

			StartCoroutine(CloudChanger());
		}

		void Update ()
		{
			DateTime clockTime = TimeSource.GetCurrentTime();

			if (yearEnd == null || yearEnd.Year != clockTime.Year)
			{
				yearEnd = new DateTime(clockTime.Year, 12, 31);
			}
			if (dayStart == null || dayStart.Day != clockTime.Day)
            {
				dayStart = new DateTime(clockTime.Year, clockTime.Month, clockTime.Day);
            }

			DateTime utcTime = clockTime.ToUniversalTime();
			DateTime solarTime = new DateTime(
				utcTime.Year,
				utcTime.Month,
				utcTime.Day,
				utcTime.Hour,
				utcTime.Minute,
				utcTime.Second,
				utcTime.Millisecond,
				DateTimeKind.Local).AddMilliseconds(longitude * millisecondsPer15Degrees);
			TimeSpan timeSinceMidnight = solarTime.Subtract(dayStart);
			float dayFration = (float)timeSinceMidnight.TotalHours / 24;

			// rotating the skybox 1 extra rotation per year causes the night sky to differ between summer and winter
			float yearlyAngle = 360 * (clockTime.DayOfYear + dayFration) / DaysInYear;
			float dailyAngle = 360 * dayFration;
			skyboxNight.rotation = Quaternion.AngleAxis((dailyAngle + yearlyAngle) % 360, skyboxNight.forward);
			// anti-rotating the sun 1 rotation per year keeps the solar day centered on solar noon
			SunMover.rotation = Quaternion.AngleAxis(-yearlyAngle, SunMover.forward);
			// moon is new when rotation around self.forward is 0
			float phaseAngle = ComputeMoonPhase(solarTime);
			moonBillboard.rotation = Quaternion.AngleAxis((dailyAngle - phaseAngle) % 360, moonBillboard.forward);

			worldPos = PlayerManager.PlayerTransform.position - WorldMover.currentMove;
			transform.position = new Vector3(worldPos.x * .001f, 0, worldPos.z * .001f);

			Vector3 sunPos = Sun.transform.position - transform.position;
			Sun.intensity = Mathf.Clamp01(sunPos.y);

			StarMaterial.SetFloat("_Visibility", (-Sun.intensity + 1) * .1f);

			SkyMaterial.SetFloat("_Exposure", Mathf.Lerp(.01f, 1f, Sun.intensity));

			CloudMaterial.SetFloat("_CloudBright", Mathf.Lerp(.02f, .9f, Sun.intensity));
			CloudMaterial.SetFloat("_CloudGradient", Mathf.Lerp(.45f, .2f, Sun.intensity));

			RenderSettings.fogColor = Color.Lerp(nightFog, defaultFog, Sun.intensity);
			RenderSettings.ambientSkyColor = Color.Lerp(ambientNight, ambientDay, Sun.intensity);

			cloudCurrent = Mathf.Lerp(cloudCurrent, cloudTarget, Time.deltaTime * 0.1f);
			CloudMaterial.SetFloat("_ClearSky", cloudCurrent);
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
				yield return new WaitForSeconds(60);
				// .5 to 5 to test it
				cloudTarget = Mathf.Clamp(UnityEngine.Random.value * 5, .5f, 5f);
#if DEBUG
				Debug.Log(string.Format("New Cloud Target of {0}, current {1}", cloudTarget, cloudCurrent));
#endif
			}
		}

		// phase range is [0-360)
		// taken from https://www.subsystems.us/uploads/9/8/9/4/98948044/moonphase.pdf
		float ComputeMoonPhase(DateTime now)
        {
			var jDays = ToJulianDays(now);
			var jDaysSinceKnownNewMoon = jDays - 2451549.5; // known new moon on 2000 January 6
			var newMoonsSinceKnownNewMoon = jDaysSinceKnownNewMoon / 29.53;
			var fractionOfCycleSinceLastNewMoon = newMoonsSinceKnownNewMoon % 1;
			return (float)(360 * fractionOfCycleSinceLastNewMoon);
		}

		double ToJulianDays(DateTime now)
        {
			var fractionOfDaySinceMidnight = now.Subtract(new DateTime(now.Year, now.Month, now.Day)).TotalHours / 24;
			int Y = now.Year, M = now.Month, D = now.Day;
			int A = Y / 100;
			int B = A / 4;
			int C = 2 - A + B;
			int E = (int)(365.25 * (Y + 4716));
			int F = (int)(30.6001 * (M + 1));
			return C + D + E + F - 1524.5 + fractionOfDaySinceMidnight;
		}
	}
}
