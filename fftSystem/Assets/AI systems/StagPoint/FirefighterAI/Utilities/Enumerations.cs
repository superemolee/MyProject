
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Firefighter.Utilities
{
	
	/// <summary>
	/// Tasks using crew methods.
	/// </summary>
	public enum Tasks
	{
		Free = 0,
		/// <summary>
		/// 
		/// </summary>
		SurveyInnerCircle,
		/// <summary>
		/// 
		/// </summary>
		SurveyOuterCircle,
		/// <summary>
		/// 
		/// </summary>
		TriageCasualties,
		/// <summary>
		/// 
		/// </summary>
		EstablishCasualtyContact,

		StabiliseVehicles,

		EstablishAToolStagingArea,

		ClearActionCircle,

		ManageGlass,

		CreateSpace,

		AccessFullScene,

		StableSills,
		StableWheels
	}

	/// <summary>
	/// This type defines how the vehicle resting on.
	/// </summary>
	public enum VehicleRestingOn{
		Wheels = 0,
		Side,
		Roof
	}
	


}