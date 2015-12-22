using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Net.Sockets;

/// <summary>
/// This component implements the speech meachanism of an avatar.
/// </summary>
[RequireComponent(typeof(Animation))]
[AddComponentMenu("Avatar/Speak/Speak")]
public class Speak : MonoBehaviour
{
	/// <summary>
	/// The head of the Avatar. This GameObject holds the AudioSource. In case this object is null
	/// it will search the string head in each children and attach the AudioSource.
	/// </summary>
	public GameObject m_head;
	
	/// <summary>
	/// The attached audio source. Will be automaticly generated during start function.
	/// </summary>
	public AudioSource m_audioSource;
	
	/// <summary>
	/// The mapping of phones to visems.
	/// </summary>
	IDictionary<string,string> m_phonems2Visems;
	
	
	/// <summary>
	/// Initializes the phonems 2 visems.
	/// </summary>
	void InitializePhonems2Visems ()
	{
		m_phonems2Visems = new Dictionary<string, string> ();
		// phoneme to viseme mapping taken from
		// "Phoneme-Viseme Mapping for German Video-Realistic Audio-Visual-Speech-Synthesis" (Aschenberner)
		
		//basic phonemes
		m_phonems2Visems.Add ("p", "P");
		m_phonems2Visems.Add ("b", "P");
		m_phonems2Visems.Add ("t", "T");
		m_phonems2Visems.Add ("d", "T");
		m_phonems2Visems.Add ("k", "T");
		m_phonems2Visems.Add ("g", "T");
		m_phonems2Visems.Add ("n", "N");
		m_phonems2Visems.Add ("@n", "N");
		m_phonems2Visems.Add ("l", "N");
		m_phonems2Visems.Add ("@l", "N");
		m_phonems2Visems.Add ("m", "M");
		m_phonems2Visems.Add ("f", "F");
		m_phonems2Visems.Add ("v", "F");
		m_phonems2Visems.Add ("s", "S");
		m_phonems2Visems.Add ("z", "S");
		m_phonems2Visems.Add ("S", "Z");
		m_phonems2Visems.Add ("Z", "Z");
		m_phonems2Visems.Add ("tS", "Z");
		m_phonems2Visems.Add ("dZ", "Z");
		m_phonems2Visems.Add ("h", "R");
		m_phonems2Visems.Add ("r", "R");
		m_phonems2Visems.Add ("x", "R");
		m_phonems2Visems.Add ("N", "R");
		m_phonems2Visems.Add ("a:", "A");
		m_phonems2Visems.Add ("a", "A");
		m_phonems2Visems.Add ("j", "C");
		m_phonems2Visems.Add ("C", "C");
		m_phonems2Visems.Add ("i:", "E");
		m_phonems2Visems.Add ("I", "E");
		m_phonems2Visems.Add ("e:", "E");
		m_phonems2Visems.Add ("E:", "E");
		m_phonems2Visems.Add ("E", "E");		
		m_phonems2Visems.Add ("o:", "O");
		m_phonems2Visems.Add ("O", "O");
		m_phonems2Visems.Add ("u:", "U");
		m_phonems2Visems.Add ("U", "U");
		m_phonems2Visems.Add ("@", "Q");			
		m_phonems2Visems.Add ("6", "Q");	
		m_phonems2Visems.Add ("y:", "Y");	
		m_phonems2Visems.Add ("Y", "Y");	
		m_phonems2Visems.Add ("2:", "Y");	
		m_phonems2Visems.Add ("9", "Y");	
		
		// composed phonemes
		m_phonems2Visems.Add ("aI", "E");	
		m_phonems2Visems.Add ("=6", "Q");	
		m_phonems2Visems.Add ("ts", "T");	
		m_phonems2Visems.Add ("aU", "A");	
		m_phonems2Visems.Add ("OY", "O");			
		
		// additional phonemes not included in the paper
		m_phonems2Visems.Add ("R", "R");	
		m_phonems2Visems.Add ("u", "U");
		
	}
	
	/// <summary>
	/// Choose Emotional speak.
	/// </summary>
	public enum EmotionalSpeak
	{
		no, //normal TTS voice
		negative, //negative TTS voice
		positve //positive TTS voice
	};
	
