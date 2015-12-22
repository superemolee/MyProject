// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;
using StagPoint.Examples;

[RequireComponent( typeof( TaskNetworkPlanner ) )]
[RequireComponent( typeof( VisualSensor ) )]
[RequireComponent( typeof( VisualAspect ) )]
[RequireComponent( typeof( BotHealthComponent ) )]
[RequireComponent( typeof( BotLaser ) )]
public class CTF_Agent : MonoBehaviour
{

	#region Public fields and properties

	[NonSerialized]
	public Blackboard Blackboard;

	public TeamType Team;

	#endregion

	#region Public data-bound blackboard variables

	[NonSerialized]
	[BlackboardVariable]
	public CTF_Commander Commander;

	[BlackboardVariable]
	[NonSerialized]
	public bool HasFlag;

	[BlackboardVariable]
	[NonSerialized]
	public float AlertLevel;

	[BlackboardVariable]
	[NonSerialized]
	public float AlertDistance = 5;

	[BlackboardVariable]
	[NonSerialized]
	public Vector3 AlertSource = Vector3.zero;

	[BlackboardVariable]
	public OrderType Orders
	{
		get { return this._orders; }
		set
		{

			this._orders = value;

			// When the bot is issued new orders, immediately abandon any existing 
			// plan. Otherwise, the bot will not switch to a lower-priority plan than
			// what is currently running.
			if( planner != null )
			{
				planner.ClearPlan();
			}

		}
	}

	[BlackboardVariable]
	[NonSerialized]
	public MonoBehaviour EnemyTarget;

	[BlackboardVariable]
	[NonSerialized]
	public CTF_Agent FollowTarget;

	[BlackboardVariable]
	public int CurrentHealth
	{
		get { return health.CurrentHealth; }
		set
		{
			health.CurrentHealth = Mathf.Min( value, health.MaxHealth );
		}
	}

	[BlackboardVariable]
	public Vector3 Position
	{
		get { return this.transform.position; }
	}

	[BlackboardVariable]
	[NonSerialized]
	public List<VisualTrackingInfo> DetectedBots = new List<VisualTrackingInfo>();

	#endregion

	#region Static variables

	public static List<CTF_Agent> ActiveBots = new List<CTF_Agent>();

	#endregion

	#region Private runtime variables

	private TaskNetworkPlanner planner;
	private VisualSensor sensor;
	private BotHealthComponent health;
	private NavMeshAgent nav;

	private OrderType _orders = OrderType.Idle;
	private Vector3 stuckLocation;
	private float stuckTimer;
	private float turnDirection = 1f;
	private float baseNavSpeed = 6f;
	private float baseNavAcceleration = 12f;

	#endregion

	#region Monobehaviour events

	void OnEnable()
	{

		ActiveBots.Add( this );

		// We want this bot to have a unique name, in order to facilitate debugging
		this.name = string.Format( "{0} ({1}) - {2}", this.GetType(), this.Team, Guid.NewGuid().ToString() );

		planner = GetComponent<TaskNetworkPlanner>();

		this.nav = GetComponent<NavMeshAgent>();
		this.baseNavSpeed = nav.speed;
		this.baseNavAcceleration = nav.acceleration;

		updateColor();

		health = GetComponent<BotHealthComponent>();

		// Obtain a reference to the runtime Blackboard instance
		this.Blackboard = planner.RuntimeBlackboard;

		// Data bind blackboard variables
		Blackboard.DataBind( this, BlackboardBindingMode.AttributeControlled );

		resetStuckCheck();

		GetComponent<VisualAspect>().AddTag( this.Team.ToString() );
		this.sensor = GetComponent<VisualSensor>();

	}

	void OnDisable()
	{
		ActiveBots.Remove( this );
	}

	void Update()
	{
		this.AlertLevel = Mathf.Max( this.AlertLevel - Time.deltaTime, 0 );
	}

