// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Visual Sensor" )]
	public class VisualSensor : MonoBehaviour
	{

		#region Public fields

		/// <summary>
		/// Gets or sets the sensor's field of view
		/// </summary>
		public float FOV = 90;

		/// <summary>
		/// Gets or sets the maximum sensor distance
		/// </summary>
		public float MaxDistance = 1000;

		/// <summary>
		/// Gets or sets the offset from which to begin the raycast. This can be used
		/// to put the visual sensor at "eye" level, etc.
		/// </summary>
		public Vector3 RaycastOffset = Vector3.up + Vector3.forward * 0.5f;

		/// <summary>
		/// Gets or sets which layers are visible to the sensor. Only VisualAspect instances
		/// on GameObjects whose layer is included in this mask will be detected.
		/// </summary>
		public LayerMask ViewLayers = 0x00;

		/// <summary>
		/// Gets or sets which layers will be used when performing raycasts, with all other 
		/// layers being ignored. By default
		/// </summary>
		public LayerMask RaycastLayers = 0x01;

		#endregion

		#region Public properties

		/// <summary>
		/// Returns a list of VisualTrackingInfo references corresponding to the objects
		/// that are currently available to the sensor.
		/// </summary>
		public List<VisualTrackingInfo> VisibleItems
		{
			get { return this.trackedItems; }
		}

		#endregion

		#region Private runtime variables

		private List<VisualTrackingInfo> trackedItems = new List<VisualTrackingInfo>();

		private Vector3 forwardDirection;
		private Vector3 worldPosition;
		private Vector3 raycastPosition;

		private VisualAspect myVisualAspect;

		#endregion

		#region Monobehaviour events

		void Start()
		{

			// If the GameObject has both a VisualSensor and a VisualAspect component, it will need
			// to ignore its own VisualAspect.
			myVisualAspect = GetComponent<VisualAspect>();

		}

		void OnDisable()
		{
			trackedItems.Clear();
		}

		void FixedUpdate()
		{

			var transform = this.transform;

			forwardDirection = transform.forward;
			worldPosition = transform.position;
			raycastPosition = worldPosition + transform.TransformDirection( RaycastOffset );

			updateTrackedItems();
			detectNewItems();

		}

		void OnDrawGizmosSelected()
		{

			var matrix = Gizmos.matrix;

			Gizmos.color = trackedItems.Count == 0 ? Color.yellow : Color.red;

			var offset = transform.TransformDirection( RaycastOffset );
			var raycastOrigin = transform.position + offset;

			Gizmos.matrix = Matrix4x4.TRS( raycastOrigin, transform.rotation, Vector3.one );
			Gizmos.DrawFrustum( Vector3.zero, this.FOV, this.MaxDistance, 0f, 1f );

			Gizmos.matrix = matrix;

			for( int i = 0; i < trackedItems.Count; i++ )
			{

				var item = trackedItems[ i ];
				var direction = item.Position - raycastOrigin;

				Gizmos.color = item.IsVisible ? Color.magenta : new Color( 0, 0, 0, 0.15f );

				Gizmos.DrawRay( raycastOrigin, direction );

			}

			Gizmos.color = Color.white;

		}

		#endregion

		public VisualTrackingInfo GetTrackingInfo( GameObject target )
		{

			for( int i = 0; i < trackedItems.Count; i++ )
			{

				var info = trackedItems[ i ];

				if( info.GameObject != null && info.GameObject == target )
				{
					return info;
				}

			}

			return null;

		}

		#region Private utility methods

		private void detectNewItems()
		{

			var activeItems = VisualAspect.ActiveItems;
			var itemCount = activeItems.Count;

			for( int i = 0; i < itemCount; i++ )
			{

				var aspect = activeItems[ i ];
				if( aspect == myVisualAspect )
					continue;

				var itemLayer = 1 << aspect.gameObject.layer;
				if( ( itemLayer & ViewLayers.value ) == 0x00 )
					continue;

				var trackingInfo = findTrackingInfo( aspect );
				if( trackingInfo != null )
					continue;

				var angle = 0f;
				var distance = 0f;

				if( isAspectVisible( aspect, ref angle, ref distance ) )
				{

					var itemTransform = aspect.transform;
					var itemPosition = itemTransform.position;

					var newTrackingInfo = new VisualTrackingInfo( aspect )
					{
						IsVisible = true,
						Position = itemPosition,
						Angle = angle,
						Distance = distance,
						Velocity = aspect.Velocity,
						Facing = itemTransform.forward
					};

					trackedItems.Add( newTrackingInfo );

					SendMessage( "OnVisibleObjectDetected", newTrackingInfo, SendMessageOptions.DontRequireReceiver );

				}

			}

		}

		private void updateTrackedItems()
		{

			for( int i = 0; i < trackedItems.Count; )
			{

				var item = trackedItems[ i ];

				if( item == null )
				{
					Debug.LogError( "Item in collection was NULL" );
					trackedItems.RemoveAt( i );
					continue;
				}

				// Remove any tracked items whose corresponding GameObject is destroyed, or whose
				// corresponding VisualAspect instance has been destroyed or disabled.
				if( item.Aspect == null || !item.Aspect.enabled || item.Aspect.gameObject == null )
				{
					trackedItems.RemoveAt( i );
					continue;
				}

				// Determine whether the item is still visible
				updateTrackedItem( item );

				if( !item.IsVisible )
				{

					if( item.Aspect != null )
					{
						SendMessage( "OnVisibleObjectLost", item, SendMessageOptions.DontRequireReceiver );
					}

					trackedItems.RemoveAt( i );
					continue;

				}

				i += 1;

			}

		}

		private void updateTrackedItem( VisualTrackingInfo item )
		{

			if( item.Aspect == null || !item.Aspect.enabled )
			{
				item.IsVisible = false;
				return;
			}

			if( !isAspectVisible( item.Aspect, ref item.Angle, ref item.Distance ) )
			{
				item.IsVisible = false;
				return;
			}

			var itemTransform = item.Aspect.transform;

			item.IsVisible = true;
			item.LastSeen = Time.realtimeSinceStartup;
			item.Velocity = item.Aspect.Velocity;
			item.Position = itemTransform.position;
			item.Facing = itemTransform.forward;

		}

		private VisualTrackingInfo findTrackingInfo( VisualAspect item )
		{

			for( int i = 0; i < trackedItems.Count; i++ )
			{
				var info = trackedItems[ i ];
				if( info.Aspect == item )
					return info;
			}

			return null;

		}

		private bool isAspectVisible( VisualAspect item, ref float detectionAngle, ref float distance )
		{

			var itemTransform = item.transform;
			var itemPosition = itemTransform.position;

			var directionToItem = ( itemPosition - raycastPosition );

			distance = directionToItem.magnitude;
			if( distance > MaxDistance )
				return false;

			if( distance <= 2f )
				return true;

			detectionAngle = Mathf.Acos( Vector3.Dot( forwardDirection, directionToItem.normalized ) ) * Mathf.Rad2Deg;
			if( Mathf.Abs( detectionAngle ) > FOV * 0.5f )
				return false;

			var ray = new Ray( raycastPosition, directionToItem );

			RaycastHit hitInfo;
			if( !Physics.Raycast( ray, out hitInfo, distance, RaycastLayers.value ) )
			{
				return true;
			}

			if( hitInfo.transform != itemTransform )
				return false;

			return true;

		}

		#endregion

	}

}
