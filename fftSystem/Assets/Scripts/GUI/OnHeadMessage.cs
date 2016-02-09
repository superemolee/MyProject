using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Firefighter.Utilities;

/// <summary>
/// Message on the head indicates what is the character doing.
/// </summary>
public class OnHeadMessage : MonoBehaviour
{

	public Camera cam;

	private Text text;
	private RescuerGeneral ft;
	private Tasks currentTask;

	private Tasks lastTask;

	public float fadeDuration = 1.0f;

	// Use this for initialization
	void Start ()
	{
		text = GetComponentInChildren<Text> ();
		if (ft == null) {
			ft = (RescuerGeneral)GetComponentInParent<FirefighterToolOperator> ();
		}
		if (ft == null) {
			ft = (RescuerGeneral)GetComponentInParent<FirefighterBackUp> ();
		}
		if (ft == null) {
			ft = (RescuerGeneral)GetComponentInParent<FirefighterLeader> ();
		}
		if (ft == null) {
			ft = (RescuerGeneral)GetComponentInParent<Paramedic> ();
		}

		if (ft != null)
			currentTask = ft.runningTask;

		StartCoroutine(StartFading());

	}
	
	// Update is called once per frame
	void Update ()
	{
		// the orientation of the text should toward to the camara cam
		if (cam != null)
			transform.rotation = Quaternion.LookRotation (cam.transform.forward, cam.transform.up);

		// update currentTask
		if (ft != null)
			currentTask = ft.runningTask;

		// show the current task on GUI
		// text will be disappeared in a short time
		if (text != null && currentTask != null) {
			if (currentTask == lastTask) {
				text.text = currentTask.ToString ();
			}else{
				StartCoroutine(StartFading());
				text.color = new Color(text.color.r,text.color.g,text.color.b, 1f);
				lastTask = currentTask;
			}
		}

	}
	

	private IEnumerator StartFading()
	{
		yield return new WaitForSeconds(3);
		yield return StartCoroutine(Fade(1.0f, 0.0f, fadeDuration));
	}
	

	private IEnumerator Fade (float startLevel, float endLevel, float time)
	{
		float speed = 1.0f/time;
		
		for (float t = 0.0f; t < 1.0; t += Time.deltaTime*speed)
		{
			float a = Mathf.Lerp(startLevel, endLevel, t);
			text.color = new Color(text.color.r,text.color.g,text.color.b, a);
			yield return 0;
		}
	}

}
