// Copyright (c) 2014 StagPoint Consulting
		
using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Object Pooling/Pooled Object" )]
	public class PooledObject : MonoBehaviour
	{

		#region Events

		public delegate void SpawnEventHandler( GameObject instance );

		public event SpawnEventHandler Spawned;
		public event SpawnEventHandler Despawned;

		#endregion

		#region Properties

		public PoolManager.ObjectPool Pool { get; set; }

		#endregion

		#region Unity events

		void Awake() { }
		void OnEnable() { }
		void OnDisable() { }
		void OnDestroy() { }

		#endregion

		#region Public methods

		public void Despawn()
		{
			this.Pool.Despawn( gameObject );
		}

		#endregion

		#region Utility methods

		internal void OnSpawned()
		{
			if( Spawned != null )
			{
				Spawned( this.gameObject );
			}
			SendMessage( "OnObjectSpawned", SendMessageOptions.DontRequireReceiver );
		}

		internal void OnDespawned()
		{
			if( Despawned != null )
			{
				Despawned( this.gameObject );
			}
			SendMessage( "OnObjectDespawned", SendMessageOptions.DontRequireReceiver );
		}

		#endregion

	}

}