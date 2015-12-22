
using UnityEngine;

[AddComponentMenu("Avatar/Speak/Speak Morph Animate")]
[ExecuteInEditMode]
/// <summary>
/// This class holds the speak visems for german speaking.
/// </summary>
public class SpeakMorphAnim : MonoBehaviour
{
	/// <summary>
	/// The visem A source Channel.
	/// </summary>
	public string	visemASrcChannel = "VisemA";
	/// <summary>
	/// The visem A percent.
	/// </summary>
	public float	visemAPercent = 0.0f;
	/// <summary>
	/// The visem a Channel.
	/// </summary>
	MegaMorphChan	visemAChannel;
	
	/// <summary>
	/// The visem C source Channel.
	/// </summary>
	public string visemCSrcChannel = "VisemC";
	/// <summary>
	/// The visem C percent.
	/// </summary>
	public float visemCPercent = 0.0f;
	/// <summary>
	/// The visem c Channel.
	/// </summary>
	MegaMorphChan visemCChannel;
	
	/// <summary>
	/// The visem E source Channel.
	/// </summary>
	public string visemESrcChannel = "VisemE";
	/// <summary>
	/// The visem E percent.
	/// </summary>
	public float visemEPercent = 0.0f;
	/// <summary>
	/// The visem e Channel.
	/// </summary>
	MegaMorphChan visemEChannel;
	
	/// <summary>
	/// The visem F source Channel.
	/// </summary>
	public string visemFSrcChannel = "VisemF";
	/// <summary>
	/// The visem F percent.
	/// </summary>
	public float visemFPercent = 0.0f;
	/// <summary>
	/// The visem f Channel.
	/// </summary>
	MegaMorphChan visemFChannel;
	
	/// <summary>
	/// The visem M source channel.
	/// </summary>
	public string visemMSrcChannel = "VisemM";
	/// <summary>
	/// The visem M percent.
	/// </summary>
	public float visemMPercent = 0.0f;
	/// <summary>
	/// The visem M channel.
	/// </summary>
	MegaMorphChan visemMChannel;
	
	public string visemNSrcChannel = "VisemN";
	public float visemNPercent = 0.0f;
	MegaMorphChan visemNChannel;
	
	/// <summary>
	/// The visem O source channel.
	/// </summary>
	public string visemOSrcChannel = "VisemO";
	/// <summary>
	/// The visem O percent.
	/// </summary>
	public float visemOPercent = 0.0f;
	/// <summary>
	/// The visem O channel.
	/// </summary>
	MegaMorphChan visemOChannel;
	
	/// <summary>
	/// The visem P source channel.
	/// </summary>
	public string visemPSrcChannel = "VisemP";
	/// <summary>
	/// The visem P percent.
	/// </summary>
	public float visemPPercent = 0.0f;
	/// <summary>
	/// The visem P channel.
	/// </summary>
	MegaMorphChan visemPChannel;
	
	/// <summary>
	/// The visem Q source channel.
	/// </summary>
	public string visemQSrcChannel = "VisemQ";
	/// <summary>
	/// The visem Q percent.
	/// </summary>
	public float visemQPercent = 0.0f;
	/// <summary>
	/// The visem Q channel.
	/// </summary>
	MegaMorphChan visemQChannel;
	
	/// <summary>
	/// The visem R source channel.
	/// </summary>
	public string visemRSrcChannel = "VisemR";
	/// <summary>
	/// The visem R percent.
	/// </summary>
	public float visemRPercent = 0.0f;
	/// <summary>
	/// The visem R channel.
	/// </summary>
	MegaMorphChan visemRChannel;
	
	/// <summary>
	/// The visem S source channel.
	/// </summary>
	public string visemSSrcChannel = "VisemS";
	/// <summary>
	/// The visem S percent.
	/// </summary>
	public float visemSPercent = 0.0f;
	/// <summary>
	/// The visem S channel.
	/// </summary>
	MegaMorphChan visemSChannel;
	
