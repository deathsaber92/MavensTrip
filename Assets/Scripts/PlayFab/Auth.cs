using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Globalization;

public class Auth : MonoBehaviour 
{
    public static string playFabId;
    public static string newsMessage;
    public static string versionBlockMessage;
    public static int chestOpenTimeInSeconds;
    public static int chestOpenRewardInDiamonds;
    public static bool isAuthenticated = false;

    public TextMeshProUGUI debugReporter;
    public TMP_InputField displayNameInputField;

    public GameObject mainMenuScreen;
    public GameObject welcomeScreen;   
    public GameObject pickDisplayName;
    public GameObject connectingAnimation;

    private string authCode;

#if UNITY_ANDROID

    #region GoogleSignIn
    /// <summary>
    /// Sign in to google
    /// </summary>
    /// <param name="linkRequest">This is used to control when the function is called for linking or not: if it is called for linking then SignInWithGoogle is no longer called, linkGoogleAccount is called instead</param>
    public void GoogleSignIn()
    {
        debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() : creating config for play games";
        connectingAnimation.SetActive(true);

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .RequestEmail()
        .RequestServerAuthCode(false)
        .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        try
        {
            debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() : trying authentication...";           
            Social.localUser.Authenticate((bool success) =>
            {
                debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() : bool success is: " + success;
                if (success)
                {
                    debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() : Authentication was succesful";
                    authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                    debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() : auth code is: " + authCode;
                    SignInWithGoogle();
                }
                else
                {
                    debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() : Failed to connect";

                    if (Application.internetReachability != NetworkReachability.NotReachable)
                    {                        
                        debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() :  internet connection is available at time: " + Time.time.ToString();
                        //show failed to connect window

                        connectingAnimation.SetActive(false);
                        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Failed to connect to Google Play Games");
                    }
                    else
                    {
                        debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() :  internet connection is not available ";
                        connectingAnimation.SetActive(false);                       
                        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Please connect to the internet first!");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            debugReporter.text = debugReporter.text + "\n" + "GoogleSignIn() :  Failed to connect to Google Play Games: " + ex.Message;
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Failed to connect to Google Play Games: " + ex.Message);
        }
    }

    /// <summary>
    /// Sign in with google 
    /// </summary>
    private void SignInWithGoogle()
    {
        debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() called";

        try
        {
            connectingAnimation.SetActive(true);
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : Success auth... ";
                    try
                    {
                        debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : createAccount true scenario stepped in";                       

                        debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : checking if google account exists with auth code: " + authCode;

                        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                        {
                            TitleId = PlayFabSettings.TitleId,
                            ServerAuthCode = authCode,
                            CreateAccount = true,
                        }, (result) =>
                        {
                            playFabId = result.PlayFabId;
                            debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : Account created with id: " + result.PlayFabId;
                            debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : Preparing to call LinkDeviceIdWithAccount();";                            
                            SetDisplayName();
                            CheckVersionOnLogin();
                            ChestObject.GetLastChestOpenTime();
                        }, OnGoogleAccountLoginResultFailed);
                    }
                    catch (Exception exception)
                    {
                        debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : Authenticate to playfab or google auth code refresh failed with error: " + exception.Message;
                        connectingAnimation.SetActive(false);

                        if (Application.internetReachability == NetworkReachability.NotReachable)
                        {
                            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("PLEASE CONNECT TO THE INTERNET TO LOGIN");
                        }
                        else
                        {
                            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage(exception.Message);
                        }
                    }
                }
                else
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("PLEASE CONNECT TO THE INTERNET FIRST"); //legit 
                        debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : Please connect to the internet first!";
                    }

                    debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : Authentication to google failed";
                    connectingAnimation.SetActive(false);
                }
            });
        }
        catch (Exception exception)
        {
            debugReporter.text = debugReporter.text + "\n" + "SignInWithGoogle() : Authenticate process failed with error: " + exception.Message;
            connectingAnimation.SetActive(false);
        }
    }

    /// <summary>
    /// Happens when the login fails
    /// </summary>
    /// <param name="obj"></param>
    private void OnGoogleAccountLoginResultFailed(PlayFabError obj)
    {
        debugReporter.text = debugReporter.text + "\n" + "OnGoogleAccountLoginResultFailed() : Failed to create account with GPG: " + obj.ErrorMessage;   
        connectingAnimation.SetActive(false);

        PlayGamesPlatform.Instance.GetAnotherServerAuthCode(true, code =>
        {
            authCode = code;
            debugReporter.text = debugReporter.text + "\n" + "LinkGoogleAccount() : Refreshed auth code";
        });

        if (obj.GenerateErrorReport().Contains("destination host"))
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("NETWORK CONNECTION FAILED! PLEASE MAKE SURE YOU HAVE AN ACTIVE INTERNET CONNECTION!");
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("SyncCurrencyWhenOnline: " + obj.GenerateErrorReport());
        }
    }

    #endregion

