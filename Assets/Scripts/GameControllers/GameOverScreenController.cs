using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using PlayFab.ClientModels;
using PlayFab;
using System;

public class GameOverScreenController : MonoBehaviour
{
    public AudioManager audioManager;

    public GameObject loadingAnimation;
    public GameObject gameOverScreen;
    public TextMeshProUGUI debugReporter;

    public GameObject wellDoneAnimation;
    public GameObject atLeastYouCollectedDiamonds;

    public GameObject currentScoreDisplay;
    public GameObject bestScoreDisplay; 

    public GameObject reviveWithDiamonds;
    public GameObject reviveWithDiamondsNotAvailable;
    public GameObject reviveWithAds;
    public GameObject reviveWithAdsNotAvailable;

    public GameObject revivePanel;
    public GameObject reviveAlreadyUsedPanel;
    
    public TextMeshProUGUI diamondsNeededToRevive;
    public TextMeshProUGUI currentScoreValue;
    public TextMeshProUGUI oldBestScoreValue;

    public int timerForRevive;
    public int baseCostOfRevive;

    private static int currentDeviceHighScore;
    private static int availableCurrency;
    private static int diamondsNeededToReviveValue;
    private static ReactiveProperty<int> secondsPassedSinceGameOver = new ReactiveProperty<int>(0);
    private float timeOfGameOver;
    private float currentTime;    
   
    private ReactiveProperty<bool> isWin = new ReactiveProperty<bool>(false);
    public static ReactiveProperty<bool> revive = new ReactiveProperty<bool>(false);
    public static ReactiveProperty<bool> gameOver = new ReactiveProperty<bool>(false);

    public static bool alreadyRevived = false;

    private void Awake()
    {
        gameOver.Value = false;
        alreadyRevived = false;
    }

    private void Start()
    {
        gameOver
           .Where(gameOver => gameOver == true)
           .Subscribe(_ =>
           {
               debugReporter.text = debugReporter.text + "\n" + "gameOver set to: " + gameOver;
               MavenMovementControl.movementAfterRespawn.Value = false;
               timeOfGameOver = Time.time;
               Time.timeScale = 0.2f;
               FindObjectOfType<InterfaceHandler>().duringGameUI.SetActive(false);
               SyncHighscoreWhenOnline();
               secondsPassedSinceGameOver.Value = 0;
               GetCurrentDeviceHighScore();
               isWin.Value = IsWinState();
               gameOverScreen.SetActive(true);
               audioManager.StopAllSoundFx();
               UpdateDiamondsNeededForRevive();
               UpdateScoreValuesToText();
               MakeReviveOptionsAvailable();
           })
           .AddTo(this);

        isWin
            .Where(isWin => isWin == true)
            .Subscribe(_ =>
            {
                debugReporter.text = debugReporter.text + "\n" + "isWin state: " + isWin;
                debugReporter.text = debugReporter.text + "\n" + "Setting UI elements for win state";
                Debug.Log("is win state");
                SetUIWinStateDependentElements();
                SetNewDeviceHighScore();
            })
            .AddTo(this);

        isWin
            .Where(isWin => isWin == false)
            .Where(_ => gameOver.Value == true)
            .Subscribe(_ =>
            {
                debugReporter.text = debugReporter.text + "\n" + "isWin state: " + isWin;
                debugReporter.text = debugReporter.text + "\n" + "Setting UI elements for lost state";
                SetUIWinStateDependentElements();
            })
            .AddTo(this);     
    }