	void OnGUI()
	{

		var position = this.transform.position;
		position.y = 0.05f;

		if( HasFlag )
		{

			var color = this.Team == TeamType.RedTeam ? Color.red : Color.blue;
			color.a = 0.5f;

			DebugRender.DrawSolidDisc( position, Vector3.up, 0.6f, color );

		}
		else
		{

			var debugColor = Color.black;

			switch( Orders )
			{
				case OrderType.Attack:
					debugColor = Color.green;
					break;
				case OrderType.Defend:
					debugColor = Color.yellow;
					break;
				case OrderType.Explore:
					debugColor = Color.grey;
					break;
				case OrderType.Pursue:
					debugColor = Color.magenta;
					break;
				case OrderType.Escort:
					debugColor = Color.cyan;
					break;
				case OrderType.HoldPosition:
					debugColor = new Color( 1, 0.4f, 0 );
					break;
			}

			DebugRender.DrawSolidDisc( position, Vector3.up, 0.75f, debugColor );

		}

	}

	#endregion

	#region SendMessage() targets

	void Alert( Vector3 source )
	{
		this.AlertLevel = Mathf.Max( this.AlertLevel, 1f );
		this.AlertSource = source;
	}

	void OnAttacked( CTF_Agent attacker )
	{

		this.EnemyTarget = attacker;

		this.AlertLevel = 5f;
		this.AlertSource = attacker.transform.position;

		var activeAlertDistance = this.AlertDistance;
		if( this.Orders == OrderType.Defend || this.Orders == OrderType.Attack )
			activeAlertDistance *= 2;

		for( int i = 0; i < ActiveBots.Count; i++ )
		{

			var bot = ActiveBots[ i ];

			if( bot.Team != this.Team )
				continue;

			var distance = distance2D( this.transform.position, bot.transform.position );
			if( distance <= activeAlertDistance )
			{
				bot.Alert( AlertSource );
			}

		}

	}

	void OnDamaged( MonoBehaviour damageSource )
	{
		if( damageSource != null && damageSource.gameObject != null )
		{
			this.AlertLevel = Mathf.Max( this.AlertLevel, 1f );
			this.AlertSource = damageSource.transform.position;
		}
	}

	void OnDeath( BotHealthComponent health )
	{

		Commander.SoldierKilled( this );

		var explosion = PoolManager.Pool[ "explosion" ].Spawn( false );
		explosion.hideFlags = HideFlags.HideAndDontSave;
		explosion.transform.position = this.Position;
		explosion.SetActive( true );

		Destroy( this.gameObject );

	}

	void OnTargetDestroyed( BotHealthComponent health )
	{
		Commander.EnemyKilled( this );
	}

	void OnGotFlag()
	{

		if( !this.HasFlag )
		{

			this.HasFlag = true;
			updateColor();

			var visual = GetComponent<VisualAspect>();
			if( visual != null )
			{
				visual.AddTag( "HasFlag" );
			}

			Commander.CapturedEnemyFlag( this );

		}

	}

	void OnDeliveredFlag()
	{

		if( this.HasFlag )
		{

			this.HasFlag = false;
			updateColor();

			var visual = GetComponent<VisualAspect>();
			if( visual != null )
			{
				visual.RemoveTag( "HasFlag" );
			}

			Commander.DeliveredEnemyFlag( this );

		}

	}

	void OnVisibleObjectLost( VisualTrackingInfo item )
	{

		var aspect = item.Aspect;

		if( aspect.HasTag( "bot" ) )
		{
			DetectedBots.Remove( item );
		}

	}

	void OnVisibleObjectDetected( VisualTrackingInfo item )
	{

		if( Commander == null )
			return;

		var aspect = item.Aspect;

		if( aspect.HasTag( "bot" ) )
		{
			DetectedBots.Add( item );
			return;
		}

	}

	#endregion

	#region Planner conditions

	public bool IsFacing( Blackboard blackboard, [ScriptParameter] MonoBehaviour target, float fieldOfView )
	{

		if( target == null || target.gameObject == null )
			return false;

		var dirToTarget = target.transform.position - this.transform.position;
		var angleToTarget = Vector3.Angle( transform.forward, dirToTarget );

		return angleToTarget <= fieldOfView * 0.5f;

	}

	public bool WithinRange( Blackboard blackboard, [ScriptParameter] MonoBehaviour target, float maxDistance )
	{

		var myPosition = transform.position;
		var targetPosition = target.transform.position;

		return distance2D( myPosition, targetPosition ) <= maxDistance;

	}

