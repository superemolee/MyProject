// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Bot Health" )]
	public class BotHealthComponent : MonoBehaviour
	{

		#region Public fields

		public int MaxHealth = 100;
		public int CurrentHealth = 100;
		public int RegenPerSecond = 10;

		public bool IsAlive = true;

		#endregion

		#region Private runtime variables

		private float accumulator = 0f;

		#endregion

		#region Monobehaviour events

		void Start()
		{
			this.IsAlive = true;
		}

		void Update()
		{

			if( !IsAlive )
				return;

			accumulator += Time.deltaTime;
			if( accumulator >= 1f )
			{
				accumulator = accumulator - 1f;
				Heal( RegenPerSecond );
			}

		}

#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			if( Application.isPlaying )
			{
				GUI.color = Color.red;
				UnityEditor.Handles.color = Color.red;
				UnityEditor.Handles.Label( transform.position + Vector3.up * 3, this.CurrentHealth.ToString() );
			}
		}
#endif

		#endregion

		#region Public methods

		public void Heal( int amount )
		{

			if( CurrentHealth >= MaxHealth )
				return;

			CurrentHealth = Mathf.Min( MaxHealth, CurrentHealth + RegenPerSecond );

			if( CurrentHealth == MaxHealth )
			{
				SendMessage( "OnHealed", this, SendMessageOptions.DontRequireReceiver );
			}

		}

		public void TakeDamage( int amount, MonoBehaviour damageSource )
		{

			if( amount <= 0 )
				return;

			this.accumulator = 0f;
			this.CurrentHealth = Mathf.Max( 0, CurrentHealth - amount );

			if( this.CurrentHealth == 0 )
			{
				this.IsAlive = false;
				SendMessage( "OnDeath", this, SendMessageOptions.DontRequireReceiver );
			}
			else
			{
				SendMessage( "OnDamaged", damageSource, SendMessageOptions.DontRequireReceiver );
			}

		}

		#endregion

	}

}