	/// <summary>
	/// Installed Mary voices.
	/// </summary>
	public enum MaryVoices
	{
		de7,
		de6,
		dfki_pavoque_styles,
		dfki_pavoque_neutral,
		bits3,
		dfki_pavoque_neutral_hsmm,
		bits3_hsmm,
		bits1_hsmm
	};
	
	/// <summary>
	/// The Scaling for the Phoneme Animation
	/// </summary>
	float m_scale = 3.0f;
	
	/// <summary>
	/// The MegaMorphModifier.
	/// </summary>
	MegaMorph m_morphModifier = null;
	
	/// <summary>
	/// The m_modify stack.
	/// </summary>
	public MegaModifyObject m_modifyStack = null;
	
	/// <summary>
	/// The m_phoneme start time.
	/// </summary>
	int m_phonemeStartTime;
	
	/// <summary>
	/// My emotional speak.
	/// </summary>
	public EmotionalSpeak m_emotionalSpeak = Speak.EmotionalSpeak.no;
	
	/// <summary>
	/// The maryvoice.
	/// </summary>
	public MaryVoices m_maryVoice = MaryVoices.bits3;
	
	/// <summary>
	/// The ip of the maryserver. Server should run in http mode
	/// </summary>
	public string m_serverip = "localhost";
	
	/// <summary>
	/// The port of the maryserver. Server should run in http mode
	/// </summary>
	public string m_serverport = "59125";

	// Use this for initialization
	void Start ()
	{
		
		SpeakMorphAnim speakAnim = gameObject.GetComponentInChildren<SpeakMorphAnim>();
		if(speakAnim)
		{
			m_morphModifier = gameObject.GetComponentInChildren<MegaMorph> ();
			m_modifyStack = gameObject.GetComponentInChildren<MegaModifyObject>();
			if(m_modifyStack!=null)
			{
				m_modifyStack.Enabled=false;
			}
			if(m_morphModifier!=null)
			{
				Mesh copy = Instantiate(m_morphModifier.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh)as Mesh;
				m_morphModifier.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = copy;
				
				MegaModifyObject mmo =GetComponentInChildren<MegaModifyObject>();
				Vector3[] verts = copy.vertices;
				
				mmo.SetMesh(ref verts);
				
				mmo.MeshUpdated();
		
				InitializePhonems2Visems ();
			}
		}
		
		if(!m_head)
		{
			Transform [] allTrans = gameObject.GetComponentsInChildren<Transform>();
			foreach(Transform trans in allTrans)
			{
				if(trans.name.ToLower().Contains("head"))
				{
					m_head = trans.gameObject;
					
					break;
				}
			}
		}
		//in case no head is found or set use the current gameObject
		if(!m_head)
		{
			m_head = gameObject;
		}
		//attach the audioSource
		m_audioSource = m_head.AddComponent<AudioSource>();
		
	}
	
