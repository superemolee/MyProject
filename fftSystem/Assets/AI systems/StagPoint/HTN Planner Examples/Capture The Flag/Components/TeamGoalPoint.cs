// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Goal" )]
	public class TeamGoalPoint : MonoBehaviour
	{

		public TeamType Team = TeamType.RedTeam;

		public Vector3 Position
		{
			get { return transform.position; }
		}

		void OnTriggerEnter( Collider other )
		{

			var agent = other.GetComponent<CTF_Agent>();

			if( agent != null && agent.Team == this.Team )
			{
				agent.SendMessage( "OnDeliveredFlag", null, SendMessageOptions.RequireReceiver );
			}

		}

	}

}
