using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    private static GameObject[] tutorialComponents;
    private static bool wasDeactivated = false;

    private void Update()
    {
        if (MavenMovementControl.movementAfterRespawn.Value == true && wasDeactivated == false)
        {
            DeactivateTutorialObjects();            
        }       
    }

    public static void ActivateTutorialObjects()
    {
        if (tutorialComponents != null)
        {
            if (tutorialComponents.Length == 0)
            {
                tutorialComponents = GameObject.FindGameObjectsWithTag("tutorial_objects");
            }
        }        
        else
        {
            tutorialComponents = GameObject.FindGameObjectsWithTag("tutorial_objects");
        }
           
        if (MavenMovementControl.movementAfterRespawn.Value == false && MainMenuHandler.levelLoaded.Value == true)
        {
            foreach (GameObject tutorialObject in tutorialComponents)
            {
                tutorialObject.SetActive(true);
            }
        }
        wasDeactivated = false;
    }

    public static void DeactivateTutorialObjects()
    {
        tutorialComponents = GameObject.FindGameObjectsWithTag("tutorial_objects");
        foreach (GameObject tutorialObject in tutorialComponents)
        {
            tutorialObject.SetActive(false);            
        }
        wasDeactivated = true;
    }
}