	/// <summary>
	/// Says the text.
	/// </summary>
	/// <param name='text'>
	/// The text to say.
	/// </param>
	public void sayText (string text)
	{
		
		if (Network.peerType != NetworkPeerType.Disconnected) {
			gameObject.networkView.RPC ("SpeakRequest", RPCMode.All, text, networkView.viewID);
		} else {
			StartCoroutine (PrepareSpeak(gameObject, text));
		}
	}
	
	
	/// <summary>
	/// Convert the inputtext to maryxml.
	/// </summary>
	/// <returns>
	/// The text in maryXML.
	/// </returns>
	/// <param name='inputtext'>
	/// The text to convert.
	/// </param>
	protected string Convert2MaryXML (string inputtext)
	{
		//replace german umlaute in html url encoding style
		string encodedInputText = inputtext.ToLower ();
		encodedInputText = encodedInputText.Replace ("\u00FC", "%FC");
		encodedInputText = encodedInputText.Replace ("\u00F6", "%F6");
		encodedInputText = encodedInputText.Replace ("\u00E4", "%E4");
		encodedInputText = encodedInputText.Replace ("\u00DF", "ss");

		//header for all mary speech
		string rawmaryxml = "" +
		"<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
		"<maryxml version=\"0.4\" " +
		"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
		"xmlns=\"http://mary.dfki.de/2002/MaryXML\" " +
		"xml:lang=\"de\">";
		
		switch (m_emotionalSpeak) {
		case EmotionalSpeak.negative:
			{
				//additional prosody for negative speech
				rawmaryxml +=
				"<prosody pitch=%22-20%25%22 " +
				"pitch-dynamics=%22-31%25%22 " +
				"range=%221.92st%22 " +
				"range-dynamics=%22-100%25%22 " +
				"preferred-accent-shape=%22falling%22 " +
				"accent-slope=%22-30%25%22 " +
				"accent-prominence=%22-4%25%22 " +
				"preferred-boundary-type=%22high%22 " +
				"rate=%22-35%25%22 " +
				"number-of-pauses=%22-36%25%22 " +
				"pause-duration=%2210%25%22 " +
				"vowel-duration=%22-13%25%22 " +
				"nasal-duration=%22-13%25%22 " +
				"liquid-duration=%22-13%25%22 " +
				"plosive-duration=%22-13%25%22 " +
				"fricative-duration=%22-13%25%22 " +
				"volume=%2233%22>" +
				encodedInputText +
				"</prosody></maryxml>";

				break;
			}
		case EmotionalSpeak.positve:
			{
				//additional prosody for positive speech
				rawmaryxml +=
				"<prosody pitch=%2218%25%22 " +
				"pitch-dynamics=%22-10%25%22 " +
				"range=%226.52st%22 " +
				"range-dynamics=%2254%25%22 " +
				"preferred-accent-shape=%22rising%22 " +
				"accent-slope=%2243%25%22 " +
				"accent-prominence=%2212%25%22 " +
				"preferred-boundary-type=%22low%22 " +
				"rate=%2240%25%22 " +
				"number-of-pauses=%2244%25%22 " +
				"pause-duration=%22-13%25%22 " +
				"vowel-duration=%2226%25%22 " +
				"nasal-duration=%2226%25%22 " +
				"liquid-duration=%2226%25%22 " +
				"plosive-duration=%2220%25%22 " +
				"fricative-duration=%2220%25%22 " +
				"volume=%2271%22>" +
				encodedInputText +
				"</prosody></maryxml>";
				break;
			}
		case EmotionalSpeak.no:
			{
				//simple speech no emotional parts
				rawmaryxml += encodedInputText +
				"</maryxml>";
				break;
			}
			
		}
		
		//replace all whitespaces with plus sign (url encoding)
		rawmaryxml = rawmaryxml.Replace (" ", "+");
		return rawmaryxml;
		
	}
	
	/// <summary>
	/// Disables the modify stack to save performance.
	/// </summary>
	void DisableModifyStack()
	{
		if(m_modifyStack!=null)
		{
			m_modifyStack.Enabled=false;
		}
	}
	
	
	/// <summary>
	/// Prepares the whole speak.
	/// </summary>
	/// <returns>
	/// Coroutine.
	/// </returns>
	/// <param name='target'>
	/// the target which should speak.
	/// </param>
	/// <param name='text'>
	/// the text to speak
	/// </param>
	/// <param name='voice'>
	/// the voice to speak.
	/// </param>
	IEnumerator PrepareSpeak(GameObject target, string text)
	{
		string rawmary = Convert2MaryXML(text);
		
		Speak targetSpeak = target.GetComponent<Speak>();
		if(targetSpeak.m_modifyStack!=null)
		{
			targetSpeak.m_modifyStack.Enabled=true;
		}
		string voice = Enum.GetName(typeof(Speak.MaryVoices), targetSpeak.m_maryVoice);
		Coroutine co = StartCoroutine(PrepareSynthesis(target,rawmary, voice));
		yield return co;
		
		
		//check if morphAnimation available
		if(m_morphModifier!=null)
		{
			co = StartCoroutine (PreparePhonemize (target,rawmary, voice));
			yield return co;
			
			if(targetSpeak.animation.GetClip("phonemize"))
			{
				targetSpeak.animation.CrossFade("phonemize");
				Invoke("DisableModifyStack", targetSpeak.animation.GetClip("phonemize").length + 0.5f);
			}
		}
		if(targetSpeak.m_audioSource.clip)
			targetSpeak.m_audioSource.Play ();
	}
	
	
	/// <summary>
	/// Gets the speak length of text. Usefull if you want to generate a conversation. 
	/// </summary>
	/// <returns>
	/// The speak length of text.
	/// </returns>
	/// <param name='text'>
	/// Text.
	/// </param>
	public IEnumerator GetAsyncSpeakLengthOfText(string text)
	{
		string inputtext = Convert2MaryXML(text);
		string voice = Enum.GetName(typeof(Speak.MaryVoices), m_maryVoice);
		string rawmaryrequest = "http://" + m_serverip + ":" + m_serverport + "/process?" +
		"INPUT_TEXT=" + inputtext +
		"&INPUT_TYPE=RAWMARYXML" +
		"&OUTPUT_TYPE=REALISED_DURATIONS" +
		"&LOCALE=de" +
		"&VOICE=" + voice +
		"&marylength.txt";
	
		WWW mw = new WWW (rawmaryrequest);
		yield return mw;
			
		if(mw.error==null)
		{
			string durations = mw.text;
			char [] splits ={' ','\n'};
			string[] words = durations.Split(splits);
			
			float duration = (float)Convert.ToDouble(words[words.Length-4]);
			Debug.Log("Duration of : "+ text + " = "+ duration + " seconds."); 

			
		}
	}
	
