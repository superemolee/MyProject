using System;
using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Avatar/Display/VerbalCommunication")]
/// <summary>
/// This class is used to provide the Verbal communication. It simple holds questions and answers. You should provide. 
/// 
/// Defaults for conscinous are provided
/// </summary>
public class VerbalCommunication : MonoBehaviour
{
	/// <summary>
	/// This internal class represents a verbal communication value for the Glasgow Coma Scale.
	/// </summary>
	[Serializable]
	public class GCSVerbalCommunicationValue{
		/// <summary>
		/// The question.
		/// </summary>
		public string question;
		/// <summary>
		/// The possible answers. Use index 5 for oriented and index 2 for incompehensible like the results on the glasgow coma scale.
		/// </summary>
		public string [] answers;
		
		public GCSVerbalCommunicationValue(string q, string[] a)
		{
			question = q;
			answers = a;
		}
	}
	
	/// <summary>
	/// The array of questions and answers to the corresponding glasgow coma scale value.
	/// </summary>
	public GCSVerbalCommunicationValue [] m_gcsVerbalCommunications;
		
			
	
}


