using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinLoader : MonoBehaviour
{
    public GameObject[] skinIndex; //Array to load all skins manually
    public static string skinName; //This is a static variable holding the current skin loaded, as only one can be active at time 
    public static int skinIndexValue;
    public static GameObject[] skinIndexStatic;
	
    private void Awake()
    {
        if (skinName != SkinSelectorController.currentSkin)
        {
            skinName = SkinSelectorController.currentSkin; //Get from player prefs the string of the current skin            
        }
        LoadSkin();
    }

	void Start ()
    {
        skinIndexStatic = skinIndex;          
	}	
	
    public void LoadSkin()
    {
        skinName = SkinSelectorController.currentSkin;

        switch (skinName)
        {
            case "mavenDefault":
                foreach (GameObject skin in skinIndex)
                {
                    if (!skin.name.Contains("Default"))
                    {
                        skin.SetActive(false);                        
                    }
                    else
                    {
                        skin.SetActive(true);
                        skinIndexValue = 1;                        
                    }
                }
                break;
          
            case "mavenFounders":
                foreach (GameObject skin in skinIndex)
                {
                    if (skin.name.Contains("Founders"))
                    {
                        skin.SetActive(true);
                    }
                    else
                    {
                        skin.SetActive(false);
                        skinIndexValue = 2;
                    }
                }
                break;

            case "mavenExplorer":
                foreach (GameObject skin in skinIndex)
                {
                    if (skin.name.Contains("Explorer"))
                    {
                        skin.SetActive(true);
                    }
                    else
                    {
                        skin.SetActive(false);
                        skinIndexValue = 3;
                    }
                }
                break;

            case "mavenRhino":
                foreach (GameObject skin in skinIndex)
                {
                    if (skin.name.Contains("Rhino"))
                    {
                        skin.SetActive(true);
                    }
                    else
                    {
                        skin.SetActive(false);
                        skinIndexValue = 4;
                    }
                }
                break;

            case "mavenPharaon":
                foreach (GameObject skin in skinIndex)
                {
                    if (skin.name.Contains("Pharaon"))
                    {
                        skin.SetActive(true);
                    }
                    else
                    {
                        skin.SetActive(false);
                        skinIndexValue = 5;
                    }
                }
                break;
        }
    }


}
