using UnityEngine;
public class DayNight : MonoBehaviour
{
	[Range(0, 24)]
	public float timeOfDay = 12;
	public  float dayLength = 180f; // seconds
	public  float angle;
	private Light sun;

	private void Start()
	{
		sun = GetComponent<Light>();
	}

	private void Update()
	{
		timeOfDay                   += Time.deltaTime / dayLength * 24;
		timeOfDay                   %= 24;
		sun.transform.localRotation =  Quaternion.Euler(timeOfDay / 24 * 360, angle, 0);
		// set the intensity of the sun based on the time of day
		float _intensity;
		_intensity    = 1 - Mathf.Abs(timeOfDay / 12 - 1) + 0.2f;
		sun.intensity = _intensity;
		// set the ambient light based on the time of day
		RenderSettings.ambientLight = new(_intensity, _intensity, _intensity);
		// set the fog color based on the time of day
		RenderSettings.fogColor = new(_intensity, _intensity, _intensity);
		// set the fog density based on the time of day
		RenderSettings.fogDensity = 1 - _intensity;
		// set the sun color based on the time of day
		sun.color = new(_intensity, _intensity, _intensity);
		// set the sun position based on the time of day
		sun.transform.position = new(0, 0, 0);
	}
}