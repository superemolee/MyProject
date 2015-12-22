using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
/// <summary>
/// Blood pressure value.
/// </summary>
public class BloodPressureVal
{
	
	public static bool operator <(BloodPressureVal x,BloodPressureVal y)
	{
		
		return (x.diastolic<y.diastolic || x.systolic < y.systolic);
	}
	
	public static bool operator >(BloodPressureVal x,BloodPressureVal y)
	{
		return (x.diastolic > y.diastolic || x.systolic > y.systolic);
	} 
	
	public static BloodPressureVal operator *(BloodPressureVal hrv,float val)
	{
		BloodPressureVal tmp = hrv;
		tmp.diastolic =(int) (((float)tmp.diastolic) * val);
		tmp.systolic =(int) (((float)tmp.systolic) * val);
		return tmp;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="BloodPressureVal"/> class.
	/// </summary>
	/// <param name='sys'>
	/// Systolic.
	/// </param>
	/// <param name='dia'>
	/// Diastolic.
	/// </param>
	public BloodPressureVal (int sys, int dia)
	{
		systolic = sys;
		diastolic = dia;
	}
	/// <summary>
	/// The systolic value.
	/// </summary>
	public int systolic;
	/// <summary>
	/// The diastolic value.
	/// </summary>
	public int diastolic;
}


[System.Serializable]
/// <summary>
/// This class represents the vital parameter blood pressure.
/// </summary>
public class VPBloodPressure : VitalPara<BloodPressureVal>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="VPBloodPressure"/> class.
	/// </summary>
	public VPBloodPressure()
	{

	}


	#region implemented abstract members of VitalSign[KeyValuePair[System.Int32,System.Int32]]
	/// <summary>
	/// rule of thumb systolic 110 + (age/2) diastolic = systolic - 50 mmHg (see "Notfallrettung und qualifizierter Krankentransport" S133) 
	/// </summary>
	public override BloodPressureVal VitalParaVal {
		get { return base.VitalParaVal; }
		set { base.VitalParaVal = value; }
	}

	/// <summary>
	/// rule of thumb systolic 110 + (age/2) diastolic = systolic - 50 mmHg (see "Notfallrettung und qualifizierter Krankentransport" S133) 
	/// </summary>
	public override bool InitializeVitalPara (GameObject target, int age, float mass, Gender gender)
	{
		base.InitializeVitalPara (target, age,mass, gender);
		MAXVITALPARA = new BloodPressureVal(180,110);
		MINVITALPARA = new BloodPressureVal(Random.Range(90,100),70);
		vitalPara.systolic = 110 + (age / 2) + Random.Range (0, 5);
		//small variation
		vitalPara.diastolic = vitalPara.systolic - 50;
		
		initialVitalPara = vitalPara;
		
		return true;
	}

	#endregion

	#region implemented abstract members of VitalSign[KeyValuePair[System.Int32,System.Int32]]
	public override bool VPUpdate ()
	{			
		if(VitalParaVal < MINVITALPARA || VitalParaVal > MAXVITALPARA)
			return false;

		return true;
	}
	
	#endregion
}

