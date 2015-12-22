using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// This class provide functions to use force feedback in unity. It depends on directx sdk. Download the SDK at 
/// <seealso cref="http://www.microsoft.com/en-us/download/details.aspx?id=6812"/>. The plugin must be placed in a Folder named "Plugins". 
/// The necessary dll is "ForceFeedback.dll"
/// </summary>
[AddComponentMenu("Avatar/Display/Control(ForceFeedback)")]
public class Control : MonoBehaviour {
	/// <summary>
	/// The indicator if the initialization of the force feedback was succesfull (0). If it's !=0 an error happend. 
	/// </summary>
	long m_forceFeedbackInit=0L;
	
	/// <summary>
	/// Inits the direct input.
	/// </summary>
	/// <returns>
	/// The direct input.
	/// </returns>
	[DllImport ("ForceFeedback")]
	private static extern long InitDirectInput();
	
	/// <summary>
	/// Forces the feedback.
	/// </summary>
	/// <returns>
	/// The feedback.
	/// </returns>
	/// <param name='val'>
	/// Value.
	/// </param>
	[DllImport ("ForceFeedback")]
	private static extern long forceFeedback(float val);	
	
	/// <summary>
	/// Frees the direct input.
	/// </summary>
	[DllImport ("ForceFeedback")]
	private static extern void FreeDirectInput();	
	
	/// <summary>
	/// Zeros the force feedback.
	/// </summary>
	public void ZeroForceFeedback()
	{
		forceFeedback(0);
	}
	
	
	void Awake () {
		//reserve force feedback for this window
		// avoid error
//		m_forceFeedbackInit = InitDirectInput();
		
	}
	
	void Start()
	{
	}
	
	/// <summary>
	/// Triggers the force feedback with sendmessage method. So we don't need to check for the component
	/// </summary>
	/// <param name='values'>
	/// first value in array is amount of rumbling, second the length of the feedback.
	/// </param>
	public void TriggerForceFeedback(object [] values)
	{
		if(enabled && values.Length==2)
			TriggerForceFeedback((float)values[0],(float)values[1]);
	}
	
	
	/// <summary>
	/// Triggers the force feedback.
	/// </summary>
	/// <param name='val1'>
	/// the amount of rumbling
	/// </param>
	/// <param name='length'>
	/// how long the feedback is on.
	/// </param>
	public void TriggerForceFeedback(float val1, float length)
	{
		if(m_forceFeedbackInit==0)
		{
			forceFeedback(val1);
			Invoke("ZeroForceFeedback",length);
		}
	}
	                              
	void OnDestroy()               
	{
		if(m_forceFeedbackInit == 0)
		{
// avoid error
//			FreeDirectInput();
		}
	}
	
	
}
