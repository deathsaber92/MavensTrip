using UnityEngine;

public class PlayerPrefsStrings : MonoBehaviour
{
    #region Logical&Events
    public static readonly string firstGameLaunch = "le0";
    public static readonly string firstPurchase = "le1";
    public static readonly string firstTimeInSettings = "le2";
    public static readonly string setStartSkin = "le3";
    public static readonly string lowOn = "le4";
    public static readonly string medOn = "le5";
    public static readonly string highOn = "le6";
    public static readonly string dontDisplayAgainTutorial = "le7";
    public static readonly string lastChestOpenTime = "le8";
    public static readonly string newsMessageDisplayed = "le9";
    #endregion

    #region Score&Currency    
    public static readonly string diamondCurrencyValue = "sc1";
    public static readonly string previousBestScore = "sc2";
    public static readonly string updatedScore = "sc3";
    #endregion

    #region Sync Offline When Reconnected
    public static readonly string highScoreOfflineForSync = "sowr0";
    public static readonly string currencyValueToSubstractForSync = "sowr1";
    public static readonly string currencyValueToAddForSync = "sowr2";
    #endregion

    #region Skins
    public static readonly string skinDefaultLock = "sk0";    
    public static readonly string skinFoundersLock = "sk5";
    public static readonly string skinExplorerLock = "sk6";
    public static readonly string skinRhinoLock = "sk7";
    public static readonly string skinPharaonLock = "sk8";
    #endregion

    #region Gameplay&Movement
    //The following should hold the upgrade level index of each upgrade
    public static readonly string mavenSpeed = "gm0";
    public static readonly string mavenJumpPower = "gm1";
    public static readonly string mavenFuel = "gm2";
    public static readonly string mavenPowerUpDuration = "gm3";
    public static readonly string mavenPowerUpCharges = "gm4";
    public static readonly string mavenMagnetRange = "gm5";
    public static readonly string mavenMagnetSpeed = "gm6";
    #endregion

    #region SyncData
    public static readonly string scoreNeedsSync = "sd0";
    public static readonly string currencyNeedsSync = "sd1";
    public static readonly string playerDataNeedsSync = "sd2";
    public static readonly string playerAccountHighscore = "sd3";
    #endregion

    private void Start()
    {
        //Setting the sync parameters to 0 to be ready to use for the first time
        if (!PlayerPrefs.HasKey(highScoreOfflineForSync))
        {
            SetSyncPlayerPrefsToZero();
        }

        if (!PlayerPrefs.HasKey(scoreNeedsSync))
        {
            PlayerPrefs.SetInt(scoreNeedsSync, 0);
        }

        if (!PlayerPrefs.HasKey(currencyNeedsSync))
        {
            PlayerPrefs.SetInt(currencyNeedsSync, 0);
        }

        if (!PlayerPrefs.HasKey(playerDataNeedsSync))
        {
            PlayerPrefs.SetInt(playerDataNeedsSync, 0);
        }
    }

    /// <summary>
    /// Sets the sync variables back to zero after sync occurs
    /// </summary>
    public void SetSyncPlayerPrefsToZero()
    {
        PlayerPrefs.SetInt(highScoreOfflineForSync, 0);
        PlayerPrefs.SetInt(currencyValueToSubstractForSync, 0);
        PlayerPrefs.SetInt(currencyValueToAddForSync, 0);
    }
}
