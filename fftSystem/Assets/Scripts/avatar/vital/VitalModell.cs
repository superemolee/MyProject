using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;

[AddComponentMenu("Avatar/Vital/Vitalmodell")]
[RequireComponent(typeof(BodyRegions))]
/// <summary>
/// The central class which calculates all parameters in the Vital modell. It holds also the values for injuries and first aid.
/// </summary>
public class VitalModell: MonoBehaviour {
	
	/// <summary>
	/// The maximum time in minutes without breathing. After this time, the patient is dead.
	/// </summary>
	public float m_maxWOBreathing = 8.0f;
	
	/// <summary>
	/// Human is alive.
	/// </summary>
	public bool m_isAlive = true;
		
	/// <summary>
	/// Age of human.
	/// </summary>
	public int m_age = 25;
	
	/// <summary>
	/// The time since disorder happens.
	/// </summary>
	public float m_timeSinceDisorder = 0.0f;
	
	/// <summary>
	/// Amount of blood depends on Mass in kg
	/// </summary>
	public int m_mass = 80;
	
	/// <summary>
	/// The avatar can walk. Will automaticly set to false if legs are broken or burnings in this region are to heavy.
	/// </summary>
	public bool m_canWalk = true;
	
	/// <summary>
	/// Gender of the human
	/// </summary>
	public VitalPara.Gender m_gender = VitalPara.Gender.male;
	
	/// <summary>
	/// Should the texture for injuries be generated from templates (true) or is it already builded (false).
	/// </summary>
	public bool m_generateInjuryTexture = true;
	
	/// <summary>
	/// The skin material with the base texture of the avatar.
	/// </summary>
	public Material m_skinMaterial = null;
	
	/// <summary>
	/// The skin renderer.
	/// </summary>
	public Renderer m_skinRenderer = null;
	
	/// <summary>
	/// The shock position enhancement.
	/// </summary>
	public float m_shockPositionEnhancement = 0.10f;
	
	/// <summary>
	/// The indicator to display all the functions in case false megamodifyobject will be disabled for performance reasons.
	/// </summary>
	public bool m_isCheckVitalMode = false;
	
	#region Vitalparameters
	/// <summary>
	/// The temperature.
	/// </summary>
	public VPTemperature m_temperature;
	/// <summary>
	/// The blood pressure.
	/// </summary>
	public VPBloodPressure m_bloodPressure;
	/// <summary>
	/// The pulse.
	/// </summary>
	public VPPulse m_pulse;
	/// <summary>
	/// The respiration freq.
	/// </summary>
	public VPRespirationRate m_respirationFreq;
	/// <summary>
	/// The blood amount.
	/// </summary>
	public VPBlood m_bloodAmount;
	#endregion
	
	
	#region injuries
	/// <summary>
	/// The possible fractures.
	/// </summary>
	public Fracture m_fracture;
	/// <summary>
	/// The possible wounds.
	/// </summary>
	public Wound m_wound;
	/// <summary>
	/// The possible poisening.
	/// </summary>
	public Poisening m_poisening;
	/// <summary>
	/// The consciosness.
	/// </summary>
	public Consciousness m_consciosness;
	/// <summary>
	/// The possible burnings.
	/// </summary>
	public Burning m_burning;
	/// <summary>
	/// The possible blocked breath.
	/// </summary>
	public BlockedBreath m_blockedBreath;
	#endregion
	
	#region first aid
	/// <summary>
	/// The first aid patient position.
	/// </summary>
	public PatientPosition m_patientPosition;
	
	/// <summary>
	/// The first aid tourniquet.
	/// </summary>
	public Tourniquet m_tourniquet;
	#endregion
	
	/// <summary>
	/// The blood loss per min.
	/// </summary>
	private int m_bloodLossmin;
	
	/// <summary>
	/// The list of internal vital parameters.
	/// </summary>
	private List<VitalPara> m_vitalParams;
	
	/// <summary>
	/// The map of disorders.
	/// </summary>
	private IDictionary<System.Type, Disorder> m_disorders;
	
	/// <summary>
	/// The list of internal first aid actions.
	/// </summary>
	private List<FirstAid> m_firstAid;
	
	/// <summary>
	/// The time breath blocked.
	/// </summary>
	private float m_timeBreathBlocked = 0.0f;
	
