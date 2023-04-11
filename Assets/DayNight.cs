using UnityEngine;
public sealed class DayNight : MonoBehaviour
{
	private static readonly int BLEND = Shader.PropertyToID("_Blend");
	[Range(0, 24)]
	public float timeOfDay = 12;
	public  float     dayLength = 180f; // seconds
	public  float     angle;
	public  Transform reference;
	public  Material  daySkybox;
	public  Material  nightSkybox;
	public  int       sunRise = 6;
	public  int       sunSet  = 18;
	private Light     sun;
	private void Start()
	{
		sun                   = GetComponent<Light>();
		timeOfDay             = 12;
		RenderSettings.skybox = daySkybox;
		RenderSettings.sun    = sun;
	}

	private void FixedUpdate()
	{
		timeOfDay += Time.fixedDeltaTime / dayLength * 24;
		timeOfDay %= 24;
		// set the intensity of the sun based on the time of day
		float _intensity = 1 - Mathf.Abs(timeOfDay / 12 - 1) + 0.2f;
		sun.intensity = _intensity;
		// set the sun color based on the time of day
		sun.color = new(_intensity, _intensity, _intensity);
		// set the sun position 
		// keep the sun at a distance of 100 units from the reference
		const int _radius    = 100;
		float     _timeOfDay = (timeOfDay - 6) / 24 * 360 * Mathf.Deg2Rad;
		int       _x         = (int)(_radius * Mathf.Cos(_timeOfDay) * Mathf.Cos(angle * Mathf.Deg2Rad));
		int       _y         = (int)(_radius * Mathf.Sin(_timeOfDay) * Mathf.Cos(angle * Mathf.Deg2Rad));
		int       _z         = (int)(_radius * Mathf.Sin(angle                         * Mathf.Deg2Rad));
		sun.transform.position = new Vector3(_x, _y, _z) + reference.position;
		// set the sun rotation
		sun.transform.LookAt(Vector3.zero);
		// set the skybox based on the time of day
		if (timeOfDay >= sunRise && timeOfDay <= sunSet)
			RenderSettings.skybox = daySkybox;
		else
			RenderSettings.skybox = nightSkybox;

		// set blend factor for the skybox
		RenderSettings.skybox.SetFloat(BLEND, Mathf.Abs(timeOfDay / 12 - 1));

	}
}