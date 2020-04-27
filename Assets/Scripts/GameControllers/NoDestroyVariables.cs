using UnityEngine;

public class NoDestroyVariables : MonoBehaviour
{
    public static bool lowOn;
    public static bool medOn;
    public static bool highOn;  

    public static NoDestroyVariables NoDestroyInstance { get; private set; } 

    private void Awake()
    {
        if (NoDestroyInstance == null)
        {
            NoDestroyInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadPlayerPrefs();
    }

    /// <summary>
    /// call to save the graphic prefs based on the player pick
    /// </summary>
    public static void SaveGraphicsPrefs()
    {
        PlayerPrefs.SetInt(PlayerPrefsStrings.lowOn, (lowOn ? 1 : 0));
        PlayerPrefs.SetInt(PlayerPrefsStrings.medOn, (medOn ? 1 : 0));
        PlayerPrefs.SetInt(PlayerPrefsStrings.highOn, (highOn ? 1 : 0));     

        PlayerPrefs.Save();
    }

    public static void LoadPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsStrings.firstGameLaunch))
        {
            PlayerPrefs.SetInt(PlayerPrefsStrings.lowOn, 0);
            PlayerPrefs.SetInt(PlayerPrefsStrings.medOn, 1);
            PlayerPrefs.SetInt(PlayerPrefsStrings.highOn, 0);    
            PlayerPrefs.SetInt(PlayerPrefsStrings.firstGameLaunch, 1);
        }
     
        lowOn = (PlayerPrefs.GetInt(PlayerPrefsStrings.lowOn) != 0);
        medOn = (PlayerPrefs.GetInt(PlayerPrefsStrings.medOn) != 0);
        highOn = (PlayerPrefs.GetInt(PlayerPrefsStrings.highOn) != 0);
       
        PlayerPrefs.Save();
    }
}
