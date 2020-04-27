using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphincsLevelsMarkersController : MonoBehaviour {

    public GameObject lowMarkers;
    public GameObject medMarkers;
    public GameObject highMarkers;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsStrings.firstGameLaunch))
        {
            if(PlayerPrefs.GetInt(PlayerPrefsStrings.firstGameLaunch) == 0)
            {
                FindObjectOfType<MainMenuHandler>().MedSettingsDefault();
                PlayerPrefs.SetInt(PlayerPrefsStrings.firstGameLaunch, 1);
            }
            
            UpdateMarkers();
        }
        else
        {
            UpdateMarkers();
        }
    }

    // Use this for initialization
    void Start ()
    {
        UpdateMarkers();
    }
    
    /// <summary>
    /// Update current graphic settings markers
    /// </summary>
    public void UpdateMarkers()
    {
        NoDestroyVariables.LoadPlayerPrefs();
       
        if (NoDestroyVariables.lowOn)
        {
            lowMarkers.SetActive(true);
            medMarkers.SetActive(false);
            highMarkers.SetActive(false);           
        }
        else if (NoDestroyVariables.medOn)
        {
            lowMarkers.SetActive(false);
            medMarkers.SetActive(true);
            highMarkers.SetActive(false);           
        }
        else if (NoDestroyVariables.highOn)
        {
            lowMarkers.SetActive(false);
            medMarkers.SetActive(false);
            highMarkers.SetActive(true);            
        }       
    }
}
