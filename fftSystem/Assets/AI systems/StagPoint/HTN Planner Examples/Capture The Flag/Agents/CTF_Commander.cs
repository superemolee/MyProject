// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning.Components;
using StagPoint.Examples;

using Random = UnityEngine.Random;

[RequireComponent( typeof( TaskNetworkPlanner ) )]
public class CTF_Commander : MonoBehaviour
{

	#region Public serialized fields

	[BlackboardVariable]
	public TeamType Team = TeamType.RedTeam;

	[DoNotDatabind]
	public GameObject BotPrefab;

	#endregion

	#region Public non-serialized fields

	[NonSerialized]
	[DoNotDatabind]
	public Blackboard Blackboard;

	#endregion

	#region Data-bound blackboard variables

	[BlackboardVariable]
	[NonSerialized]
	public float RespawnTimer = 0f;

	[NonSerialized]
	[BlackboardVariable]
	public TeamFlagPoint TeamFlag;

	[NonSerialized]
	[BlackboardVariable]
	public TeamGoalPoint TeamGoal;

	[NonSerialized]
	[BlackboardVariable]
	public TeamFlagPoint EnemyFlag;

	[NonSerialized]
	[BlackboardVariable]
	public TeamGoalPoint EnemyGoal;

	[NonSerialized]
	[BlackboardVariable]
	public CTF_Commander EnemySpawn;

	[BlackboardVariable]
	public int NumDefenders
	{
		get
		{
			return NumSoldiersAssignedTo( OrderType.Defend );
		}
	}

	[BlackboardVariable]
	public int NumAmbushers
	{
		get
		{
			return NumSoldiersAssignedTo( OrderType.HoldPosition );
		}
	}

	[BlackboardVariable]
	public int NumNotAssigned
	{
		get
		{
			return NumSoldiersAssignedTo( OrderType.Explore );
		}
	}

	[BlackboardVariable]
	public int NumAttackers
	{
		get
		{
			return NumSoldiersAssignedTo( OrderType.Attack );
		}
	}

	[BlackboardVariable]
	public int NumEscorts
	{
		get
		{
			return NumSoldiersAssignedTo( OrderType.Escort );
		}
	}

	[BlackboardVariable]
	public int NumSoldiers
	{
		get
		{
			return teamAgents.Count;
		}
	}

	[BlackboardVariable]
	public Vector3 Position
	{
		get { return transform.position; }
	}

	#endregion

	#region Private runtime variables

	private TaskNetworkPlanner planner;
	private TaskNetworkPlan currentPlan;

	private ListEx<CTF_Agent> teamAgents = new ListEx<CTF_Agent>();

	#endregion

	#region MonoBehaviour events

	void Start()
	{

		if( BotPrefab == null )
		{
			DebugMessages.LogError( "Bot Prefab is not set" );
			this.enabled = false;
			return;
		}

		BotPrefab.SetActive( false );

		// Obtain a reference to the planner and runtime Blackboard
		planner = GetComponent<TaskNetworkPlanner>();
		Blackboard = planner.RuntimeBlackboard;

		// Data bind blackboard variables
		Blackboard.DataBind( this, BlackboardBindingMode.AttributeControlled );

	}

	void OnEnable()
	{

		this.EnemySpawn = FindObjectsOfType<CTF_Commander>().Where( x => x.Team != this.Team ).FirstOrDefault();

		var flagPoints = FindObjectsOfType<TeamFlagPoint>();
		var goalPoints = FindObjectsOfType<TeamGoalPoint>();

		this.TeamFlag = flagPoints.Where( x => x.Team == this.Team ).FirstOrDefault();
		this.EnemyFlag = flagPoints.Where( x => x.Team != this.Team ).FirstOrDefault();

		this.TeamGoal = goalPoints.Where( x => x.Team == this.Team ).FirstOrDefault();
		this.EnemyGoal = goalPoints.Where( x => x.Team != this.Team ).FirstOrDefault();

	}

	void Update()
	{
		this.RespawnTimer = Mathf.Max( 0, this.RespawnTimer - Time.deltaTime );
		teamAgents.RemoveAll( x => x == null || x.gameObject == null );
	}

	#endregion

	#region Methods called by bots