	/// <summary>
	/// The visem T source channel.
	/// </summary>
	public string visemTSrcChannel = "VisemT";
	/// <summary>
	/// The visem T percent.
	/// </summary>
	public float visemTPercent = 0.0f;
	/// <summary>
	/// The visem T channel.
	/// </summary>
	MegaMorphChan visemTChannel;
	
	/// <summary>
	/// The visem U source channel.
	/// </summary>
	public string visemUSrcChannel = "VisemU";
	/// <summary>
	/// The visem U percent.
	/// </summary>
	public float visemUPercent = 0.0f;
	/// <summary>
	/// The visem U channel.
	/// </summary>
	MegaMorphChan visemUChannel;
	
	/// <summary>
	/// The visem Y source channel.
	/// </summary>
	public string visemYSrcChannel = "VisemY";
	/// <summary>
	/// The visem Y percent.
	/// </summary>
	public float visemYPercent = 0.0f;
	/// <summary>
	/// The visem Y channel.
	/// </summary>
	MegaMorphChan visemYChannel;
	
	/// <summary>
	/// The visem Z source channel.
	/// </summary>
	public string visemZSrcChannel = "VisemZ";
	/// <summary>
	/// The visem Z percent.
	/// </summary>
	public float visemZPercent = 0.0f;
	/// <summary>
	/// The visem Z channel.
	/// </summary>
	MegaMorphChan visemZChannel;
	

	void Start()
	{
		MegaMorph mr = GetComponent<MegaMorph>();

		if ( mr != null )
		{
			visemAChannel = mr.GetChannel(visemASrcChannel);
		 visemCChannel = mr.GetChannel(visemCSrcChannel);
		 visemEChannel = mr.GetChannel(visemESrcChannel);
		 visemFChannel = mr.GetChannel(visemFSrcChannel);
		 visemMChannel = mr.GetChannel(visemMSrcChannel);
		 visemNChannel = mr.GetChannel(visemNSrcChannel);
		 visemOChannel = mr.GetChannel(visemOSrcChannel);
		 visemPChannel = mr.GetChannel(visemPSrcChannel);
		 visemQChannel = mr.GetChannel(visemQSrcChannel);
		 visemRChannel = mr.GetChannel(visemRSrcChannel);
		 visemSChannel = mr.GetChannel(visemSSrcChannel);
		 visemTChannel = mr.GetChannel(visemTSrcChannel);
		 visemUChannel = mr.GetChannel(visemUSrcChannel);
		 visemYChannel = mr.GetChannel(visemYSrcChannel);
		 visemZChannel = mr.GetChannel(visemZSrcChannel);

			
			
		}
	}

	void Update()
	{
		if (visemAChannel!= null )  visemAChannel.Percent =  visemAPercent;
		if (visemCChannel!= null )  visemCChannel.Percent =  visemCPercent;
		if (visemEChannel!= null )  visemEChannel.Percent =  visemEPercent;
		if (visemFChannel!= null )  visemFChannel.Percent =  visemFPercent;
		if (visemMChannel!= null )  visemMChannel.Percent =  visemMPercent;
		if (visemNChannel!= null )  visemNChannel.Percent =  visemNPercent;
		if (visemOChannel!= null )  visemOChannel.Percent =  visemOPercent;
		if (visemPChannel!= null )  visemPChannel.Percent =  visemPPercent;
		if (visemQChannel!= null )  visemQChannel.Percent =  visemQPercent;
		if (visemRChannel!= null )  visemRChannel.Percent =  visemRPercent;
		if (visemSChannel != null ) visemSChannel.Percent = visemSPercent;
		if (visemTChannel != null ) visemTChannel.Percent = visemTPercent;
		if (visemUChannel != null ) visemUChannel.Percent = visemUPercent;
		if (visemYChannel != null ) visemYChannel.Percent = visemYPercent;
		if (visemZChannel != null ) visemZChannel.Percent = visemZPercent;
	
		
		
	}
}