using UnityEngine;
using System.Collections;

public class ExplosionLight : MonoBehaviour
{
	public float LightDuration = 0.2f;
	public float LightIntensity = 2f;

	IEnumerator Start()
	{
		float div = 1f / LightDuration;

		float t = 0f;
		while( t < 1f )
		{
			t = Mathf.MoveTowards( t, 1f, Time.deltaTime * div );

			light.intensity = Mathf.Lerp( LightIntensity, 0f, t );

			yield return null;
		}

		light.enabled = false;
	}
}