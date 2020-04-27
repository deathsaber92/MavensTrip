using UnityEngine;

public class MenuStartParameters : MonoBehaviour {

	void Start ()
    {
        MainMenuHandler.menuLoaded.Value = true;   
        Application.targetFrameRate = 60;
    }
}
