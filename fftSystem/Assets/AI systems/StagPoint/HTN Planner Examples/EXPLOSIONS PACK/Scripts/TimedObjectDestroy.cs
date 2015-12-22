using UnityEngine;
using System.Collections;

using StagPoint.Examples;

public class TimedObjectDestroy : MonoBehaviour
{

	public float Delay = 1f;

	void OnEnable()
	{
		StartCoroutine( destroyAfterDelay() );
	}

	private IEnumerator destroyAfterDelay()
	{
		yield return new WaitForSeconds( Delay );
		PoolManager.Pool[ "explosion" ].Despawn( this.gameObject );
	}

}