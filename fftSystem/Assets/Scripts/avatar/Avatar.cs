using UnityEngine;
//using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// This Class provides basic methods to control an avatar. 
/// </summary>
[RequireComponent(typeof(Transform))]
//[RequireComponent(typeof(Animation))]
public class Avatar : MonoBehaviour
{
	  /// <summary>
    /// current Animation as string (walk,idle)
    /// </summary>
    private string myCurrentAnimation = "";
   
	public GameObject avatar;
	public bool canSpeak=false;
	private Animation avatarAnimation;
	private Animation avatarSound;
	
	
    private NetworkView generalNetworkView;
	
	private float lastDistance = -1.0f;
	
	public float mySpeed = 1.0f;
	
	public float myDistanceReached=3.0f;
	
	/// <summary>
	/// Identifies if this avatar is controlled from outside (avatar is no player). 
	/// </summary>
	public bool myIsExternalControlled=false;
	
	private Vector3 myWalkGoal = Vector3.zero;


    // Use this for initialization
    void Start()
    {
        //myAnimation = transform.GetComponentInChildren<Animation>();
		if(avatar!=null)
		{
			avatarAnimation = avatar.animation;
			
		}else{
			avatarAnimation = animation;
		}
		
		
        avatarAnimation["walk"].wrapMode = WrapMode.Loop;
        avatarAnimation["idle"].wrapMode = WrapMode.Loop;
        myCurrentAnimation = "idle";
		if(!networkView.isMine)
		{

		}
    }
	
	/// <summary>
	/// Walk to a gameobject provided by the string
	/// </summary>
	/// <param name="theGameObject"> the name of the gameobject.
	/// A <see cref="System.String"/>
	/// </param>
	public void WalkToPosition(string theGameObject)	
	{
		GameObject go = GameObject.Find(theGameObject);
		WalkToPosition(go);
		
	}
	public void WalkToPosition(GameObject go)
	{
		if(go)
		{
			myWalkGoal = go.transform.position;
			ChangeAnimation("walk");
		}
	}
	
	/// <summary>
	/// Handle a gameItem. Avatar have to be in a distance of 3 m. 
	/// </summary>
	/// <param name="gameItemName"> the name of the game Item.
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="gameItemAction"> the action which should be executed.
	/// A <see cref="System.String"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/> which identifies the success of the operation
	/// </returns>
	public bool HandleGameItem(string gameItemName, string gameItemAction)
	{
//		GameItem gi = GameObject.Find(gameItemName).GetComponent<GameItem>();
//		if(gi)
//		{
//			if(Vector3.Distance(gi.transform.position, transform.position)< 3.0f)
//			{
//				if(gi.ExternalUserAction(gameItemAction))
//				{
//					TaskRecorder.AddTask(Time.timeSinceLevelLoad, gi.GetInstanceID(), gi.name, GetInstanceID(), name, gameItemAction);
//					return true;
//				}
//			}
//		}
		return false;
		
	}
	
	
	/// <summary>
	/// Prepares and executes a speech by the given text 
	/// </summary>
	/// <param name="text"> The text to speech. 
	/// A <see cref="System.String"/> 
	/// </param>
	/// <returns> Success of the operation. 
	/// A <see cref="System.Boolean"/>
	/// </returns>
//	public bool Say(string text)
//	{
//		Speak sp = gameObject.GetComponent<Speak>();
//		sp.enabled = true;
//		sp.sayText(text);
//		sp.enabled = false;
//		return true;
//}
	
	
	
    // Update is called once per frame
    void Update()
    {
		if(myIsExternalControlled)
		{
			
			if(myWalkGoal!=Vector3.zero)
			{
				//ChangeAnimation("walk");
				Vector3 posWOY=myWalkGoal;
				posWOY[1]=gameObject.transform.position[1];
				gameObject.transform.LookAt(posWOY);
				
	//			gameObject.GetComponent<CharacterMotor>().inputMoveDirection =  gameObject.transform.forward;
//				
//				gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, myWalkGoal, Time.deltaTime);
				float distance = Vector3.Distance(gameObject.transform.position, myWalkGoal);
				//Debug.Log(distance);
				if(distance < myDistanceReached)
				{
				//	gameObject.GetComponent<CharacterMotor>().inputMoveDirection = Vector3.zero;
					myWalkGoal = Vector3.zero;
					gameObject.transform.LookAt(posWOY);
					ChangeAnimation("idle");
				}
//				//check if distance changed in small delta
//				if(lastDistance >0 && System.Math.Abs(lastDistance - distance) < 0.01f)
//				{
//					lastDistance = -1.0f;
//					gameObject.GetComponent<CharacterMotor>().inputMoveDirection = Vector3.zero;
//					myWalkGoal = Vector3.zero;
//					gameObject.transform.LookAt(posWOY);
//					ChangeAnimation("idle");
//				}
			
				lastDistance = distance;  
			}
		}else{
		
		
	        //myAnimation = transform.GetComponentInChildren<Animation>();
	        float x = Input.GetAxis("Horizontal");
	        float y = Input.GetAxis("Vertical");
	        float currentSpeed = Mathf.Sqrt(x * x + y * y);
	        if (currentSpeed > 0.1)
	        {
	            if (myCurrentAnimation != "walk")
	            {
					
	                ChangeAnimation("walk", (y < 0));
	            }
	        }
	        else
	        {
	            if (myCurrentAnimation != "idle")
	            {
	                ChangeAnimation("idle");
	            }
	        }
		}

		
//		if(Input.GetKeyDown("m"))
//		{
//			WalkToPosition("verteiler_noskin");
//		}
//		if(Input.GetKeyDown("n"))
//		{
//			if(myIsExternalControlled)
//				HandleGameItem("verteiler_noskin", "TRANSPORTABLE");
//		}
//		if(Input.GetKeyDown(","))
//		{
//			if(myIsExternalControlled)
//				Say("Wasser Marsch!");
//		}
		
		
    }
	