#endif

    #region DeviceIDSignIn

    /// <summary>
    /// Sign in with device
    /// </summary>
    private void SignInWithDeviceId()
    {
        debugReporter.text = debugReporter.text + "\n" + "SignInWithDeviceId(): Trying to sign in with device id...";

        PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            AndroidDevice = SystemInfo.deviceModel,
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            OS = SystemInfo.operatingSystem,
            CreateAccount = true            
        }, (result) =>
        {
            playFabId = result.PlayFabId;
            isAuthenticated = true;       
            debugReporter.text = debugReporter.text + "\n" + "SignInWithDeviceId(): succesful with playFabId: " + result.PlayFabId.ToString();      
            CheckVersionOnLogin();
            ChestObject.GetLastChestOpenTime();
        }, OnPlayFabError);
    }
    #endregion

    #region DisplayName

    /// <summary>
    /// Will set the display name for the first time
    /// </summary>
    public void SetDisplayName()
    {
        debugReporter.text = debugReporter.text + "\n" + "SetDisplayName(): Trying to change display name to : " + Social.localUser.userName;
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = Social.localUser.userName
        }, result =>
        {
            debugReporter.text = debugReporter.text + "\n" + "SetDisplayName(): The player's display name is now: " + result.DisplayName;
        }, error =>
        {
            debugReporter.text = debugReporter.text + "\n" + "SetDisplayName() : The player's display name was not changed with error: " + error.ErrorMessage;
        });
        
    }
    #endregion

    /// <summary>
    /// The anonymous login button that will pop a warning screen
    /// </summary>
    public void AnonymousLogin_Button()
    {
        FindObjectOfType<AudioManager>().PlaySound("buttonSound");
        connectingAnimation.SetActive(true);
        SignInWithDeviceId();
    }
    
    /// <summary>
    /// Logs the error when authentication fails and sets the isAuthenticated to false
    /// </summary>
    /// <param name="obj"></param>
    private void OnPlayFabError(PlayFabError obj)
    {
        debugReporter.text = debugReporter.text + "\n" + "Failed to authorize login: " + obj.ErrorMessage;
        isAuthenticated = false;
    }

    /// <summary>
    /// Not having the latest version will not allow the player to login
    /// To be implemented in future: a parameter which will tell if the 
    /// </summary>
    private void CheckVersionOnLogin()
    {
        string clientVersion = Application.version;
        //Debug.Log("Application Version : " + clientVersion);

        float trimmedClientVersion = float.Parse(clientVersion, CultureInfo.InvariantCulture.NumberFormat);
        //Debug.Log("Trimmed application Version : " + trimmedClientVersion);

        int numericalClientVersion = Int32.Parse((trimmedClientVersion * 1000).ToString());
        //Debug.Log("Numerical application Version : " + numericalClientVersion);

        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result =>
            {
                if (result.Data.ContainsKey("announcementMessage"))
                {
                    newsMessage = result.Data["announcementMessage"].ToString();
                }
                else
                {
                    newsMessage = "0";
                }

                if (result.Data.ContainsKey("versionBlockMessage"))
                {
                    versionBlockMessage = result.Data["versionBlockMessage"].ToString();
                }
                else
                {
                    versionBlockMessage = "0";
                }                

                if (result.Data.ContainsKey("chestOpenTime"))
                {
                    chestOpenTimeInSeconds = Int32.Parse(result.Data["chestOpenTime"]);
                }
                else
                {
                    chestOpenTimeInSeconds = 3600;
                }

                if (result.Data.ContainsKey("diamondsRewarded"))
                {
                    chestOpenRewardInDiamonds = Int32.Parse(result.Data["diamondsRewarded"]);
                }
                else
                {
                    chestOpenRewardInDiamonds = 500;
                }

                if (result.Data.ContainsKey("version"))
                {
                    int numericalServerVersion = Int32.Parse(result.Data["version"]);
                    //Debug.Log("Version on server is: " + numericalServerVersion);

                    if (numericalServerVersion > numericalClientVersion)
                    {
                        if (versionBlockMessage.Equals("0"))
                        {
                            FindObjectOfType<ShowErrorMessageController>().SetVersionErrorMessage("YOU ARE NOT USING THE LASTEST VERSION OF THE APP! PLEASE UPDATE BEFORE PLAYING!");
                        }
                        else
                        {
                            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage(versionBlockMessage);
                        }
                    }
                    else
                    {
                        isAuthenticated = true;                       
                        FindObjectOfType<UpgradesAndGameplayController>().GetUpgradePlayerData(true);
                        FindObjectOfType<PlayFabInventoryManager>().GetSkinPrices();
                        FindObjectOfType<SkinLockStatusManager>().UpdateSkinLockStatus();
                        FindObjectOfType<CurrencyController>().GetCurrencyValue(true);
                        FindObjectOfType<MainMenuHandler>().SyncCurrencyOnLogin();
                        FindObjectOfType<LeaderboardController>().GetHighscore();
                        welcomeScreen.SetActive(false);
                        mainMenuScreen.SetActive(true);
                        connectingAnimation.SetActive(false);

                        if (!newsMessage.Equals("0"))
                        {
                            if (!newsMessage.Equals(PlayerPrefs.GetString(PlayerPrefsStrings.newsMessageDisplayed)))
                            {
                                FindObjectOfType<ShowErrorMessageController>().SetNewsMessage(newsMessage);
                            }
                        }
                    }
                }
                else
                {
                    connectingAnimation.SetActive(false);
                    FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Server or client do not contain version information. Please contact us to solve this issue!");
                }
            },
            error =>
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("There was an error trying to check the version of your app: " + error.GenerateErrorReport());
            }
        );
    }
}
