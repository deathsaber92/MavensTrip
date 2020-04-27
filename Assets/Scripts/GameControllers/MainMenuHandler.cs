using UnityEngine;
using UniRx;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx.Triggers;
using System;
using UnityEngine.SocialPlatforms;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuHandler : MonoBehaviour
{
    public TextMeshProUGUI debugReporter;

    private static GameObject[] mavenThrusters = new GameObject[2];
    private static ParticleSystem[] mavenThrustersParticleSystem = new ParticleSystem[2];
    private ReactiveProperty<bool> displayThrusters = new ReactiveProperty<bool>(false);

    //Pipeline assets quality settings
    public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset lowQuality;
    public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset medQuality;
    public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset highQuality;

    //Game state static booleans
    public static ReactiveProperty<bool> levelLoaded = new ReactiveProperty<bool>(false);
    public static ReactiveProperty<bool> menuLoaded = new ReactiveProperty<bool>(true);

    //Audio
    public AudioManager audioManager;

    //Canvas
    public Canvas canvas;
    public static GameObject mainMenuScreen;
    public static GameObject mainMenuEnvironment;
    public GameObject shop;
    public GameObject mainMenuItems;

    //1stLevel items
    private GameObject firstLevelItems;

    //popups
    private GameObject reportABugScreen;
    private GameObject bugReportWasSentScreen;
    private GameObject relicScreen;
    private GameObject levelSelectorScreen;    
    private GameObject youDontOwnThisSkinPopup;
    private GameObject youDontHaveEnoughDiamonds;
    private GameObject timeModeReleaseDatePopup;
    private GameObject relicScreenTutorial;
    public GameObject upgradesScreen;

    public GameObject soundON;
    public GameObject soundOFF;

    //2ndLevel items
    private GameObject settingsLevels;  
    private Button lowSettings;
    private Button medSettings;
    private Button highSettings;  
    private Button graphicsSet;
    private Button trainingRoom;
    private InterfaceHandler handler;
    
    //Temporary variables for graphics settings to be able to revert graphic settings  
    private bool lowOn_Temp;
    private bool medOn_Temp; 
    private bool highOn_Temp;

    private int unityQualitySettingsIndex;
    private float timeSinceMenuLoad = 0;

    //Holder for DeltaTime.timeDeltaTime
    public static float deltaTime;

    protected void Start()
    {
        displayThrusters = new ReactiveProperty<bool>(false);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;     
        GetInterfaceElements();
        canvas = canvas.GetComponent<Canvas>();
        PopulateCanvases();

        if (!PlayerPrefs.HasKey("accessSettings"))
        {
           MedSettingsDefault(); 
        }

        displayThrusters
            .Where(displayThrusters => displayThrusters == true)            
            .Subscribe(_ => PlayThrustersParticleEffect())
            .AddTo(this);
    }
    
    public void SyncCurrencyOnLogin()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsStrings.currencyNeedsSync) == 1 && Application.internetReachability != NetworkReachability.NotReachable)
        {
            debugReporter.text = debugReporter.text + "\n" + "SyncCurrencyWhenOnline() called and needs to add: " + PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync).ToString();
            debugReporter.text = debugReporter.text + "\n" + "SyncCurrencyWhenOnline() called and needs to substract: " + PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync).ToString();

            CurrencyController.SyncCurrencyWhenOnline();
        }
    }

    private void GetSkinsFromCurrent()
    {
        mavenThrusters = GameObject.FindGameObjectsWithTag("jumpthrusters");
        mavenThrustersParticleSystem[0] = mavenThrusters[0].GetComponent<ParticleSystem>();
        mavenThrustersParticleSystem[1] = mavenThrusters[1].GetComponent<ParticleSystem>();
    }

    private void PlayThrustersParticleEffect()
    {
        audioManager.PlaySound("thrusters");
        mavenThrustersParticleSystem[0].Play();
        mavenThrustersParticleSystem[1].Play();        
    }

    private void StopPlayingThrustersPlarticleEffect()
    {
        audioManager.StopWithFadeAtEnd("thrusters");
        mavenThrustersParticleSystem[0].Stop();
        mavenThrustersParticleSystem[1].Stop();
    }

    public void OnPointerUp() 
    {
        StopPlayingThrustersPlarticleEffect();
        displayThrusters.Value = false;
    }

    public void OnInvisibleThrustersDisplayButtonDown()
    {
        GetSkinsFromCurrent();
        displayThrusters.Value = true;
    }

    /// <summary>
    /// Get all the interface elements
    /// </summary>   
    private void GetInterfaceElements()
    {        
        handler = FindObjectOfType<InterfaceHandler>();
        settingsLevels = handler.settingsLevels;
        lowSettings = handler.lowSettings;
        medSettings = handler.medSettings;
        highSettings = handler.highSettings;
        graphicsSet = handler.graphicsSet;
        bugReportWasSentScreen = handler.bugReportWasSentScreen;
        relicScreen = handler.relicControllerScreen;
        trainingRoom = handler.trainingRoom;

        youDontOwnThisSkinPopup = handler.youDontOwnThisSkinPopup;
        youDontHaveEnoughDiamonds = handler.youDontHaveEnoughDiamonds;
        mainMenuEnvironment = handler.menuEnvironment;
        mainMenuScreen = handler.menuInterface;
        firstLevelItems = handler.firstLevelItems;
    }

    #region MainMenu
    //Open up the graphic settings buttons

    /// <summary>
    /// Graphics button pressed
    /// </summary>
    public void GraphicsButton()
    {
        audioManager.PlaySound("buttonSound");
        PlayerPrefs.SetInt(PlayerPrefsStrings.firstTimeInSettings, 1);     
        settingsLevels.gameObject.SetActive(true);
        firstLevelItems.gameObject.SetActive(false);
        GetCurrentGraphicsSettings();
    }

    
    /// <summary>
    /// Upgrades tab opening
    /// </summary>
    public void UpgradesButton()
    {
        audioManager.PlaySound("buttonSound");
        //this is safe because the internet is already used to connect and check should not fail
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            FindObjectOfType<UpgradesAndGameplayController>().GetUpgradePlayerData(true);            
            upgradesScreen.SetActive(true);
            mainMenuItems.SetActive(false);
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("PLEASE CONNECT TO THE INTERNET TO MAKE UPGRADES!");
        }        
    }
  
   
    /// <summary>
    /// Back was pressed on video settings
    /// </summary>
    public void BackFromVideoSettings()
    {
        audioManager.PlaySound("buttonSound");
        settingsLevels.gameObject.SetActive(false);
        firstLevelItems.gameObject.SetActive(true);        
    }

    /// <summary>
    /// Back was pressed on relic screen
    /// </summary>
    public void BackFromRelicScreen()
    {
        audioManager.PlaySound("buttonSound");
        if (!audioManager.IsThisPlaying("menuTheme"))
        {
            audioManager.PlaySound("menuTheme");
        }
        relicScreen.SetActive(false);
        mainMenuItems.SetActive(true);
        mainMenuScreen.SetActive(true);
        mainMenuEnvironment.SetActive(true);
    }

    /// <summary>
    /// Back was pressed in shop
    /// </summary>
    public void BackFromShop()
    {
        audioManager.PlaySound("buttonSound");
        shop.SetActive(false);
        mainMenuItems.SetActive(true);
    }

    /// <summary>
    /// Button to start the game, it also checks if the wanted skin is owned first
    /// </summary>
    public void PlaySoloButton()
    {
        audioManager.PlaySound("buttonSound");

        if (SkinLockStatusManager.currentSkinOwned)
        {
            debugReporter.text = debugReporter.text + "\n" + "PlayButton() is current skin owned? :" + SkinLockStatusManager.currentSkinOwned;
            relicScreen.SetActive(true);
            mainMenuItems.SetActive(false);
            PlayerPrefs.SetInt(PlayerPrefsStrings.setStartSkin, SkinLoader.skinIndexValue);
        }        
        else
        {
            debugReporter.text = debugReporter.text + "\n" + "PlayButton() is current skin owned? :" + SkinLockStatusManager.currentSkinOwned;
            youDontOwnThisSkinPopup.SetActive(true);
        }
    }    
   
    public void SoundONPress()
    {
        audioManager.MuteSound();
        soundON.SetActive(false);
        soundOFF.SetActive(true);
    }

    public void SoundOFFPress()
    {
        audioManager.ResumeSound();
        audioManager.PlaySound("buttonSound");
        soundOFF.SetActive(false);
        soundON.SetActive(true);
    }

    public void OkButton()
    {
        audioManager.PlaySound("buttonSound");
        youDontOwnThisSkinPopup.SetActive(false);
        youDontHaveEnoughDiamonds.SetActive(false);
    }   
    #endregion   

    #region Quality_Settings
    
    //Set low settings
    public void LowSettings()
    {
        audioManager.PlaySound("buttonSound");

        GraphicsSettings.renderPipelineAsset = lowQuality;

        NoDestroyVariables.lowOn = true;
        NoDestroyVariables.medOn = false;
        NoDestroyVariables.highOn = false;
      
        canvas.enabled = false;
        Canvas.ForceUpdateCanvases();
        canvas.enabled = true;

        NoDestroyVariables.SaveGraphicsPrefs();
        FindObjectOfType<GraphincsLevelsMarkersController>().UpdateMarkers();
    }

    //Set medium settings
    public void MedSettings()
    {
        audioManager.PlaySound("buttonSound");

        GraphicsSettings.renderPipelineAsset = medQuality;

        NoDestroyVariables.lowOn = false;
        NoDestroyVariables.medOn = true;
        NoDestroyVariables.highOn = false;
       
        canvas.enabled = false;
        Canvas.ForceUpdateCanvases();        
        canvas.enabled = true;

        NoDestroyVariables.SaveGraphicsPrefs();
        FindObjectOfType<GraphincsLevelsMarkersController>().UpdateMarkers();
    }

    public void MedSettingsDefault()
    {
        GraphicsSettings.renderPipelineAsset = medQuality;

        NoDestroyVariables.lowOn = false;
        NoDestroyVariables.medOn = true;
        NoDestroyVariables.highOn = false;
      
        NoDestroyVariables.SaveGraphicsPrefs();        
    }

    //Set high settings
    public void HighSettings()
    {
        audioManager.PlaySound("buttonSound");

        GraphicsSettings.renderPipelineAsset = highQuality;

        NoDestroyVariables.lowOn = false;
        NoDestroyVariables.medOn = false;
        NoDestroyVariables.highOn = true;
   
        canvas.enabled = false;
        Canvas.ForceUpdateCanvases();        
        canvas.enabled = true;

        NoDestroyVariables.SaveGraphicsPrefs();
        FindObjectOfType<GraphincsLevelsMarkersController>().UpdateMarkers();
    }
  
    #endregion

    private void PopulateCanvases()
    {
        lowSettings = lowSettings.GetComponent<Button>();
        medSettings = medSettings.GetComponent<Button>();
        highSettings = highSettings.GetComponent<Button>();
       
        graphicsSet = graphicsSet.GetComponent<Button>();
        trainingRoom = trainingRoom.GetComponent<Button>();     
    }

    private void GetCurrentGraphicsSettings()
    {
        lowOn_Temp = NoDestroyVariables.lowOn;
        medOn_Temp = NoDestroyVariables.medOn;
        highOn_Temp = NoDestroyVariables.highOn;
       
        unityQualitySettingsIndex = QualitySettings.GetQualityLevel();
    }

    private void RevertToTempGraphicsSettings()
    {
        NoDestroyVariables.lowOn = lowOn_Temp;
        NoDestroyVariables.medOn = medOn_Temp;
        NoDestroyVariables.highOn = highOn_Temp;      
        QualitySettings.SetQualityLevel(unityQualitySettingsIndex);
    }    
}

