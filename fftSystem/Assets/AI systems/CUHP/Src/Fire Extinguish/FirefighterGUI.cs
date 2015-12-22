using UnityEngine;
using System.Collections;

public class FirefighterGUI : MonoBehaviour {

    // FIELDS

    /// <summary>
    /// Rectangle for the pop-up window (this is required to make the window dragable)
    /// </summary>
    private Rect windowRect = new Rect(110, 10, 520, 200);

    /// <summary>
    /// Denotes if something went wrong in the planning interface
    /// </summary>
    private bool somethingWrong = false;


    // METHODS

    /// <summary>
    /// Shows the GUI elements
    /// </summary>
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 80, 30), "Extinghuish"))
        {
            if (GetComponent<Interface>())
            {
                somethingWrong = !(GetComponent<Interface>().Extinguishfire());
            }
        }

        if (somethingWrong)
            GUI.Label(new Rect(10, 90, 300, 30), "Sorry, something went wrong.");

    }

}
