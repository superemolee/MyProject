using System;


[System.Serializable]
/// <summary>
/// This class represents the Disorder Consciousness. It is based on the motor, verbal and eyelevel. 
/// The Disorder Value is the sum of the glasgow coma scale
/// </summary>
public class Consciousness : Disorder<int>
{

	
	/// <summary>
	/// The Glasgow Coma Scale verbal level.
	/// </summary>
	public enum GCSVerbalLevel { 
		oriented = 5, 
		confused = 4, 
		inappropriate = 3, 
		incomprehensible  = 2, 
		nothing = 1, 
	};
	
	/// <summary>
	/// The Glasgow Coma Scale verbal level.
	/// </summary>
	public enum GCSMotorLevel { 
		obeysCommands = 6, 
		localizesStimuli = 5, 
		withdrawalStimuli = 4, 
		decorticateResponse = 3, 
		decerebrateResponse  = 2, 
		nothing = 1, 
	};
	
		/// <summary>
	/// The Glasgow Coma Scale verbal level.
	/// </summary>
	public enum GCSEyeLevel { 
		spontaneously = 4, 
		inResponse = 3, 
		inPainfulStimuli = 2, 
		noOpening  = 1, 
	};
	
	
	/// <summary>
	/// level of reduced consciousness
	/// </summary>
	public enum LevelOfReducedConsciousness {
		None = 0,
		Somnolence = 1,
		Sopor = 2,
		Coma = 3
	};
	
	/// <summary>
	/// The glasgow coma scale eye level.
	/// </summary>
	public GCSEyeLevel m_gcsEyeLevel = GCSEyeLevel.spontaneously;
	
	/// <summary>
	/// The glasgow coma scale motor level.
	/// </summary>
	public GCSMotorLevel m_gcsMotorLevel = GCSMotorLevel.obeysCommands;
	
	/// <summary>
	/// The glasgow coma scale verbal level.
	/// </summary>
	public GCSVerbalLevel m_gcsVerbalLevel = GCSVerbalLevel.oriented;
	
	/// <summary>
	/// The m_level of consciousness.
	/// </summary>
	public LevelOfReducedConsciousness m_levelOfConsciousness;
	
	/// <summary>
	/// Gets or sets the disorder value. The value is the score of the Glasgow Coma Scale.
	/// </summary>
	/// <value>
	/// The disorder value.
	/// </value>
	public override int DisorderValue {
		get {
			return (int) m_gcsEyeLevel + (int) m_gcsVerbalLevel + (int) m_gcsMotorLevel;
		}
		set {
			base.DisorderValue = value;
		}
	}
	
	/// <summary>
	/// Initializes the disorder.
	/// </summary>
	/// <param name='avatar'>
	/// The Avatar.
	/// </param>
	public override void initializeDisorder (UnityEngine.GameObject avatar)
	{
		base.initializeDisorder (avatar);
		SetLevelOfReducedConsciousness();
	}
	
	/// <summary>
	/// Calculates the of reduced consciousness upon the glasgow coma scale values.
	/// </summary>
	void SetLevelOfReducedConsciousness()
	{
		if(DisorderValue < 7)
		{
			m_levelOfConsciousness = LevelOfReducedConsciousness.Coma;
		}else if(DisorderValue < 10)
		{
			m_levelOfConsciousness = LevelOfReducedConsciousness.Sopor;
		}else if(DisorderValue < 12)
		{
			m_levelOfConsciousness = LevelOfReducedConsciousness.Somnolence;
		}else{
			m_levelOfConsciousness = LevelOfReducedConsciousness.None;
		}
	}
	
	/// <summary>
	/// The Disorder update function.
	/// </summary>
	public override void DOUpdate ()
	{
		base.DOUpdate ();
		SetLevelOfReducedConsciousness();
		
		
	}
	
}
