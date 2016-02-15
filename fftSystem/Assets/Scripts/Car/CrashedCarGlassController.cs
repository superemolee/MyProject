using UnityEngine;
using System.Collections;

public class CrashedCarGlassController : MonoBehaviour
{

    public Material myMats;
    public Texture2D myTexture;
    public Shader myShader;
    public Color32 myColor;

    public bool isManaged;
    public bool isTaped;


    void Awake(){
        myShader = Shader.Find("Transparent/Diffuse");
        isManaged = false;
        isTaped = false;
    }

    // Use this for initialization
    void Start()
    {
        if (myMats == null && myShader != null)
        {
            myMats = new Material(myShader);
            gameObject.renderer.material = myMats;

        }
        if (myTexture != null)
        {
            gameObject.renderer.material.mainTexture = myTexture;
        }
        myColor = new Color32(0, 0, 0, 102);
        myMats.color = myColor;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void assignColor()
    {
            myColor = new Color32(0, 0, 0, 102);
            myMats.color = myColor;
    }

    public void assignTexture()
    {
        // TODO: the current car dont have UV... 
        // so that a texture is not possible to assign.
        // hereby I only change the color..
        myColor = new Color32(255,255,255,200);
        myMats.color = myColor;
//        if (myTexture == null)
//        {
//            myTexture = Resources.Load("Textures/brockglass") as Texture2D;
//        }
//        gameObject.renderer.material.mainTexture = myTexture;
    }

}