	public bool CanSeeObject( Blackboard blackboard, [ScriptParameter] MonoBehaviour targetObject )
	{

		if( targetObject == null )
			return false;

		return sensor.VisibleItems.Any( x => x.IsVisible && x.GameObject == targetObject.gameObject );

	}

	public bool IsBotStuck( Blackboard blackboard, [DefaultValue( 1f )] float timeout )
	{

		var myPosition = transform.position;

		// NOTE: If you are going to use a distance check to determine whether the 
		// bot is stuck, you must also make sure that the NavMeshAgent.stoppingDistance
		// value is not larger than the distance you choose to detect when the bot
		// has become "stuck".

		if( distance2D( myPosition, stuckLocation ) < 1f )
		{
			stuckTimer += Time.deltaTime;
			if( stuckTimer > timeout )
			{
				Debug.LogError( "Bot is stuck: " + this.name );
				health.TakeDamage( int.MaxValue, null );
				return true;
			}
		}
		else
		{
			resetStuckCheck();
		}

		return false;

	}

	public bool CanSeeEnemyBot( Blackboard blackboard, [DefaultValue( 60f )] float fieldOfView )
	{

		fieldOfView = Mathf.Max( 1f, fieldOfView );

		for( int i = 0; i < DetectedBots.Count; i++ )
		{

			var trackingInfo = DetectedBots[ i ];
			if( !trackingInfo.IsVisible || trackingInfo.Angle > fieldOfView )
				continue;

			var bot = trackingInfo.Aspect.GetComponent<CTF_Agent>();
			if( bot.Team != this.Team )
				return true;

		}

		return false;

	}

	#endregion

	#region Planner actions

	[Operator( "Find Cover Node", "Finds the nearest cover node that provides cover from the position passed in, and places that value in the NextDestination blackboard variable" )]
	public TaskStatus FindCover( [ScriptParameter] Vector3 threatPosition )
	{

		var cover =
			CoverPointGenerator.Instance.FindCoverNodes( threatPosition, this.Position, 15, 0, 5, 0x01 )
			.OrderBy( x => distance2D( x.position, this.Position ) )
			.FirstOrDefault();

		if( cover == null )
			return TaskStatus.Failed;

		cover.ReserveFor( this );

		Blackboard.SetValue( "NextDestination", cover.position );

		return TaskStatus.Succeeded;

	}

	[Operator( "Find Ambush", "Finds the nearest cover node that can work as an ambush point for the position passed in, and places that value in the NextDestination blackboard variable" )]
	public TaskStatus FindAmbush( [ScriptParameter] Vector3 threatPosition )
	{

		var cover =
			CoverPointGenerator.Instance.FindAmbushNodes( threatPosition, this.Position, 15, -10, 0, 0x01 )
			.OrderBy( x => distance2D( x.position, this.Position ) )
			.FirstOrDefault();

		if( cover == null )
			return TaskStatus.Failed;

		cover.ReserveFor( this );

		Blackboard.SetValue( "NextDestination", cover.position );

		return TaskStatus.Succeeded;

	}

	[Operator( "Self Destruct", "Causes the bot to explode" )]
	TaskStatus SelfDestruct()
	{
		health.TakeDamage( int.MaxValue, this );
		return TaskStatus.Succeeded;
	}

	[Operator( "Go To Object", "Causes the agent to navigate to the object's position. If the object moves, this action will cause the agent to follow the object." )]
	public IEnumerator GoToObject( [ScriptParameter] MonoBehaviour target, [DefaultValue( 1f )] float speedMultiplier )
	{

		// Note: I would never abuse Unity's navigation in production for a task like this, but it 
		// didn't seem prudent to further complicate the example by also adding better navigation.

		var targetPosition = Vector3.one * float.MaxValue;

		nav.autoBraking = false;
		nav.speed = baseNavSpeed * speedMultiplier;
		nav.acceleration = baseNavAcceleration * speedMultiplier;

		while( target != null && target.gameObject != null )
		{

			// Need to periodically re-path. We'll do that whenever the object changes position
			if( distance2D( target.transform.position, targetPosition ) > 1f )
			{
				targetPosition = target.transform.position;
				nav.destination = restrictToNavmesh( targetPosition );
			}

			// If we've reached (close enough to) the destination, return success
			if( distance2D( transform.position, target.transform.position ) <= 1f )
			{
				nav.autoBraking = true;
				yield return TaskStatus.Succeeded;
			}

			yield return TaskStatus.Running;

		}

		yield return TaskStatus.Failed;


	}