	public int NumSoldiersAssignedTo( OrderType orders )
	{

		int count = 0;
		var bots = teamAgents;

		for( int i = 0; i < bots.Count; i++ )
		{
			var bot = bots[ i ];
			if( bot.Orders == orders )
				count += 1;
		}

		return count;

	}

	public void EnemyKilled( CTF_Agent agent )
	{
		SendMessage( "OnEnemyKilled", null, SendMessageOptions.DontRequireReceiver );
	}

	public void SoldierKilled( CTF_Agent agent )
	{

		teamAgents.Remove( agent );

		SendMessage( "OnSoldierKilled", null, SendMessageOptions.DontRequireReceiver );

		if( !agent.HasFlag )
			return;

		var escorts = teamAgents
			.Where( x => x.Orders == OrderType.Escort )
			.OrderBy( x => Vector3.Distance( x.Position, EnemyFlag.Position ) )
			.ToList();

		if( escorts.Count == 0 )
			return;

		escorts[ 0 ].Orders = OrderType.Attack;

		for( int i = 1; i < escorts.Count; i++ )
		{
			escorts[ i ].FollowTarget = escorts[ 0 ];
			escorts[ i ].Orders = OrderType.Escort;
		}

	}

	public void TeamFlagCaptured( CTF_Agent enemy )
	{

		var agents = teamAgents
			.Where( x =>
				x.Orders == OrderType.HoldPosition ||
				x.Orders == OrderType.Explore ||
				x.Orders == OrderType.Defend
			);

		foreach( var agent in agents )
		{
			agent.Orders = OrderType.Pursue;
			agent.FollowTarget = enemy;
		}

	}

	public void CapturedEnemyFlag( CTF_Agent agent )
	{

		// TESTING CODE: Bots that capture the enemy flag are (partially) healed.
		var health = agent.GetComponent<BotHealthComponent>();
		health.CurrentHealth = Mathf.Max( health.CurrentHealth, health.MaxHealth / 3 );

		SendMessage( "OnEnemyFlagCaptured", null, SendMessageOptions.DontRequireReceiver );

		DebugMessages.LogImportant( string.Format( "{0} captured the enemy flag", this.Team ) );

		// It is possible someone other than an attacker captured the flag...
		agent.Orders = OrderType.Attack;

		EnemySpawn.TeamFlagCaptured( agent );

	}

	public void DeliveredEnemyFlag( CTF_Agent agent )
	{

		SendMessage( "OnGoalScored", null, SendMessageOptions.DontRequireReceiver );

		DebugMessages.LogImportant( string.Format( "{0} delivered the enemy flag", this.Team ) );

		EnemyFlag.ResetFlag();

	}

	public void TeamFlagReset()
	{

		var agents = teamAgents.Where( x => x.Orders == OrderType.HoldPosition || x.Orders == OrderType.Pursue );

		foreach( var agent in agents )
		{
			agent.Orders = OrderType.Explore;
		}

	}

	public Vector3 GetNavigationTarget( CTF_Agent agent )
	{

		var waypoints = WaypointGenerator.Instance.waypoints
			.Where( x =>
				Vector3.Distance( x, EnemySpawn.Position ) > 10 &&
				Vector3.Distance( x, this.Position ) > 25 &&
				Vector3.Distance( x, agent.Position ) > 25
			).ToList();

		var index = Random.Range( 0, waypoints.Count - 1 );

		return waypoints[ index ];

	}

	#endregion

	#region Action/Operator methods

	public TaskStatus AssignPursuit()
	{

		var target = TeamFlag.FlagBearer;
		if( target == null )
			return TaskStatus.Failed;

		var agent = teamAgents
			.Where( x => !x.HasFlag )
			.OrderBy( x => Vector3.Distance( TeamFlag.transform.position, x.transform.position ) )
			.FirstOrDefault();

		if( agent != null )
		{

			agent.Orders = OrderType.Pursue;
			agent.FollowTarget = target;

			return TaskStatus.Succeeded;

		}

		return TaskStatus.Failed;

	}

