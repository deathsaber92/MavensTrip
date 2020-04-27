using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChestObject : MonoBehaviour
{
    public GameObject emptyChesti;
    public GameObject filledChest;
    public GameObject animationBackground;
    public TextMeshProUGUI timerDisplay;
    public GameObject collectDisplay;
    private static Button chestButton;

    protected ulong lastChestOpenTimeStamp;
    protected ulong currentSystemTicks;

    private int timeForChestOpen;
    private int rewardedDiamonds;
    protected static int? secondsLeftToOpenChest = 0;
    protected static int? secondsLeftToOpen;
    protected static ulong? playfabServerTime;
    protected static ulong? checkedPlayfabTime;
    protected static int realtimeMinutesLeftToOpen = 0;
    protected static int realtimeSecondsLeftToOpen = 0;
    protected static ReactiveProperty<int> generalTimer = new ReactiveProperty<int>(0);

    private bool isChestAvailable;
    private static bool serverTimeReceived = false;

    private void Awake()
    {
        chestButton = GetComponent<Button>();
        isChestAvailable = false;
        SetButtonUnavailableState();
    }

    private void Start()
    {
        chestButton = GetComponent<Button>();
        isChestAvailable = false;
        timeForChestOpen = Auth.chestOpenTimeInSeconds;
        rewardedDiamonds = Auth.chestOpenRewardInDiamonds;

        CheckChestTimeOnPlayFab(); 
        CheckPlayFabServerTime();        

        generalTimer.Subscribe
        (_ =>
            {
                if (serverTimeReceived)
                {
                    SyncedCountDown();
                }                
            }
        ).AddTo(this);
    }

    private void Update()
    {
        if (isChestAvailable)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                SetButtonAvailableState();
                CountDown();
            }           
        }
        else
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                UpdateTimerOnUI();
                SetButtonUnavailableState();
                CountDown();
            }
        }
    }

    /// <summary>
    /// Getting the last chest open time from playfab and then do the local logic if anything fails
    /// </summary>
    private void CheckChestTimeOnPlayFab()
    {
        GetLastChestOpenTime();

        if (PlayerPrefs.HasKey(PlayerPrefsStrings.lastChestOpenTime))
        {
            //Debug.Log("CheckChestTimeOnPlayFab() : haskey " );

            if (PlayerPrefs.GetString(PlayerPrefsStrings.lastChestOpenTime).Contains("noTimeRecorded") == false)
            {
                lastChestOpenTimeStamp = ulong.Parse(PlayerPrefs.GetString(PlayerPrefsStrings.lastChestOpenTime));
                Debug.Log("noTimeRecorded " + lastChestOpenTimeStamp);
            }
            else
            {
                lastChestOpenTimeStamp = 0;
                //Debug.Log("CheckChestTimeOnPlayFab() : noTimeRecorded key ");
            }

            //Debug.Log("CheckChestTimeOnPlayFab() : lastChestOpenTimeStamp: " + lastChestOpenTimeStamp);
        }
        else
        {
            PlayerPrefs.SetString(PlayerPrefsStrings.lastChestOpenTime, "noTimeRecorded"); //if this is set to 000000 chest will be available
            lastChestOpenTimeStamp = 0;

            Debug.Log("CheckChestTimeOnPlayFab() : lastChestOpenTimeStamp: " + lastChestOpenTimeStamp);
        }
    }

    private void SetButtonUnavailableState()
    {
        chestButton.interactable = false;
        //Debug.Log("SetButtonUnavailableState()");
        filledChest.SetActive(false);
        animationBackground.SetActive(false);
        timerDisplay.gameObject.SetActive(true);
        collectDisplay.SetActive(false);        
    }

    private void SetButtonAvailableState()
    {
        //Debug.Log("SetButtonAvailableState()");
        chestButton.interactable = true;
        filledChest.SetActive(true);
        animationBackground.SetActive(true);
        timerDisplay.gameObject.SetActive(false);
        collectDisplay.SetActive(true);
    }

    /// <summary>
    /// updates the playerpref from cloud on login so that the user cannot uninstall and intall the game to get more chests
    /// </summary>
    public static void GetLastChestOpenTime()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = Auth.playFabId,
            Keys = null
        },
        result =>
        {
            if (result.Data["lastChestOpenTime"].Value != null)
            {
                PlayerPrefs.SetString(PlayerPrefsStrings.lastChestOpenTime, result.Data["lastChestOpenTime"].Value);
                Debug.Log("player data got the time: " + result.Data["lastChestOpenTime"]);
            }
            Debug.Log("player data should have gotten the time: " + result.Data["lastChestOpenTime"]);
        },
        error =>
        {
            Debug.Log("error getting data: " + error.ErrorMessage);
        });
    }

    /// <summary>
    /// This is called only when opening timed chest
    /// </summary>
    private void SetChestOpenTime()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "currTimeSeconds"
        }, result =>
        {
            //Debug.Log(result.FunctionResult.ToString().Substring(0, result.FunctionResult.ToString().IndexOf(".")));
            PlayerPrefs.SetString(PlayerPrefsStrings.lastChestOpenTime, result.FunctionResult.ToString().Substring(0, result.FunctionResult.ToString().IndexOf(".")));
            lastChestOpenTimeStamp = ulong.Parse(result.FunctionResult.ToString().Substring(0, result.FunctionResult.ToString().IndexOf(".")));

            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string>()
            {
                {"lastChestOpenTime", PlayerPrefs.GetString(PlayerPrefsStrings.lastChestOpenTime)}
            }
            },
            resultInner =>
            {
                Debug.Log("successfully updated chest time");
            },
            errorInner =>
            {
                Debug.Log("chest open time failed to update: " + errorInner.ErrorMessage);
            });

        }, error =>
        {
            Debug.Log("error getting time");
        });
    }

    /// <summary>
    /// This is checking the playfab server time. It can return ulong or null so be sure to catch it in a nullable ulong.
    /// </summary>
    /// <returns></returns>
    public void CheckPlayFabServerTime()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "currTimeSeconds"
        }, result =>
        {
            playfabServerTime = ulong.Parse(result.FunctionResult.ToString().Substring(0, result.FunctionResult.ToString().IndexOf(".")));
            //Debug.Log("playfabServerTime: " + playfabServerTime.ToString());     
            StartCounterFromTimeLeft(ReturnSecondsLeftToOpen());
            serverTimeReceived = true;
        }, error =>
        {
            Debug.Log("error checking time");
            playfabServerTime = null;
            serverTimeReceived = false;
        });
    }

    /// <summary>
    /// This is always setting the timer to start from what is left
    /// </summary>
    /// <param name="secondsLeftToOpen">How many seconds are left until chest open</param>
    private void StartCounterFromTimeLeft(int? secondsLeftToOpen)
    {
        secondsLeftToOpenChest = secondsLeftToOpen;
        //Debug.Log("StartCounterFromTimeLeft(), secondsLeftToOpenChest: " + secondsLeftToOpenChest);
    }

    private void CountDown()
    {
        generalTimer.Value = (int)Time.time;        
    }

    /// <summary>
    /// Syncs the countdown to the actual time ticking by seconds with unirx
    /// </summary>
    private void SyncedCountDown()
    {
        if (secondsLeftToOpenChest != null)
        {
            realtimeMinutesLeftToOpen = (int)(secondsLeftToOpenChest / 60);
            realtimeSecondsLeftToOpen = (int)secondsLeftToOpenChest % 60;         


            if (secondsLeftToOpenChest > 0)
            {
                isChestAvailable = false;            
                secondsLeftToOpenChest = (int)secondsLeftToOpenChest - 1;                
            }
            else if (secondsLeftToOpenChest == 0)
            {
                isChestAvailable = true;
            }
            else
            {
                secondsLeftToOpenChest = 0;               
            }

            //Debug.Log(realtimeSecondsLeftToOpen);
        }
        else
        {
            timerDisplay.text = "ERROR!";
        }
    }

    /// <summary>
    /// This updates the timer on the UI
    /// </summary>
    private void UpdateTimerOnUI()
    {
        if (realtimeMinutesLeftToOpen <= 9)
        {
            if (realtimeSecondsLeftToOpen <= 9)
            {
                timerDisplay.text = "0" + realtimeMinutesLeftToOpen + " : 0" + realtimeSecondsLeftToOpen;
            }
            else
            {
                timerDisplay.text = "0" + realtimeMinutesLeftToOpen + " : " + realtimeSecondsLeftToOpen;
            }            
        }
        else
        {
            if (realtimeSecondsLeftToOpen <= 9)
            {
                timerDisplay.text = realtimeMinutesLeftToOpen + " : 0" + realtimeSecondsLeftToOpen;
            }
            else
            {
                timerDisplay.text = realtimeMinutesLeftToOpen + " : " + realtimeSecondsLeftToOpen;
            }
        }
    }

    /// <summary>
    /// This returns the seconds left to open a chest. It can return int or null so be sure to catch it in a nullable int.
    /// </summary>
    /// <returns></returns>
    private int? ReturnSecondsLeftToOpen()
    {
        checkedPlayfabTime = playfabServerTime;

        if (checkedPlayfabTime != null)
        {
            if ((int)(checkedPlayfabTime - lastChestOpenTimeStamp) < 3600)
            {
                secondsLeftToOpen = timeForChestOpen - ((int)(checkedPlayfabTime - lastChestOpenTimeStamp));
                isChestAvailable = false;
            }
            else
            {
                secondsLeftToOpen = 0;            
                isChestAvailable = true;
            }
        }
        else
        {
            secondsLeftToOpen = null;
        }

        //Debug.Log("time left to open: " + secondsLeftToOpen);

        if (lastChestOpenTimeStamp != 0)
        {
            isChestAvailable = false;
            return secondsLeftToOpen;
        }
        else
        {
            isChestAvailable = true;
            return 0;
        }        
    }

    public void PressChest()
    {
        isChestAvailable = true;
        FindObjectOfType<UnityAdsController>().PlayRewardedAd("chestReward");
    }

    internal void ReviveAdFailed()
    {
        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("AD FAILED TO FINISH PLAYING. PLEASE WATCH THE WHOLE AD TO GET REWARDED!");
    }

    internal void ReviveAdWatched()
    {
        isChestAvailable = false;
        FindObjectOfType<ShowErrorMessageController>().SetSuccessMessage($"YOU HAVE BEEN REWARDED WITH {rewardedDiamonds} DIAMONDS");
        FindObjectOfType<CurrencyController>().AddDiamondCurrency(rewardedDiamonds);
        secondsLeftToOpenChest = timeForChestOpen;
        SetChestOpenTime();        
    }

}
