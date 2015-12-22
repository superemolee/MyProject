// Copyright (c) 2014 StagPoint Consulting

using System;
using UnityEngine;

namespace StagPoint.DataCollection
{


	public class DataVariable
	{

		#region Public fields 

		public float Total;
		public float Average;
		public float Min;
		public float Max;

		#endregion 

		#region Private variables

		private Func<float, float> updateAverage;
		private bool isInitialized;

		#endregion 

		#region Public methods 

		public void Update( float value )
		{

			if( !this.isInitialized )
			{
				this.initialize();
			}
			
			this.Total += value;
			this.Min = Mathf.Min( this.Min, value );
			this.Max = Mathf.Max( this.Max, value );
			this.Average = updateAverage( value );

		}

		#endregion 

		#region Private methods

		private void initialize()
		{

			this.isInitialized = true;

			this.Total = 0f;
			this.Min = float.MaxValue;
			this.Max = float.MinValue;
			this.Average = 0;

			this.updateAverage = calculatCMA();

		}

		private static Func<float, float> calculatCMA()
		{

			var avg = 0f;
			var count = 0;

			return ( value ) =>
			{

				if( count == 0 )
					avg = value;
				else
					avg = value / ( count + 1 ) + avg * count / ( count + 1 );

				count += 1;

				return avg;

			};

		}

		#endregion

	}

	public class TimedCounter
	{

		#region Public fields 

		public float Total;
		public DataVariable PerSecond = new DataVariable();

		#endregion 

		#region Private variables 

		private float startTime = 0f;
		private int counter = 0;

		#endregion 

		#region Public methods 

		public void Increment()
		{

			Total += 1;

			if( startTime <= 0f )
			{
				startTime = Time.realtimeSinceStartup;
			}
			else if( Time.realtimeSinceStartup - startTime >= 1f - Time.deltaTime )
			{
				PerSecond.Update( counter );
				counter = 0;
				startTime = Time.realtimeSinceStartup;
			}

			counter += 1;

		}

		public void Flush()
		{

			

		}

		#endregion 

	}

}