	/// <summary>
	/// Gets the speak length of text. Usefull if you want to generate a conversation. 
	/// </summary>
	/// <returns>
	/// The speak length of text.
	/// </returns>
	/// <param name='text'>
	/// Text.
	/// </param>
	public float GetSyncSpeakLengthOfText(string text)
	{
		string inputtext = Convert2MaryXML(text);
		string voice = Enum.GetName(typeof(Speak.MaryVoices), m_maryVoice);
		string rawmaryrequest = "http://" + m_serverip + ":" + m_serverport + "/process?" +
		"INPUT_TEXT=" + inputtext +
		"&INPUT_TYPE=RAWMARYXML" +
		"&OUTPUT_TYPE=REALISED_DURATIONS" +
		"&LOCALE=de" +
		"&VOICE=" + voice +
		"&marylength.txt";
	
		WWW mw = new WWW (rawmaryrequest);
		
		//while loop to see when finished
		while(!mw.isDone && mw.error==null)
			;
		
		string durations = mw.text;
		char [] splits ={' ','\n'};
		string[] words = durations.Split(splits);
		
		float duration = (float)Convert.ToDouble(words[words.Length-4]);
		Debug.Log("Duration of : "+ text + " = "+ duration + " seconds."); 

		return duration;	
		
	}
	
	
	/// <summary>
	/// Prepares the synthesis of the mary speak.
	/// </summary>
	/// <returns>
	/// Coroutine
	/// </returns>
	/// <param name='target'>
	/// Target GameObject.
	/// </param>
	/// <param name='inputtext'>
	/// The Inputtext in rawmaryxml
	/// </param>
	/// <param name='voice'>
	/// The Voice.
	/// </param>
	IEnumerator PrepareSynthesis (GameObject target, string inputtext, string voice)
	{
		
		
		//request has to end with a filename.wav otherwise unity can't handle it in the www object
		string rawmaryrequest = "http://" + m_serverip + ":" + m_serverport + "/process?" +
		"INPUT_TEXT=" + inputtext +
		"&INPUT_TYPE=RAWMARYXML" +
		"&OUTPUT_TYPE=AUDIO" +
		"&AUDIO=WAVE_FILE" +
		"&LOCALE=de" +
		"&VOICE=" + voice +
		"&AUDIO_OUT=WAVE_FILE" +
		"&mary.wav";
	
		WWW mw = new WWW (rawmaryrequest);
		yield return mw;
			
		if(mw.error==null)
		{		
//			byte[]stream = mw.bytes;
//			FileStream fs = new FileStream("test.wav",FileMode.CreateNew);
//			fs.Write(stream, 0, stream.Length);
			m_audioSource.clip = mw.audioClip;
			m_audioSource.clip.name = "maryspeak";			
			
			Debug.Log("Audiofile length: " + m_audioSource.clip.length);
		}
		else{
			Debug.Log(mw.error);
		}
	}
		
	