    /// <summary>
    /// Gets values to be operated in the current gameover screen
    /// </summary>
    private void GetCurrentDeviceHighScore()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsStrings.playerAccountHighscore))
        {
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.playerAccountHighscore) > PlayerPrefs.GetInt(PlayerPrefsStrings.highScoreOfflineForSync))
            {
                currentDeviceHighScore = PlayerPrefs.GetInt(PlayerPrefsStrings.playerAccountHighscore);
            }
            else
            {
                currentDeviceHighScore = PlayerPrefs.GetInt(PlayerPrefsStrings.highScoreOfflineForSync);
                PlayerPrefs.SetInt(PlayerPrefsStrings.scoreNeedsSync, 1);
            }
        }    
        else
        {
            currentDeviceHighScore = 0;
        }
    }

    /// <summary>
    /// Updates the value for diamonds needed to revive
    /// </summary>
    private void UpdateDiamondsNeededForRevive()
    {
        diamondsNeededToReviveValue = (int)(baseCostOfRevive * (ProceduralGenerator.jumpsTaken.Value + 1));
        //diamondsNeededToRevive.text = diamondsNeededToReviveValue.ToString();
        diamondsNeededToRevive.text = "250";
    }

    /// <summary>
    /// Update the text elements for score
    /// </summary>
    private void UpdateScoreValuesToText()
    {
        currentScoreValue.text = GameManager.collectedDiamonds.Value.ToString();

        //if a new highscore has not been set just set the highscore as old best and the specific playerpref
        if (isWin.Value == false)
        {
            oldBestScoreValue.text = currentDeviceHighScore.ToString();
            PlayerPrefs.SetInt(PlayerPrefsStrings.previousBestScore, currentDeviceHighScore);
        }
        else
        {
            if (PlayerPrefs.HasKey(PlayerPrefsStrings.previousBestScore))
            {
                oldBestScoreValue.text = PlayerPrefs.GetInt(PlayerPrefsStrings.previousBestScore).ToString();
            }
            else
            {
                oldBestScoreValue.text = "0";
                PlayerPrefs.SetInt(PlayerPrefsStrings.previousBestScore, currentDeviceHighScore);
            }
            
        }

        PlayerPrefs.Save();
    }
        
       

    /// <summary>
    /// Compares if the game state is loss or win to set the reactive bool which controls the elements on gameover
    /// </summary>
    private bool IsWinState()
    {
        if (currentDeviceHighScore != 0)
        {          
            if (GameManager.collectedDiamonds.Value > currentDeviceHighScore)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }


    /// <summary>
    /// Sets the new best score as highscore in playerprefs
    /// </summary>
    private void SetNewDeviceHighScore()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsStrings.playerAccountHighscore))
        {
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.playerAccountHighscore) > PlayerPrefs.GetInt(PlayerPrefsStrings.highScoreOfflineForSync))
            {
                currentDeviceHighScore = PlayerPrefs.GetInt(PlayerPrefsStrings.playerAccountHighscore);

                if (GameManager.collectedDiamonds.Value > currentDeviceHighScore)
                {
                    PlayerPrefs.SetInt(PlayerPrefsStrings.playerAccountHighscore, GameManager.collectedDiamonds.Value);
                }
            }
        }
        else
        {
            PlayerPrefs.SetInt(PlayerPrefsStrings.playerAccountHighscore, GameManager.collectedDiamonds.Value);
            currentDeviceHighScore = 0;
        }

        //If not connected to the internet store a new device highscore but one ready for sync            
        currentDeviceHighScore = PlayerPrefs.GetInt(PlayerPrefsStrings.playerAccountHighscore);

        if (GameManager.collectedDiamonds.Value > currentDeviceHighScore)
        {
            PlayerPrefs.SetInt(PlayerPrefsStrings.highScoreOfflineForSync, GameManager.collectedDiamonds.Value);
            PlayerPrefs.SetInt(PlayerPrefsStrings.playerAccountHighscore, GameManager.collectedDiamonds.Value);
            PlayerPrefs.SetInt(PlayerPrefsStrings.scoreNeedsSync, 1);
        }
    }

    /// <summary>
    /// Adds current collected diamonds to playerPrefs, it will be later added to Playfab via sync when returning to main menu
    /// </summary>
    private void AddDiamondsCollectedToCurrency()
    {
        FindObjectOfType<CurrencyController>().AddDiamondCurrency(GameManager.collectedDiamonds.Value);
        PlayerPrefs.SetInt(PlayerPrefsStrings.currencyValueToAddForSync, PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync) + GameManager.collectedDiamonds.Value);
        PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);        
    }

    /// <summary>
    /// Syncing highscore when connected to the internet
    /// </summary>
    public void SyncHighscoreWhenOnline()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsStrings.scoreNeedsSync) == 1)
        {
            FindObjectOfType<LeaderboardController>().SetStats(PlayerPrefs.GetInt(PlayerPrefsStrings.highScoreOfflineForSync), false);
        }
    }
        
    /// <summary>
    /// Saves player prefs and adds collected diamonds to currency
    /// </summary>
    public void SaveAndEnd()
    {
        debugReporter.text = debugReporter.text + "\n" + "SaveAndEnd() called";
        alreadyRevived = false;
        PlayerPrefs.Save();   
        AddDiamondsCollectedToCurrency();     
        SetUIWinStateDependentElements();

        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            PlayerPrefs.SetInt(PlayerPrefsStrings.highScoreOfflineForSync, GameManager.collectedDiamonds.Value);
            PlayerPrefs.SetInt(PlayerPrefsStrings.scoreNeedsSync, 1);
        }
        else
        {
            try
            {
                FindObjectOfType<LeaderboardController>().SetStats(GameManager.collectedDiamonds.Value, false);
            }
            catch
            {
                PlayerPrefs.SetInt(PlayerPrefsStrings.highScoreOfflineForSync, GameManager.collectedDiamonds.Value);
                PlayerPrefs.SetInt(PlayerPrefsStrings.scoreNeedsSync, 1);
            }
        }        

        SyncHighscoreWhenOnline();
    }

    /// <summary>
    /// Give one more chance to the player using ads
    /// </summary>   
    public void ReviveWithAd()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            debugReporter.text = debugReporter.text + "\n" + "Watch ad picked";     
            FindObjectOfType<UnityAdsController>().PlayRewardedAd("reviveReward");            
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("YOU NEED AN ACTIVE INTERNET CONNECTION TO WATCH ADS");
        }
    }

    public void ReviveAdWatched()
    {
        gameOverScreen.SetActive(false);
        alreadyRevived = true;
        revive.Value = true;
        Time.timeScale = 1;
        FindObjectOfType<InterfaceHandler>().duringGameUI.SetActive(true);
    }

    public void ReviveAdFailed()
    {
         FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("OOPS! AD FAILED TO DISPLAY. TRY USING DIAMONDS INSTEAD!");
    }

    /// <summary>
    /// Give one more chance to the player using diamonds
    /// </summary>
    public void ReviveWithDiamonds()
    {
        loadingAnimation.SetActive(true);
        Time.timeScale = 1;
        
        if (!(CurrencyController.diamondCurrencyValue.Value < 250))
        {
            try
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest()
                    {
                        ItemId = "revive",
                        Price = 250,
                        VirtualCurrency = "DM",
                        CatalogVersion = "Utility"
                    },
                    result =>
                    {
                        alreadyRevived = true;
                        debugReporter.text = debugReporter.text + "\n" + "ReviveWithDiamonds() success";
                        revive.Value = true;
                        gameOverScreen.SetActive(false);
                        loadingAnimation.SetActive(false);
                        FindObjectOfType<InterfaceHandler>().duringGameUI.SetActive(true);

                    },
                    error =>
                    {
                        if (error.GenerateErrorReport().Contains("Insufficient funds"))
                        {
                            debugReporter.text = debugReporter.text + "\n" + "You don't have enough diamonds window display";
                            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("YOU DON'T HAVE ENOUGH DIAMONDS");
                        }
                        else
                        {
                            debugReporter.text = debugReporter.text + "\n" + "ReviveWithDiamonds() failed with error: " + error.GenerateErrorReport();
                        }

                        loadingAnimation.SetActive(false);
                        return;
                    });
                }
                else
                {
                    FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(250);
                    PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);
                    revive.Value = true;
                    gameOverScreen.SetActive(false);
                    loadingAnimation.SetActive(false);
                }
            }
            catch (Exception)
            {
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(250);
                PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);
                revive.Value = true;
                gameOverScreen.SetActive(false);
                loadingAnimation.SetActive(false);
            }
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("YOU DON'T HAVE ENOUGH DIAMONDS");
        }
    }

    /// <summary>
    /// Displays current score or new best frames based on win state
    /// </summary>
    private void SetUIWinStateDependentElements()
    {
        if (isWin.Value == true)
        {
            bestScoreDisplay.SetActive(true);
            currentScoreDisplay.SetActive(false);
            atLeastYouCollectedDiamonds.SetActive(false);
            wellDoneAnimation.SetActive(true);
        }
        else
        {
            bestScoreDisplay.SetActive(false);
            currentScoreDisplay.SetActive(true);
            atLeastYouCollectedDiamonds.SetActive(true);
            wellDoneAnimation.SetActive(false);
        }
    }

    /// <summary>
    /// Makes options active on gameover
    /// </summary>
    private void MakeReviveOptionsAvailable()
    {
        if (!alreadyRevived)
        {
            revivePanel.SetActive(true);
            reviveAlreadyUsedPanel.SetActive(false);           
        }
        else
        {
            revivePanel.SetActive(false);
            reviveAlreadyUsedPanel.SetActive(true);
        }
    }    
}