	[Operator( "Pursue", "Causes the agent to follow the specified target" )]
	public IEnumerator Pursue( [ScriptParameter] MonoBehaviour target, [DefaultValue( 1f )] float speedMultiplier )
	{

		// Note: I would never abuse Unity's navigation in production for a task like this, but it 
		// didn't seem prudent to further complicate the example by also adding better navigation.

		resetStuckCheck();

		if( target == null || target.gameObject == null )
			yield return TaskStatus.Failed;

		nav.speed = baseNavSpeed * speedMultiplier;
		nav.acceleration = baseNavAcceleration * speedMultiplier;

		var targetTransform = target.transform;
		var followPosition = Vector3.one * float.MaxValue;

		while( target != null && target.gameObject != null )
		{

			if( IsBotStuck( this.Blackboard, 3f ) )
				yield return TaskStatus.Failed;

			if( distance2D( followPosition, targetTransform.position ) > 2f )
			{
				followPosition = nav.destination = restrictToNavmesh( targetTransform.position );
			}

			yield return TaskStatus.Running;

		}

		yield return TaskStatus.Succeeded;


	}

	[Operator( "Select Enemy Target", "Evaluates all visible enemies and determines which one to fire at. Will set the EnemyTarget and AlertSource blackboard variables with the bot and bot position, respectively." )]
	public TaskStatus SelectEnemyTarget()
	{

		var maxAngle = Mathf.Max( 1f, sensor.FOV * 0.5f );

		DetectedBots.RemoveAll( x => x == null || !x.IsVisible );

		var target = DetectedBots
			.Select( x => new { bot = x.Aspect.GetComponent<CTF_Agent>(), info = x } )
			.Where( x => x.bot.Team != this.Team && x.info.Angle <= maxAngle )
			.OrderBy( x => scoreEnemyBot( x.info, x.bot ) )
			.FirstOrDefault();

		if( target == null )
			return TaskStatus.Failed;

		this.EnemyTarget = target.bot;
		this.AlertSource = target.info.Position;

		return TaskStatus.Succeeded;

	}

	public IEnumerator FaceObject( [ScriptParameter] MonoBehaviour target, [DefaultValue( 2f )] float timeout, [DefaultValue( 1f )] float toleranceInDegrees )
	{

		var myPosition = transform.position;

		if( timeout <= float.Epsilon )
			yield return TaskStatus.Failed;
		else
			timeout += Time.realtimeSinceStartup;

		while( Time.realtimeSinceStartup < timeout )
		{

			if( target == null || target.gameObject == null )
				yield return TaskStatus.Failed;

			var targetPosition = target.transform.position;
			var vectorToTarget = ( targetPosition - myPosition ).normalized;

			var difference = Vector3.Angle( transform.forward, vectorToTarget );
			if( difference <= toleranceInDegrees )
			{
				transform.LookAt( target.transform );
				yield return TaskStatus.Succeeded;
			}

			var isTargetVisible = DetectedBots.Any( x => x.IsVisible && x.GameObject == target.gameObject );
			if( !isTargetVisible )
			{
				yield return TaskStatus.Failed;
			}

			// Avoid the "Look rotation viewing vector is zero" message in the console
			if( vectorToTarget.sqrMagnitude <= float.Epsilon )
				continue;

			var lookAt = Quaternion.LookRotation( vectorToTarget );
			transform.rotation = Quaternion.Slerp( transform.rotation, lookAt, Time.deltaTime * 28f );

			yield return TaskStatus.Running;

		}

		yield return TaskStatus.Failed;

	}