	/// <summary>
	/// Speak function for using from outside this component. This locks also the console 
	/// </summary>
	public void Speak()
	{
//		Speak sp = GetComponent<Speak>();
//		sp.enabled = true;
//		MonoBehaviour[] excl={sp};
//		InputSwitcher.LockInput(gameObject,excl);
	}
	
	public void ChangeAnimation(string anim, bool isForward)
	{
		
		if(Network.peerType!=NetworkPeerType.Disconnected)
		{
        	networkView.RPC("SendAnimationChange", RPCMode.Others, networkView.viewID, anim, isForward);
			
			animation[anim].normalizedSpeed = isForward?1:-1;
			animation.CrossFade(anim);
		}
		else
		{
			
			animation.wrapMode = WrapMode.Loop;

			animation[anim].normalizedSpeed = isForward?1:-1;
			animation.CrossFade(anim);

		}
		myCurrentAnimation = anim;
	}
	

	/// <summary>
	/// Change the Animation of the avatar and send the change over the network.  
	/// </summary>
	/// <param name="anim"> The name of the animation.
	/// A <see cref="System.String"/>
	/// </param>
	public void ChangeAnimation(string anim)
	{
		ChangeAnimation(anim, true);

	}

    void OnNetworkInstantiate(NetworkMessageInfo msg)
    {
        // This is our own player
        if (networkView.isMine)
        {
            //Namen vergeben
            GameObject go = GameObject.Find("GeneralScripts");
			if(go!=null)
			{
				generalNetworkView = go.networkView;
				//generalNetworkView.RPC("initCams",RPCMode.Server);
			}
        }
        // This is just some remote controlled player, don't execute direct
        // user input on this. DO enable multiplayer control
        else
        {
//            thisName = "Remote" + UnityEngine.Random.Range(1, 10);
//            name += thisName;

            //Skripte auf diesen Remote Playern ausschalten
//			CharacterMotor cm = GetComponent<CharacterMotor>();
//			if(cm != null){
//				GetComponent<CharacterMotor>().enabled = false;
//			}
//
//			FPSInputController fpsic = GetComponent<FPSInputController>();
//			if(fpsic != null){
//				fpsic.enabled = false;
//			}
//			MouseLookScript ml = GetComponent<MouseLookScript>();
//			if(ml != null){
//            	ml.enabled = false;	
//			}
//			GameItemHandler gih = GetComponent<GameItemHandler>();
//			if(gih != null){
//				gih.enabled = false;
//			}
//          
//            //GetComponent("MouseLook").active = false;
//            //tmp5.enabled = false;
//
//            //networkView.RPC("askName", networkView.viewID.owner, Network.player);
//			//Transform myCamera = transform.FindChild("CameraBack");
//            Transform myCamera = transform.FindChild(/*"Bip01/Bip01 Pelvis/*/"CameraHelper/CameraBack");
//			if(myCamera != null){
//				myCamera.gameObject.GetComponent<AudioListener>().enabled = false;
//				myCamera.gameObject.GetComponent<MouseLook>().enabled = false;
//			
//            	myCamera.camera.enabled = false;
//			}
//			this.enabled = false;

			
			
        }

    }
	
	

    /// <summary>
    /// Sendet an alle die Nachricht, dass bei einem bestimmten Spieler die Animation geändert werden muss.
    /// </summary>
    /// <param name="viewID">Von wem die Animation geaendert werden muss</param>
    /// <param name="anim">Welche Animation abgespielt werden soll</param>
    [RPC]
    void SendAnimationChange(NetworkViewID viewID, string anim, bool isForward)
    {
        GameObject player = NetworkView.Find(viewID).gameObject;
		if(player)
		{
			Animation myAnimation = player.GetComponentInChildren<Animation>();
        	myAnimation.CrossFade(anim);
        	myAnimation.wrapMode = WrapMode.Loop;
		}

    }

    /// <summary>
    /// Setzt den Namen fuer diesen Spieler
    /// </summary>
    /// <param name="name"></param>
    [RPC]
    void setName(string name)
    {
        this.name = name;
    }

    /// <summary>
    /// Der Besitzer der NetworkView wird gefragt wie sein Spieler heisst 
    /// </summary>
    /// <param name="asker">Der Anfrager</param>
    [RPC]
    void askName(NetworkPlayer asker)
    {
        networkView.RPC("setName", asker, this.name);
    }

}