	public TaskStatus AssignDefender()
	{

		var defender = teamAgents
			.OrderBy( x => Vector3.Distance( TeamFlag.transform.position, x.transform.position ) )
			.FirstOrDefault();

		if( defender != null )
		{
			defender.Orders = OrderType.Defend;
			return TaskStatus.Succeeded;
		}

		return TaskStatus.Failed;

	}

	public TaskStatus AssignAmbusher()
	{

		if( CoverPointGenerator.Instance == null )
			return TaskStatus.Failed;

		var flagTooCloseToGoal = ( Vector3.Distance( TeamFlag.Position, EnemyGoal.Position ) < 7f );
		var defensePosition = NumAmbushers < 2 || flagTooCloseToGoal ? TeamFlag.Position : EnemyGoal.Position;

		var defender = teamAgents
			.Where( x => x.Orders == OrderType.Explore )
			.OrderBy( x => Vector3.Distance( defensePosition, x.Position ) )
			.FirstOrDefault();

		if( flagTooCloseToGoal )
		{

			defender.Orders = OrderType.HoldPosition;
			defender.Blackboard.SetValue( "NextDestination", defensePosition + Random.insideUnitSphere );

			return TaskStatus.Succeeded;

		}

		var nodes = CoverPointGenerator.Instance
			.FindAmbushNodes( EnemyFlag.Position, defensePosition, 15, -10, 5, 0x01 )
			.OrderBy( x => Vector3.Distance( defensePosition, x.position ) )
			.ToList();

		if( nodes.Count > 0 )
		{

			var maxIndex = Mathf.Min( 5, nodes.Count - 1 );
			var node = nodes[ Random.Range( 0, maxIndex ) ];

			node.ReserveFor( defender );

			defensePosition = node.position;

		}

		defender.Orders = OrderType.HoldPosition;
		defender.Blackboard.SetValue( "NextDestination", defensePosition );

		return TaskStatus.Succeeded;

	}

	public TaskStatus AssignAttacker()
	{

		var attacker = teamAgents
			.Where( x => x.Orders != OrderType.Defend )
			.OrderBy( x => Vector3.Distance( EnemyFlag.transform.position, x.transform.position ) )
			.FirstOrDefault();

		if( attacker != null )
		{

			var debug = attacker.GetComponent<RuntimePlannerInfo>();
			if( debug != null )
			{
				debug.showDebugGUI = true;
				debug.showWhenSelected = false;
			}

			attacker.Orders = OrderType.Attack;
			return TaskStatus.Succeeded;

		}

		return TaskStatus.Failed;

	}

	public TaskStatus AssignEscort()
	{

		var attacker = teamAgents.Where( x => x.Orders == OrderType.Attack ).FirstOrDefault();

		var escort = teamAgents
			.Where( x => x.Orders == OrderType.Explore )
			.OrderBy( x => Vector3.Distance( EnemyFlag.transform.position, x.transform.position ) )
			.FirstOrDefault();

		if( escort != null )
		{

			escort.Orders = OrderType.Escort;
			escort.FollowTarget = attacker;

			return TaskStatus.Succeeded;

		}

		return TaskStatus.Failed;

	}

	public IEnumerator Respawn()
	{
		StartCoroutine( spawnNewAgent() );
		yield return TaskStatus.Succeeded;
	}

	#endregion

	#region Private utility methods

	private IEnumerator spawnNewAgent()
	{

		var newBot = (GameObject)GameObject.Instantiate( BotPrefab, transform.position - Vector3.up * 3f, transform.rotation );
		newBot.SetActive( true );

		var agent = newBot.GetComponent<CTF_Agent>();
		agent.Commander = this;
		agent.Team = this.Team;
		agent.Orders = OrderType.Explore;

		teamAgents.Add( agent );

		var nav = newBot.GetComponent<NavMeshAgent>();
		nav.enabled = false;

		var planner = newBot.GetComponent<TaskNetworkPlanner>();

		// Do not allow agent's planner to update until it is fully spawned
		planner.enabled = false;

		var position = this.transform.position - Vector3.up;

		while( position.y < 1 )
		{
			position += Vector3.up * Time.deltaTime * 10;
			newBot.transform.position = position;
			yield return TaskStatus.Running;
		}

		nav.enabled = true;
		agent.enabled = true;
		planner.enabled = true;

	}

	#endregion

}
