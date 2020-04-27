using UniRx;
using UniRx.Triggers;
using UnityEngine;
using TMPro;
using PlayFab.ClientModels;
using PlayFab;

public class CurrencyController : MonoBehaviour {

    public TextMeshProUGUI debugReporter;
    public TextMeshProUGUI diamonCurrencyValueText;
    public static ReactiveProperty<int> diamondCurrencyValue = new ReactiveProperty<int>();
    public GameObject loadingAnimation;

    // Use this for initialization
    void Start ()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsStrings.firstGameLaunch))
        {      
            PlayerPrefs.SetInt(PlayerPrefsStrings.firstGameLaunch, 1);
            PlayerPrefs.SetInt(PlayerPrefsStrings.diamondCurrencyValue, 0);
            diamondCurrencyValue.Value = 0;
        }       

        //This will never let diamond currency value to go < 0
        this.UpdateAsObservable()
            .Where(_ => diamondCurrencyValue.Value < 0)
            .Subscribe(_ => { diamondCurrencyValue.Value = 0; })
            .AddTo(this);

        //diamondCurrencyValue
        //    .Where(_ => GameOverScreenController.gameOver.Value == false)
        //    .Subscribe(_ =>
        //    {
        //        PlayerPrefs.SetInt(PlayerPrefsStrings.diamondCurrencyValue, diamondCurrencyValue.Value);
        //    })
        //    .AddTo(this);
    }       
  
    /// <summary>
    /// Remove diamond currency with specified amount
    /// </summary>
    /// <param name="diamondsAmmount">Currency to be removed</param>
    public void WithdrawDiamondCurrency(int diamondsAmmount)
    {
        debugReporter.text = debugReporter.text + "\n" + "WithdrawDiamondCurrency() called with available currency: " + diamondCurrencyValue.Value.ToString();
        diamondCurrencyValue.Value = diamondCurrencyValue.Value - diamondsAmmount;
        PlayerPrefs.SetInt(PlayerPrefsStrings.diamondCurrencyValue, diamondCurrencyValue.Value);
        PlayerPrefs.SetInt(PlayerPrefsStrings.currencyValueToSubstractForSync, PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync) + diamondsAmmount);
        PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);
        diamonCurrencyValueText.text = diamondCurrencyValue.ToString();
        debugReporter.text = debugReporter.text + "\n" + "WithdrawDiamondCurrency() after call: " + diamondCurrencyValue.Value.ToString();
    }

    /// <summary>
    /// Adds diamond currency with specified amount
    /// </summary>
    /// <param name="diamondsAmmount">Currency to be added</param>
    public void AddDiamondCurrency(int diamondsAmmount)
    {
        debugReporter.text = debugReporter.text + "\n" + "AddDiamondCurrency() called with available currency: " + diamondCurrencyValue.Value.ToString();
        diamondCurrencyValue.Value = diamondCurrencyValue.Value + diamondsAmmount;
        PlayerPrefs.SetInt(PlayerPrefsStrings.diamondCurrencyValue, diamondCurrencyValue.Value);
        PlayerPrefs.SetInt(PlayerPrefsStrings.currencyValueToAddForSync, PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync) + diamondsAmmount);
        PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);
        diamonCurrencyValueText.text = diamondCurrencyValue.ToString();
        debugReporter.text = debugReporter.text + "\n" + "AddDiamondCurrency() after call: " + diamondCurrencyValue.Value.ToString();
    }

    /// <summary>
    /// Gets the currency value for the current user or sets it from cache
    /// </summary>
    public void GetCurrencyValue(bool showLoadScreen)
    {
        if (showLoadScreen)
        {
            loadingAnimation.SetActive(true);

            if (Application.internetReachability != NetworkReachability.NotReachable && PlayerPrefs.GetInt(PlayerPrefsStrings.currencyNeedsSync) == 1)
            {
                debugReporter.text = debugReporter.text + "\n" + "SyncCurrencyWhenOnline() called and needs to add: " + PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync).ToString();
                debugReporter.text = debugReporter.text + "\n" + "SyncCurrencyWhenOnline() called and needs to substract: " + PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync).ToString();
                SyncCurrencyWhenOnline();
            }
        }

        debugReporter.text = debugReporter.text + "\n" + "GetCurrencyValue() called";
        
        GetUserInventoryRequest requestInventory = new GetUserInventoryRequest();     
        int gold = 0;

        PlayFabClientAPI.GetUserInventory(requestInventory,
           result =>
           {
               loadingAnimation.SetActive(false);
               result.VirtualCurrency.TryGetValue("DM", out gold);
               diamondCurrencyValue.Value = gold;
               diamonCurrencyValueText.text = gold.ToString();
               PlayerPrefs.SetInt(PlayerPrefsStrings.diamondCurrencyValue, diamondCurrencyValue.Value);
               debugReporter.text = debugReporter.text + "\n" + "GetCurrencyValue() : refreshed diamond currency";
           },
           error =>
           {
               if (Application.internetReachability != NetworkReachability.NotReachable)
               {
                   loadingAnimation.SetActive(false);
                   diamondCurrencyValue.Value = PlayerPrefs.GetInt(PlayerPrefsStrings.diamondCurrencyValue);
                   diamonCurrencyValueText.text = diamondCurrencyValue.Value.ToString();
                   debugReporter.text = debugReporter.text + "\n" + "GetCurrencyValue() : error getting currency value" + error.GenerateErrorReport();
                   FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Retrieving the currency value failed: " + error.GenerateErrorReport());
               }
               else
               {
                   //Sets the currency value from player prefs if no internet connection is available
                   diamondCurrencyValue.Value = PlayerPrefs.GetInt(PlayerPrefsStrings.diamondCurrencyValue);
                   diamonCurrencyValueText.text = diamondCurrencyValue.Value.ToString();
               }

               
           });       
    }

    /// <summary>
    /// Syncing player prefs data for highscore if playing offline. 
    /// Syncing player prefs data for currency value if purchase with diamonds was made offline.
    /// </summary>
    public static void SyncCurrencyWhenOnline()
    {
        Debug.Log(PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync));
        Debug.Log(PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync));
        AddUserVirtualCurrencyRequest addCurrencyRequest = new AddUserVirtualCurrencyRequest()
        {
            Amount = PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync),
            VirtualCurrency = "DM"
        };

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync) != 0)
        {
            PlayFabClientAPI.AddUserVirtualCurrency(addCurrencyRequest,
            result =>
            {
                PlayerPrefs.SetInt(PlayerPrefsStrings.currencyValueToAddForSync, 0);
                PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 0);
            },
            error =>
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Syncing offline data failed: " + error.GenerateErrorReport());
                PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);
            });
        }
        else
        {
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync) == 0)
            {
                PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 0);
            }
        }
       

        //TBD: This was made because when clicking an upgrade, substract currency is updated as playerpref, if game is closed without pressing back, when coming online again currency could go to minus
        //With this check players can exploit the upgrades without paying for currency
        //You could create a new playerpref holding the values before starting the upgrades as (old currency value) and one for upgrades process started = 1
        //When pressing back the upgrade process started playerpref should be 0 and the old values should be updated with the new upgraded ones
        //If upgrade process started playerpref = 1 don't do the currency substraction // eventuall make a difference between the currency substracted from upgrades and other currencies
        if (PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync) != 0)
        {
            SubtractUserVirtualCurrencyRequest substractCurrencyRequest = new SubtractUserVirtualCurrencyRequest()
            {
                Amount = PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync),
                VirtualCurrency = "DM"
            };

            if (PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync) != 0)
            {
                PlayFabClientAPI.SubtractUserVirtualCurrency(substractCurrencyRequest,
                result =>
                {
                    PlayerPrefs.SetInt(PlayerPrefsStrings.currencyValueToSubstractForSync, 0);
                    PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 0);
                },
                error =>
                {
                    if (error.GenerateErrorReport().Contains("destination host"))
                    {
                        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("NETWORK CONNECTION FAILED! PLEASE MAKE SURE YOU HAVE AN ACTIVE INTERNET CONNECTION!");
                    }
                    else
                    {
                        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("SyncCurrencyWhenOnline: " + error.GenerateErrorReport());
                    }
                    
                    PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);
                });
            }
            else
            {
                if (PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync) == 0)
                {
                    PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 0);
                }
            }
        }
        else
        {
            PlayerPrefs.SetInt(PlayerPrefsStrings.currencyValueToSubstractForSync, 0);
            PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 0);
        }
    }
}