	[NotInterruptable]
	public IEnumerator UseWeapon()
	{

		// Call the BotLaser.FireLaser() method
		var weapon = GetComponent<BotLaser>();
		if( weapon == null )
		{
			DebugMessages.LogError( "No weapon to fire: " + this.name );
			yield return TaskStatus.Failed;
		}

		weapon.laserColor = ( this.Team == TeamType.RedTeam ) ? Color.red : Color.blue;

		var target = weapon.FireLaser( 0.25f );
		if( target != null )
		{
			target.SendMessage( "OnAttacked", this, SendMessageOptions.DontRequireReceiver );
			Alert( target.transform.position );
		}

		// Wait for the laser to be drawn and hidden
		var timeout = Time.realtimeSinceStartup + 0.33f;
		while( Time.realtimeSinceStartup < timeout )
		{
			yield return TaskStatus.Running;
		}

		yield return TaskStatus.Succeeded;

	}

	public TaskStatus RequestNavigationTarget()
	{

		if( this.Blackboard == null )
			throw new NullReferenceException( "Blackboard is null" );

		if( this.Commander == null )
			throw new NullReferenceException( "Commander is null" );

		var waypoint = this.Commander.GetNavigationTarget( this );

		Blackboard.SetValue( "NextDestination", waypoint );

		return TaskStatus.Succeeded;

	}

	public IEnumerator DoVisualScan( [DefaultValue( 360f )] float degrees )
	{

		Stop();

		degrees *= 0.5f;

		var myPosition = transform.position;
		var wasFacingWall = Physics.Raycast( myPosition, transform.forward, 2f, 0x01 );

		float timeout = Time.realtimeSinceStartup + 2f;
		while( Time.realtimeSinceStartup < timeout )
		{

			transform.eulerAngles += new Vector3( 0, degrees * Time.deltaTime * turnDirection, 0 );

			var isFacingWall = Physics.Raycast( myPosition, transform.forward, 2f, 0x01 );
			if( isFacingWall && !wasFacingWall )
				turnDirection *= -1;

			wasFacingWall = isFacingWall;

			yield return TaskStatus.Running;

		}

		yield return TaskStatus.Succeeded;

	}

	public IEnumerator NavigateToPosition( [ScriptParameter] Vector3 position, [DefaultValue( 1f )] float speedMultiplier )
	{

		position = restrictToNavmesh( position );

		// Since this is just a flat single-height level, make the destination the same altitude as the bot
		position.y = transform.position.y;

		// Inform the NavMeshAgent of the destination and set speed parameters
		nav.autoBraking = false;
		nav.destination = position;
		nav.speed = baseNavSpeed * speedMultiplier;
		nav.acceleration = baseNavAcceleration * speedMultiplier;

		resetStuckCheck();

		while( true )
		{

			yield return TaskStatus.Running;

			if( distance2D( transform.position, position ) <= 0.5f )
			{
				nav.autoBraking = true;
				yield return TaskStatus.Succeeded;
			}

			if( IsBotStuck( this.Blackboard, 3f ) )
				yield return TaskStatus.Failed;

		}

	}

	public TaskStatus Stop()
	{

		nav.autoBraking = true;
		nav.destination = transform.position;
		nav.Stop();

		return TaskStatus.Succeeded;

	}

	#endregion

	#region Private utility methods

	private float scoreEnemyBot( VisualTrackingInfo info, CTF_Agent enemy )
	{

		if( enemy.HasFlag )
			return -float.MaxValue;

		if( enemy == EnemyTarget )
			return -float.MaxValue * 0.5f;

		return info.Angle * Mathf.Deg2Rad;

	}

	private float distance2D( Vector3 lhs, Vector3 rhs )
	{
		rhs.y = lhs.y;
		return Vector3.Distance( lhs, rhs );
	}

	private Vector3 restrictToNavmesh( Vector3 position )
	{

		NavMeshHit navHit;
		NavMesh.SamplePosition( position, out navHit, 10, 0xff );

		return navHit.position;

	}

	private void resetStuckCheck()
	{
		stuckLocation = transform.position;
		stuckTimer = 0f;
	}

	private void updateColor()
	{

		var color = Color.white;

		if( Team == TeamType.RedTeam )
			color = Color.red;
		else
			color = Color.blue;

		if( !HasFlag )
			color += new Color( 0.7f, 0.7f, 0.7f );

		var mat = GetComponent<MeshRenderer>().material;
		mat.color = color;

	}

	#endregion

}
