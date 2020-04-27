using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CHECKED FOR UNUSED VARIABLES
public class RelicController : MonoBehaviour
{   
    private GameObject relicScreen;
    public static int levelIndex = 1;

    public static string relicType = "none"; 

    private void Start()
    {
        relicScreen = FindObjectOfType<InterfaceHandler>().relicControllerScreen;   
    }

    private void Update()
    {
        //Debug.Log("relic type: " + relicType);
    }

    public void OnJumpRelicSelect()
    {
        relicType = "jump_power";
        FindObjectOfType<AudioManager>().PlaySound("buttonSound");
        DeactivateMenuInterfaceElements();
        FindObjectOfType<LevelLoader>().LoadLevel(levelIndex);        
    }

    public void OnSpeedRelicSelect()
    {
        relicType = "movement_speed";
        FindObjectOfType<AudioManager>().PlaySound("buttonSound");
        DeactivateMenuInterfaceElements();
        FindObjectOfType<LevelLoader>().LoadLevel(levelIndex);
    }

    public void OnAutomaticInteractRelicSelect() 
    {
        relicType = "explosion_resistance";
        FindObjectOfType<AudioManager>().PlaySound("buttonSound");
        DeactivateMenuInterfaceElements();
        FindObjectOfType<LevelLoader>().LoadLevel(levelIndex);
    }

    private void DeactivateMenuInterfaceElements()
    {
        relicScreen.SetActive(false);
        MainMenuHandler.menuLoaded.Value = false;
        MainMenuHandler.mainMenuScreen.SetActive(false);
        MainMenuHandler.mainMenuEnvironment.SetActive(false);
    }
}