	/// <summary>
	/// Parses the XML and creates the list of phonemes
	/// </summary>
	/// <param name='xmltext'>
	/// the response of the maryserver.
	/// </param>
	/// <param name='output'>
	/// the list of Phonemes to play.
	/// </param>
	private void parseXML (string xmltext, out IList<Phoneme> output)
	{
		System.IO.StringReader xmlSR = new System.IO.StringReader (xmltext);
		Debug.Log(xmltext);
		XmlTextReader textReader = new XmlTextReader (xmlSR);
		output = new List<Phoneme> ();
		m_phonemeStartTime = 0;
		// Read until end of file
		while (textReader.Read()) {
			XmlNodeType nType = textReader.NodeType;
			// if node type is an element
			if (nType == XmlNodeType.Element) {
				switch (textReader.Name.ToString ()) {
				case "ph":
					{
						Phoneme phon = new Phoneme ();
						phon.Name = textReader.GetAttribute ("p");
						phon.Start = m_phonemeStartTime;

						phon.Duration = (int)Convert.ToDouble (textReader.GetAttribute ("d"));
						if (phon.Name == "?") {
							phon.IsPause = true;
						}
						output.Add (phon);
						m_phonemeStartTime += phon.Duration;
					}
					break;
				case "a":
				case "m":
				case "callback":
					{
						Phoneme phon = new Phoneme ();
						phon.Name = "\\" + textReader.Name + textReader.GetAttribute ("name");
						phon.Start = m_phonemeStartTime;

						phon.Duration = 1000;
						output.Add (phon);

					}
					break;
				case "boundary":
					{
						Phoneme phon = new Phoneme ();
                                
						phon.IsPause = true;

						phon.Duration = Convert.ToInt32 (textReader.GetAttribute ("duration"));
						output.Add (phon);
						m_phonemeStartTime += phon.Duration;
					}
					break;
				}
				;
			}

		}
	}
	
	
	/// <summary>
	/// Prepares the synthesis of the mary speak.
	/// </summary>
	/// <returns>
	/// Coroutine
	/// </returns>
	/// <param name='target'>
	/// Target GameObject.
	/// </param>
	/// <param name='inputtext'>
	/// The Inputtext in rawmaryxml
	/// </param>
	/// <param name='voice'>
	/// The Voice.
	/// </param>
	public IEnumerator PreparePhonemize (GameObject target, string inputtext, string voice)
	{
		IList<Phoneme> phonemesList = new List<Phoneme> ();
		m_phonemeStartTime = 0;
      string rawmaryrequest = "http://" + m_serverip + ":" + m_serverport + "/process?" +
		"INPUT_TEXT=" + inputtext +
		"&INPUT_TYPE=RAWMARYXML" +
		"&OUTPUT_TYPE=REALISED_ACOUSTPARAMS" +
		"&VOICE=" + voice +
		"&LOCALE=de" +
		"&mary.txt";
	
		WWW mw = new WWW (rawmaryrequest);
		yield return mw;
		
		if(mw.error==null)
		{

			parseXML (mw.text, out phonemesList);
			AnimationClip ac =mapSequence (phonemesList);
			if(ac!=null)
			{
				target.animation.AddClip(ac, "phonemize");
				target.animation["phonemize"].layer = 3;
				Debug.Log("Animation length: " + ac.length);
			}
		}
	}
	
	
	/// <summary>
	/// Maps the sequence of the phonemes. Based on paper "Phoneme-Viseme Mapping for German Video-Realistic Audio-Visual-Speech-Synthesis" (Aschenberner)
	/// </summary>
	/// <returns>
	/// An AnimationClip of the mapped sequence
	/// </returns>
	/// <param name='input'>
	/// the list of Phonemes
	/// </param>
	AnimationClip mapSequence ( IList<Phoneme> input)
	{
		Dictionary<string, AnimationCurve> curvelist = new Dictionary<string, AnimationCurve> ();
	
		foreach (Phoneme phoneme in input) {
			//const Phoneme & phoneme = *it;
			if (!phoneme.IsPause) {
				// Check the type of the viseme
				if (phoneme.Name.StartsWith ("\\a")) {
					string s = phoneme.Name;
					s.Remove (0, 2);
					//this
					if (m_phonems2Visems.ContainsKey (s)) {
						string viseme = "Visem" + m_phonems2Visems [s];
					
						if (!curvelist.ContainsKey (viseme)) {
							curvelist.Add (viseme, new AnimationCurve ());
						}
						
						curvelist [viseme].AddKey (phoneme.Start / 1000.0f, 100.0f);
					} else {
						Debug.Log ("could not find " + s + " in list"); 
					}

					//output.addKey(101 + id,phoneme.getStart() / 1000.0,1.0);
					continue;
				}
				if (phoneme.Name.StartsWith ("\\m")) {
					string s = phoneme.Name;
					s.Remove (0, 2);
					if (m_phonems2Visems.ContainsKey (s)) {
						string viseme = "Visem" + m_phonems2Visems [s];
					
						if (!curvelist.ContainsKey (viseme)) {
							curvelist.Add (viseme, new AnimationCurve ());
						}
						curvelist [viseme].AddKey ((phoneme.Start / 1000.0f)-0.3f, 0.0f);

						if (phoneme.Duration < 0)
							curvelist [viseme].AddKey (((float)phoneme.Start / 1000.0f), phoneme.Duration/-100.0f);
						
					
						if (phoneme.Duration == 0)
						// Default
							curvelist [viseme].AddKey ((phoneme.Start / 1000.0f),50.0f);
						if (phoneme.Duration > 0) {
							// Default weight and a duration
							curvelist [viseme].AddKey ((phoneme.Start / 1000.0f),50.0f);
							curvelist [viseme].AddKey ((phoneme.Start +phoneme.Duration) / 1000.0f,0.0f);

						}
							
					} else {
						Debug.Log ("could not find " + s + " in list"); 
					}
					
					continue;
				}
				if (phoneme.Name.StartsWith ("\\callback")) {
					Debug.Log("callback");
					//output.addKey(100,phoneme.getStart() / 1000.0);
					continue;
				} else {
					// get viseme
					if (m_phonems2Visems.ContainsKey (phoneme.Name)) {
						string morphtargetname = m_phonems2Visems [phoneme.Name];
						float duration = phoneme.Duration / 1000.0f * m_scale;
						float start = phoneme.Start / 1000.0f - (duration - phoneme.Duration / 1000.0f) / 2.0f;
						float weight = phoneme.Duration / 100.0f;
						if (weight > 1.0f)
							weight = 1.0f;
						
						if (!curvelist.ContainsKey (morphtargetname)) {
							curvelist.Add (morphtargetname, new AnimationCurve ());
						}
						curvelist [morphtargetname].AddKey (start - duration * 0.15f, 0.0f);
						curvelist [morphtargetname].AddKey (start + duration * 0.15f, 50f*weight);
						curvelist [morphtargetname].AddKey (start + duration * 0.3f, 100.0f*weight);
						curvelist [morphtargetname].AddKey (start + duration *1.0f, 0.0f);
						
						
						
					} else {
						Debug.Log ("Unmapped phoneme: " + phoneme.Name);
					}

				}
			}
		}
		
		if(curvelist.Count>0)
		{
			AnimationClip ac = new AnimationClip ();
			foreach (KeyValuePair<string, AnimationCurve> kvp in curvelist)
			{
				ac.SetCurve (m_morphModifier.name, typeof(SpeakMorphAnim), "visem"+kvp.Key+"Percent", kvp.Value);
			}
			Debug.Log("added animation with: " + curvelist.Count + " curvses"); 
			return ac;//animation.AddClip (ac, "phonemize");
		}
		return null;
	}
	
	
	/// <summary>
	/// Request a remote Speak.
	/// </summary>
	/// <param name='inputtext'>
	/// The text to speak.
	/// </param>
	/// <param name='voice'>
	/// The voice to speak.
	/// </param>
	/// <param name='np'>
	/// the NetworkViewID who speaks
	/// </param>
	[RPC]
	void SpeakRequest (string inputtext, NetworkViewID np)
	{
		GameObject [] gos = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject go in gos) {
			if (go.networkView.viewID == np) {
				StartCoroutine (PrepareSpeak(go,inputtext));
				break;
			}
		}
	}
}