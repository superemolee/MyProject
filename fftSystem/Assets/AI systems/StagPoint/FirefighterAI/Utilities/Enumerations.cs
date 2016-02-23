
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Firefighter.Utilities
{
    public enum GroupLeaderTasks{
        Free = 0
    }

    public enum ToolGroupTasks{
        Free = 0,
        InnerCircleSurvey,
        StableVehicle
    }

    public enum ToolMemberTasks{
        Free = 0,
        StableSills,
        StableWheels
    }

    public enum ToolOperatorTasks{
        Free = 0,
        StableSills,
        StableWheels,
        InnerCircle
    }

    public enum WaterGroupTasks{
        Free = 0
    }

    public enum WaterMemberTasks{
        Free = 0
    }
   
    public enum HoseGroupTasks{
        Free = 0
    }

    public enum HoseMemberTasks{
        Free = 0
    }
	
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