	/// <summary>
	/// The time since no breath.
	/// </summary>
	private float m_timeNoBreath = -1.0f;
	
	/// <summary>
	/// The actual cohb value.
	/// </summary>
	private double m_cohb =0.0;
	
	/// <summary>
	/// The time since dead.
	/// </summary>
	private float m_timeSinceDead = 0.0f;
	
	/// <summary>
	/// The injured region textures.
	/// </summary>
	private IDictionary<int, Color[]> m_injuredRegionTextures;
	
	/// <summary>
	/// The is shock position enhance.
	/// </summary>
	private bool m_isShockPositionEnhance = false;
	
	/// <summary>
	/// The internal modify object. The Modifier will get activated if m_isCheckVitalMode is enabled. This safes a lot of calculations.
	/// </summary>
	public MegaModifyObject m_modifyObject;
	
	// Use this for initialization
	void Start () {
	
		// turnoff gravity of 
		Rigidbody []rbs = gameObject.GetComponentsInChildren<Rigidbody>();
		
		foreach(Rigidbody rb in rbs)
		{
			
			//rb.useGravity = false;
			rb.isKinematic = true;
		}
		m_vitalParams = new List<VitalPara>();
		m_vitalParams.Add(m_respirationFreq);
		m_vitalParams.Add(m_bloodPressure);
		m_vitalParams.Add(m_temperature);
		m_vitalParams.Add(m_pulse);
		m_vitalParams.Add(m_bloodAmount);
		
		m_disorders = new Dictionary<System.Type, Disorder>();
		m_disorders.Add(typeof(Fracture),m_fracture);
		m_disorders.Add(typeof(Wound),m_wound);
		m_disorders.Add(typeof(Poisening),m_poisening);
		m_disorders.Add(typeof(Burning),m_burning);
		m_disorders.Add(typeof(BlockedBreath), m_blockedBreath);
		m_disorders.Add(typeof(Consciousness), m_consciosness);
		
		
		m_firstAid = new List<FirstAid>();
		m_firstAid.Add(m_patientPosition);
		m_firstAid.Add(m_tourniquet);
		
		m_modifyObject = GetComponentInChildren<MegaModifyObject>();
					
		InitVitalParas();
		InitDisorders();
		
		//calculate the initial since wounds and fractures are there
		m_bloodAmount.VitalParaVal = m_bloodAmount.initialVitalPara - ((m_wound.TotalBloodLoss + m_fracture.TotalBloodLoss )*m_timeSinceDisorder);
		m_timeBreathBlocked = ((m_timeSinceDisorder*60.0f) *m_blockedBreath.DisorderValue);
		
//		Debug.Log("TimeBreathblocked: " + m_timeBreathBlocked);
		
		m_cohb = CalculateCOHb(m_timeSinceDisorder*60.0f);
//		Debug.Log("COHB: " +m_cohb);
		
		//disable canwalk if there are fractures at the leg
		if(m_canWalk)
		{
			m_canWalk = (m_fracture.m_lShin.fractureType != Fracture.FractureType.None) ||
				(m_fracture.m_rShin.fractureType != Fracture.FractureType.None) ||
				(m_fracture.m_lThigh.fractureType != Fracture.FractureType.None) ||
				(m_fracture.m_rThigh.fractureType != Fracture.FractureType.None) ;
		}
		
		//generate texture from templates
		if(m_generateInjuryTexture)
		{
			//if renderer is null use default makehuman
			if(m_skinRenderer == null || m_skinMaterial == null)
			{
				
				Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
				foreach(Renderer ren in allRenderers)
				{
					if(ren.name.ToLower().Contains(name.ToLower()+"mesh") || ren.name == name)
					{
						m_skinRenderer = ren;
						m_skinMaterial = ren.materials[0];
						break;
					}
				}
				
			}
			
			
			
			//create the texture for the human in order to 
			Material tmpmat = Instantiate(m_skinMaterial) as Material;
			
			Texture2D tex = tmpmat.mainTexture as Texture2D;
	
			Texture2D newTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32,true);
			Color [] orgPixels = tex.GetPixels();
			
			
			IList <KeyValuePair<int[], Color[]>> combinedTexture = new List <KeyValuePair<int[], Color[]>>();
			
			
			//redness
			int [] burning1Idx = m_burning.GetBurningRegions(Burning.BurningDegree.degree1);
			if(burning1Idx != null)
			{
				// here add an offset to the base texture
				
				orgPixels = TextureUtilities.AddChannelOffset(ref orgPixels, burning1Idx, m_burning.burning1stDegreeOffset, TextureUtilities.ColorChannel.red);
				
			}
			//hematoms
			int [] haematomIdx = m_fracture.GetHematomRegions();
			if(haematomIdx!= null)
			{
				Color [] colors = m_fracture.m_haematomTextureTemplate.GetPixels();
				combinedTexture.Add(new KeyValuePair<int[],Color[]>(haematomIdx, colors));
			}
			
			//burning2
			int [] burning2Idx = m_burning.GetBurningRegions(Burning.BurningDegree.degree2);
			if(burning2Idx != null)
			{
				Color [] colors = m_burning.burning2ndDegreeTex.GetPixels();
				combinedTexture.Add(new KeyValuePair<int[],Color[]>(burning2Idx, colors));
			}
			//wounds
			int [] woundIdx = m_wound.GetWoundRegions();
			if(woundIdx != null)
			{
				Color [] colors = m_wound.m_woundTextureTemplate.GetPixels();
				combinedTexture.Add(new KeyValuePair<int[],Color[]>(woundIdx, colors));
			}
			//burning3
			int [] burning3Idx = m_burning.GetBurningRegions(Burning.BurningDegree.degree3);
			if(burning3Idx != null)
			{
				Color [] colors = m_burning.burning3rdDegreeTex.GetPixels();
				combinedTexture.Add(new KeyValuePair<int[],Color[]>(burning3Idx, colors));
			}
			
			Color [] newPixels = TextureUtilities.CombineMultipleTextures(ref orgPixels, ref combinedTexture);
			
			newTex.SetPixels(newPixels);
			newTex.Apply();
			
			//TextureUtilities.EncodeTextureToFile(newTex, "debug.png");
			tmpmat.mainTexture = newTex;
			
			for(int i = 0; i< m_skinRenderer.materials.Length;++i)
			{
				if(m_skinRenderer.materials[i].name == m_skinMaterial.name)
				{
					m_skinRenderer.materials[i]= tmpmat;
					m_skinRenderer.material.mainTexture = newTex;
					break;
				}
			}
		}	
		m_isAlive = true;
	}
	
	/// <summary>
	/// Calculates the respiration rate with all possible injuries.
	/// </summary>
	void CalculateRespirationRate()
	{
		List<float> combinedList = new List<float>();
		
		int resFreq = -1;
		if(m_blockedBreath.DisorderValue == 1)
		{
			resFreq = 0;	
		}
		else
		{
			if(m_poisening.CarbonMonoxidPPM >0.0)
			{
				float cohbblood = (float) m_cohb;
				if(cohbblood>=0.0f && cohbblood <=0.7f)
				{
				    combinedList.Add((float) ((float)m_respirationFreq.MAXVITALPARA - (float)m_respirationFreq.initialVitalPara)/0.7f *(float)m_cohb + (float)m_respirationFreq.initialVitalPara);
				}
				else if(cohbblood >0.7f){
					resFreq = 0;
				}
			}
			float bu = m_burning.DisorderValue;
			if(bu > 0.0f)
			{
				if(bu >0.0f && bu <=0.35f)
				{
					combinedList.Add( (float) ((float)m_respirationFreq.MAXVITALPARA - (float)m_respirationFreq.initialVitalPara)/0.35f *bu + (float) m_respirationFreq.initialVitalPara);
				}
				else
				{
					resFreq = 0;
				}
			}
			float bl = (float)(m_bloodAmount.initialVitalPara - m_bloodAmount.VitalParaVal)/(float)m_bloodAmount.initialVitalPara;
			if(bl >0.0f)
			{
				if(bl >0.2f && bl <=0.5f)
				{
					combinedList.Add( (float) ((float)m_respirationFreq.MAXVITALPARA - (float)m_respirationFreq.initialVitalPara)/0.3f *
						bl - (m_respirationFreq.MAXVITALPARA-m_respirationFreq.initialVitalPara)/1.5f + (float) m_respirationFreq.initialVitalPara);
				}
				else if(bl >0.5f)
				{
					resFreq =0;
				}
			}
			
			
		}
		if(resFreq < 0)
		{
			if(combinedList.Count >0)
			{
				float rr = 0.0f;
				foreach(float f in combinedList)
				{
					rr+=f;
				}
				rr/=combinedList.Count;
				m_respirationFreq.VitalParaVal = Mathf.CeilToInt(rr);
			}
			else{
				m_respirationFreq.VitalParaVal = m_respirationFreq.initialVitalPara;
			}
			
		}
		else{
			m_respirationFreq.VitalParaVal = 0;
		}
		
	}
	
	/// <summary>
	/// Calculates the pulse with all possible injuries.
	/// </summary>
	void CalculatePulse()
	{
		List<float> combinedList = new List<float>();
		
		int pulseFreq = -1;
		
		//blood loss
		float bl = (float)(m_bloodAmount.initialVitalPara - m_bloodAmount.VitalParaVal)/(float)m_bloodAmount.initialVitalPara;
		if(bl >0.0f)
		{
			if(bl >0.0f && bl <=0.5f)
			{
				combinedList.Add( (float) ((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)*2.0f *bl + (float) m_pulse.initialVitalPara);
			}
			else if(bl >0.5f)
			{
				pulseFreq =0;
			}
		}
		
		//burnings
		float bu = m_burning.DisorderValue;
		if(bu > 0.0f)
		{
			if(bu >0.0f && bu <=0.35f)
			{
				combinedList.Add( (float) ((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)/0.35f *bu + (float) m_pulse.initialVitalPara);
			}
			else
			{
				pulseFreq = 0;
			}
		}
		
		if(m_blockedBreath.DisorderValue == 1)
		{
			if(m_timeBreathBlocked > 120.0f && m_timeBreathBlocked <=480.0f)
			{
				combinedList.Add( (float) ((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)/6.0f *(m_timeBreathBlocked/60.0f) - 
					4*(((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)/3.0f) + (float)m_pulse.MAXVITALPARA);
					
			}
			else if(m_timeBreathBlocked >480.0f)
			{
				pulseFreq = 0;
				
			}
		}

		if(m_poisening.CarbonMonoxidPPM >0.0)
		{
			float cohbblood = (float) m_cohb/100.0f ;
			if(cohbblood>0.1f && cohbblood <=0.5f)
			{
			    combinedList.Add((float) ((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)/0.4f *(float)m_cohb +
					(float)m_pulse.MAXVITALPARA - (((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)/0.4f)*0.5f);
			}
			else if (cohbblood >0.5f && cohbblood <=0.7f)
			{
				combinedList.Add((float) ((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)/0.4f *(float)m_cohb +
					(float)m_pulse.MAXVITALPARA - (((float)m_pulse.MAXVITALPARA - (float)m_pulse.initialVitalPara)/0.4f)*0.5f);
			}
			else if( cohbblood>0.7f )
			{
				pulseFreq =0;
			}
		}


		if(pulseFreq < 0 )
		{
			if(combinedList.Count >0)
			{
				float val = 0.0f;
				foreach(float f in combinedList)
				{
					val+=f;
				}
				val/=combinedList.Count;
				
				if(m_isShockPositionEnhance)
				{
					val*= (1.0f-m_shockPositionEnhancement);
				}
				
				m_pulse.VitalParaVal = Mathf.CeilToInt(val);
			}
			
		}
		else{
			m_pulse.VitalParaVal = 0;
		}
		
	}
	
	/// <summary>
	/// Calculates the blood pressure with all possible injuries.
	/// </summary>
	void CalculateBloodPressure()
	{
		List<BloodPressureVal> combinedList = new List<BloodPressureVal>();
		
		
		BloodPressureVal bp = null;
		
		//blood loss
		float bl = (float)(m_bloodAmount.initialVitalPara - m_bloodAmount.VitalParaVal)/(float)m_bloodAmount.initialVitalPara;
		if(bl >0.0f)
		{
			if(bl >0.0f && bl <=0.5f)
			{
				float sys = (float) ((float)m_bloodPressure.MINVITALPARA.systolic - (float)m_bloodPressure.initialVitalPara.systolic)*2.0f *
					bl + (float) m_bloodPressure.initialVitalPara.systolic;
				
				float dia = (float) ((float)m_bloodPressure.MINVITALPARA.diastolic - (float)m_bloodPressure.initialVitalPara.diastolic)*2.0f *
					bl + (float) m_bloodPressure.initialVitalPara.diastolic;
				
				
				combinedList.Add(new BloodPressureVal(Mathf.CeilToInt(sys),Mathf.CeilToInt(dia)));
			}
			else if(bl >0.5f)
			{
				bp =new BloodPressureVal(0,0);
			}
		}
		
		//burnings
		float bu = m_burning.DisorderValue;
		if(bu > 0.0f)
		{
			if(bu >0.0f && bu <=0.35f)
			{
				float sys = (float) ((float)m_bloodPressure.MINVITALPARA.systolic - (float)m_bloodPressure.initialVitalPara.systolic)/0.35f *bu + 
					(float) m_bloodPressure.initialVitalPara.systolic;
				float dia = (float) ((float)m_bloodPressure.MINVITALPARA.diastolic - (float)m_bloodPressure.initialVitalPara.diastolic)/0.35f *bu + 
					(float) m_bloodPressure.initialVitalPara.diastolic;
				
				combinedList.Add(new BloodPressureVal(Mathf.CeilToInt(sys),Mathf.CeilToInt(dia)) );
			}
			else
			{
				bp =new BloodPressureVal(0,0);
			}
		}
		//blocked breath
		if(m_blockedBreath.DisorderValue == 1)
		{
			if(m_timeBreathBlocked > 120.0f && m_timeBreathBlocked <=480.0f)
			{
				float sys = (float) ((float)m_bloodPressure.MAXVITALPARA.systolic - (float)m_bloodPressure.initialVitalPara.systolic)/6.0f 
					*(m_timeBreathBlocked/60.0f) - 4*(((float)m_bloodPressure.MAXVITALPARA.systolic - 
						(float)m_bloodPressure.initialVitalPara.systolic)/3.0f) + (float)m_bloodPressure.MAXVITALPARA.systolic;
				
				float dia = (float) ((float)m_bloodPressure.MAXVITALPARA.diastolic - (float)m_bloodPressure.initialVitalPara.diastolic)/6.0f 
					*(m_timeBreathBlocked/60.0f) - 4*(((float)m_bloodPressure.MAXVITALPARA.diastolic - 
						(float)m_bloodPressure.initialVitalPara.diastolic)/3.0f) + (float)m_bloodPressure.MAXVITALPARA.diastolic;
				
				combinedList.Add(new BloodPressureVal(Mathf.CeilToInt(sys),Mathf.CeilToInt(dia)) );
					
			}
			else if(m_timeBreathBlocked >480.0f)
			{
				bp =new BloodPressureVal(0,0);	
			}
		}
		//poisening
		if(m_poisening.CarbonMonoxidPPM >0.0)
		{
			float cohbblood = (float)m_cohb/100.0f;
			if(cohbblood>0.1f && cohbblood <=0.5f)
			{
			    float sys = (float) ((float)m_bloodPressure.MAXVITALPARA.systolic - (float)m_bloodPressure.initialVitalPara.systolic)/0.4f 
					*(float)m_cohb + (float)m_bloodPressure.MAXVITALPARA.systolic - (((float)m_bloodPressure.MAXVITALPARA.systolic 
						- (float)m_bloodPressure.initialVitalPara.systolic)/0.4f)*0.5f;
				float dia = (float) ((float)m_bloodPressure.MAXVITALPARA.diastolic - (float)m_bloodPressure.initialVitalPara.diastolic)/0.4f 
					*(float)m_cohb + (float)m_bloodPressure.MAXVITALPARA.diastolic - (((float)m_bloodPressure.MAXVITALPARA.diastolic 
						- (float)m_bloodPressure.initialVitalPara.diastolic)/0.4f)*0.5f;
				
				
				combinedList.Add(new BloodPressureVal(Mathf.CeilToInt(sys),Mathf.CeilToInt(dia)));
			}
			else if (cohbblood >0.5f && cohbblood <=0.7f)
			{
			    float sys = (float) ((float)m_bloodPressure.MINVITALPARA.systolic - (float)m_bloodPressure.MAXVITALPARA.systolic)/-0.2f 
					*(float)m_cohb + (float)m_bloodPressure.MINVITALPARA.systolic - (((float)m_bloodPressure.MINVITALPARA.systolic 
						- (float)m_bloodPressure.MAXVITALPARA.systolic)/-0.2f)*0.7f;
				float dia = (float) ((float)m_bloodPressure.MINVITALPARA.diastolic - (float)m_bloodPressure.MAXVITALPARA.diastolic)/-0.2f 
					*(float)m_cohb + (float)m_bloodPressure.MINVITALPARA.diastolic - (((float)m_bloodPressure.MINVITALPARA.diastolic 
						- (float)m_bloodPressure.MAXVITALPARA.diastolic)/-0.2f)*0.7f;
				
				
				combinedList.Add(new BloodPressureVal(Mathf.CeilToInt(sys),Mathf.CeilToInt(dia)));
			}
			else if( cohbblood>0.7f )
			{
				bp =new BloodPressureVal(0,0);	
			}
			
		}


		if(bp ==null)
		{
			if(combinedList.Count >0)
			{
				BloodPressureVal val = new BloodPressureVal(0,0);	
				foreach(BloodPressureVal f in combinedList)
				{
					val.systolic+=f.systolic;
					val.diastolic+=f.diastolic;
					
				}

				
				val.systolic=Mathf.CeilToInt((float) val.systolic /(float)combinedList.Count);
				val.diastolic=Mathf.CeilToInt((float) val.diastolic /(float)combinedList.Count);
				
				//enhance the pressure if the shockposition is activated
				if(m_isShockPositionEnhance)
				{
					val.systolic= Mathf.CeilToInt((float) val.systolic*(1.0f+m_shockPositionEnhancement));
					
					val.diastolic= Mathf.CeilToInt((float) val.diastolic*(1.0f+m_shockPositionEnhancement));
					
				}
				
				m_bloodPressure.VitalParaVal = val;
			}
			
		}
		else{
			m_bloodPressure.VitalParaVal = bp;
		}
		
	}
	
	/// <summary>
	/// Calculates the temperature after dead.
	/// </summary>
	void CalculateTemperature()
	{
		//calculate temperature after dead
		if(!m_isAlive)
		{
			float deltatime = Time.deltaTime;
			float val = m_temperature.VitalParaVal;
			float actualtemp = m_temperature.SurroundingAirTemperature();
			float plusminus = actualtemp >  val ? 1.0f:-1.0f;
			val = val +  (plusminus/60.0f*deltatime);
			m_temperature.VitalParaVal = plusminus < 0 ?Mathf.Max(val, actualtemp) : Mathf.Min(val, actualtemp);
		}
	}
	
	
	
	/// <summary>
	/// Calculates all the vital parameters.
	/// </summary>
	void CalculateVitalParameters()
	{
		//calculate timerelated parameters
		float deltaTime = Time.deltaTime;
		//update current bloodloss if isalive
		if(m_isAlive)
		{
			float totalBloodloss = m_bloodAmount.VitalParaVal - ((m_wound.TotalBloodLoss + m_fracture.TotalBloodLoss )*(deltaTime)/60.0f);
		
			m_bloodAmount.VitalParaVal = totalBloodloss;
		}
		//update time of breath blocked
		m_timeBreathBlocked +=( deltaTime *m_blockedBreath.DisorderValue);
		
				
		//update COHb level only if we are breathing otherwise it makes no sense
		if(m_respirationFreq.VitalParaVal > 0)
		{
			m_cohb+=CalculateCOHb(deltaTime);
		}
		
		//Debug.Log("cohb value:" + m_cohb);
		
		CalculateRespirationRate();
		CalculateBloodPressure();
		CalculatePulse();
		CalculateTemperature();
	
				
		//calculate if can survive
		if(m_respirationFreq.VitalParaVal == 0)
		{
			//initialize no breath after first calculation
			if(m_timeNoBreath < 0.0f)
			{
				m_timeNoBreath = m_timeSinceDisorder * 60.0f;
			}
			else{
				m_timeNoBreath = 0.0f;
			}
			m_timeNoBreath += deltaTime;
			Rigidbody []rbs = gameObject.GetComponentsInChildren<Rigidbody>();
		
			foreach(Rigidbody rb in rbs)
			{
				
				rb.isKinematic = false;
				rb.WakeUp();
			}	
			if(animation.isPlaying)
			{
				//animation.Stop();
				
			}
			
			if(m_timeNoBreath > (m_maxWOBreathing*60.0f) && m_isAlive)
			{
				m_isAlive = false;
				
				
				
			}
		
		}

	}
	
	/// <summary>
	/// Calculates the binding of carbon monoxid with the hemoglobin based on formula 
	/// from Stewart <seealso cref="http://www.ncbi.nlm.nih.gov/pubmed/4682844"/>
	/// </summary>
	/// <returns>
	/// The COhb.
	/// </returns>
	/// <param name='time'>
	/// The elapsed Time in seconds.
	/// </param>
	double CalculateCOHb(float time)
	{
		return (0.00003317 * System.Math.Pow(m_poisening.CarbonMonoxidPPM,(1.036))*m_respirationFreq.RMV*(time/60.0))/100.0f;
	}
	
	
	/// <summary>
	/// Inits the disorders.
	/// </summary>
	void InitDisorders()
	{
		foreach(KeyValuePair<System.Type, Disorder> dis in m_disorders)
		{
			dis.Value.initializeDisorder(gameObject);
		}
	}
	
	
	void OnDestroy()
	{
		MegaMorph mm = gameObject.GetComponent<MegaMorph>();
		if(mm!=null)
		{
		
		}
	}
	
	
	// Update is called once per frame
	void Update () {
		if(m_isAlive)
		{
			CalculateVitalParameters();
			UpdateVitalParas();
			UpdateDisorders();
			
		}
		else{
			m_timeSinceDead += Time.deltaTime;
		}
		
		
		if(m_modifyObject && m_modifyObject.Enabled != m_isCheckVitalMode)
		{
			m_modifyObject.Enabled = m_isCheckVitalMode;
		}
	
					
		
	}
	
	/// <summary>
	/// Inits the vital paras.
	/// </summary>
	void InitVitalParas()
	{
		for(int i= 0; i < m_vitalParams.Count;++i)
		{
			m_vitalParams[i].InitializeVitalPara(this.gameObject, m_age, m_mass, m_gender);
		}
	}
	
	/// <summary>
	/// Updates the vital paras.
	/// </summary>
	void UpdateVitalParas()
	{
		
		foreach(VitalPara vs in m_vitalParams)
		{

			vs.VPUpdate();

		}

	}
	
	/// <summary>
	/// Updates the disorders.
	/// </summary>
	void UpdateDisorders()
	{
		foreach(KeyValuePair<System.Type, Disorder> dis in m_disorders)
		{
			//
			dis.Value.DOUpdate();
		}
	}
	
	/// <summary>
	/// Sets a patient position.
	/// </summary>
	/// <param name='patPos'>
	/// Pat position.
	/// </param>
	public void SetPatientPosition(PatientPosition patPos)
	{


		switch(patPos.m_patientPositionType)
		{
		case PatientPosition.PatientPositionType.RecoveryPosition:
		{
			if(m_blockedBreath.m_blockedObject == null)
			{
				m_blockedBreath.DisorderValue = 0;
				m_blockedBreath.m_isBlocked = false;
			}
			break;
		}
		case PatientPosition.PatientPositionType.OverstretchHead:
			if(m_blockedBreath.m_blockedObject == null)
			{
				m_blockedBreath.DisorderValue = 0;
				m_blockedBreath.m_isBlocked = false;
				//set that just the head moves
				//animblendmode = AnimationBlendMode.Additive;
				//layer = 3;
			}
			break;
		case PatientPosition.PatientPositionType.ShockPosition:
			//because this model just offers a fracture of the hip as internal injury we check this disorder first
			if(m_fracture.m_pelvis.bloodLossPerMinute == 0)
			{
				//check schockindex
				if(((float)m_pulse.VitalParaVal / (float)m_bloodPressure.VitalParaVal.systolic) > 0.9f)
				{
					m_isShockPositionEnhance = true;
				}
			}
			break;
		}
		if(patPos.m_patientPositionAnimationClip)
		{
			animation.AddClip(patPos.m_patientPositionAnimationClip, patPos.m_patientPositionAnimationClip.name);
			animation.CrossFade(patPos.m_patientPositionAnimationClip.name);
		}
	}
	
	/// <summary>
	/// Sets a tourniquet at a region.
	/// </summary>
	/// <param name='region'>
	/// Region.
	/// </param>
	public void SetTourniquet(Color32 region)
	{
		m_wound.SetBloodLossOfRegion(region);
	}
}
