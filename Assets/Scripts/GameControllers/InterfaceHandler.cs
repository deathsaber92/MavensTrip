using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InterfaceHandler : MonoBehaviour
{
    public AudioManager audioManager;
    public GameObject loadingScreen;
    public GameObject menuInterface;
    public GameObject menuEnvironment;
    public GameObject gameOverScreen;
    public GameObject duringGameUI;
    public GameObject pauseGamePopup;
    public GameObject explosionResistRelicButton;
    public GameObject jumpRelicButton;
    public GameObject speedRelicButton;
    public GameObject lowMarkers;
    public GameObject medMarkers;
    public GameObject highMarkers;
    public GameObject relicControllerScreen;
    public GameObject firstLevelItems;
    public GameObject quitGameGraphicsPopup;
    public GameObject timeModeReleaseDatePopup;
    public GameObject scoreModeReleaseDatePopup;
    public GameObject settingsTutorial; //NOT REFACTORED 
    public GameObject bugReportWasSentScreen;
    public GameObject settingsLevels;
    public GameObject creditsScreen;
    public GameObject youDontOwnThisSkinPopup;
    public GameObject youDontHaveEnoughDiamonds;
    public GameObject skinPurchasePopup;
    public GameObject notEnoughCurrencyPopup;
    public GameObject highlightRelic;
    public GameObject relicTimeHolder;
    public Button graphicsSet;
    public Button lowSettings;
    public Button medSettings;
    public Button highSettings;
    public Button trainingRoom;
    public TextMeshProUGUI numberOfDiamonds;
    public TextMeshProUGUI purchaseValueInPopup;
    public TextMeshProUGUI relicCharge;
    public TextMeshProUGUI relicTimer;
    public TextMeshProUGUI relicChargeMax;
    public FixedJoystick fixedJoystick;
    public TextMeshProUGUI debugReporter;
    public Slider sliderLevelLoader;
    public GameObject mainMenuItems;
    public GameObject welcomeScreen;

    /// <summary>
    /// Resume level from pause state
    /// </summary> 
    public void ResumeLevelFromPause()
    {
        pauseGamePopup.SetActive(false);
        GameManager.ResumeGame();
    }

    /// <summary>
    /// Game pause pressed will display the paused game popup
    /// </summary>
    public void GamePausePopup()
    {
        pauseGamePopup.SetActive(true);
        GameManager.PauseGame();        
    }

    /// <summary>
    /// Replay level button pressed
    /// </summary>
    public void OnReplayLevelPress()
    {
        MavenMovementControl.movementAfterRespawn.Value = false;
        MainMenuHandler.levelLoaded.Value = false;
        Time.timeScale = 1;
        FindObjectOfType<UpgradesAndGameplayController>().SetGameplayValuesByUpgradeIndex();
        gameOverScreen.SetActive(false);
        speedRelicButton.SetActive(false);
        jumpRelicButton.SetActive(false);
        explosionResistRelicButton.SetActive(false);        

        if (pauseGamePopup.activeSelf)
        {
            pauseGamePopup.SetActive(false);
        }
        
        GameManager.collectedDiamonds.Value = 0;
        RelicUsageController.timeSinceRelicPressed.Value = 0;
        RelicUsageController.isRelicActive = false;
        RelicUsageController.relicChargeValue.Value = RelicUsageController.relicChargesMax;
        FindObjectOfType<GameOverScreenController>().SaveAndEnd();
        PlayerPrefs.SetInt(PlayerPrefsStrings.updatedScore, 0);
        GameOverScreenController.alreadyRevived = false;
        Time.timeScale = 1;

        FindObjectOfType<LevelLoader>().LoadLevel(0);
        FindObjectOfType<InterfaceHandler>().menuInterface.SetActive(true);
        FindObjectOfType<InterfaceHandler>().menuEnvironment.SetActive(true);
        FindObjectOfType<MainMenuHandler>().PlaySoloButton();
    }

    /// <summary>
    /// Exit to menu
    /// </summary>
    public void ExitToMenu()
    {
        FindObjectOfType<GameOverScreenController>().SaveAndEnd();
        Time.timeScale = 1;        
        debugReporter.text = debugReporter.text + "\n" + "Exit to menu called";
        MainMenuHandler.levelLoaded.Value = false;
        gameOverScreen.SetActive(false);
        pauseGamePopup.SetActive(false);
        duringGameUI.SetActive(false);
        GameOverScreenController.gameOver.Value = false;
        PlayerPrefs.SetInt(PlayerPrefsStrings.updatedScore, 0);
        var loadSceneIndex = 0;
        jumpRelicButton.SetActive(false);
        speedRelicButton.SetActive(false);
        explosionResistRelicButton.SetActive(false);
        FindObjectOfType<LevelLoader>().LoadLevel(loadSceneIndex);        
        audioManager.PlaySound("menuTheme");
        menuInterface.SetActive(true);
        menuEnvironment.SetActive(true);
        mainMenuItems.SetActive(true);
        FindObjectOfType<ChestObject>().CheckPlayFabServerTime();
    }
}