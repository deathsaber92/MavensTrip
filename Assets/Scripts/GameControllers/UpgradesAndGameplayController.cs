using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradesAndGameplayController : MonoBehaviour 
{
    public AudioManager audioManager;
    public GameObject upgradesScreen;
    public GameObject loadingAnimation;
    public GameObject mainMenuItems;
    public TextMeshProUGUI debugReporter;

    public TMP_FontAsset fontGreen;
    public TMP_FontAsset fontRed;

    public TextMeshProUGUI mavenSpeedPrice;
    public TextMeshProUGUI mavenFuelPrice;
    public TextMeshProUGUI mavenJumpPowerPrice;
    public TextMeshProUGUI mavenPowerUpDurationPrice;
    public TextMeshProUGUI mavenPowerUpChargesPrice;
    public TextMeshProUGUI mavenMagnetRangePrice;
    public TextMeshProUGUI mavenMagnetSpeedPrice; 
    public TextMeshProUGUI availableDiamonds;

    public GameObject[] speedFilled;
    public GameObject[] speedEmpty;
    public GameObject[] speedUpgradeable;

    public GameObject[] fuelFilled;
    public GameObject[] fuelEmpty;
    public GameObject[] fuelUpgradeable;

    public GameObject[] jumpPowerFilled;
    public GameObject[] jumpPowerEmpty;
    public GameObject[] jumpPowerUpgradeable;

    public GameObject[] powerUpDurationFilled;
    public GameObject[] powerUpDurationEmpty;
    public GameObject[] powerUpDurationUpgradeable;

    public GameObject[] powerUpChargesFilled;
    public GameObject[] powerUpChargesEmpty;
    public GameObject[] powerUpChargesUpgradeable;

    public GameObject[] magnetRangeFilled;
    public GameObject[] magnetRangeEmpty;
    public GameObject[] magnetRangeUpgradeable;

    public GameObject[] magnetSpeedFilled;
    public GameObject[] magnetSpeedEmpty;
    public GameObject[] magnetSpeedUpgradeable;

    /// <summary>
    /// Those are the statistics names which should be 0 on default
    /// </summary>
    public static int mavenSpeed = 0;
    public static int mavenJumpPower = 0;
    public static int mavenFuel = 0;
    public static int mavenPowerUpDuration = 0;
    public static int mavenPowerUpCharges = 0;
    public static int mavenMagnetRange = 0; 
    public static int mavenMagnetSpeed = 0;

    public int[] pricesForUpgrades;

    public float[] mavenSpeedValues;
    public float[] mavenJumpPowerValues;
    public float[] mavenFuelValues;
    public float[] mavenPowerUpDurationValues;
    public int[] mavenPowerUpChargesValues;
    public float[] mavenMagnetRangeValues;
    public float[] mavenMagnetSpeedValues;

    private GetLeaderboardAroundPlayerRequest requestLeaderboardAroundPlayer;
    private bool firstTimeUpdated = false;
    private static bool madeUpgrades = false;
            
    /// <summary>
    /// Sets the gameplay parameteres based on upgrade levels
    /// </summary>
    public void SetGameplayValuesByUpgradeIndex()
    {
        var indexSpeed = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed);
        var indexJumpPower = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower);
        var indexFuel = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel);
        var indexMagnetSpeed = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed);
        var indexMagnetRange = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange);
        var indexPowerUpDuration = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration);
        var indexPowerUpCharges = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges);

        FindObjectOfType<MavenMovementControl>().SetMavenMovementParameters(mavenSpeedValues[indexSpeed], mavenJumpPowerValues[indexJumpPower], mavenFuelValues[indexFuel]);
        FindObjectOfType<Magnet>().SetMavenMagnetParameters(mavenMagnetSpeedValues[indexMagnetSpeed], mavenMagnetRangeValues[indexMagnetRange]);
        FindObjectOfType<RelicUsageController>().SetMavenPowerUpParameters(mavenPowerUpDurationValues[indexPowerUpDuration], mavenPowerUpChargesValues[indexPowerUpCharges]);        
    }

    /// <summary>
    /// Gets the desired upgrade name by the button name which should be like mavenParameter0
    /// </summary>
    public void OnUpgradeButtonClick()
    {
        debugReporter.text = debugReporter.text + "\n" + "UnlockCurrentSkin() called. Trying to unlock current skin...";
        audioManager.PlaySound("buttonSound");

        string selectedUpgrade = EventSystem.current.currentSelectedGameObject.name.ToString();

        string upgradeIndexString = selectedUpgrade.Substring(selectedUpgrade.Length - 1);
        int upgradeIndex = Int32.Parse(upgradeIndexString);
        int price = pricesForUpgrades[upgradeIndex];

        
        if (selectedUpgrade.Contains("mavenSpeed"))
        {
            MakeUpgrade(selectedUpgrade, price, upgradeIndex, speedEmpty[upgradeIndex], speedFilled[upgradeIndex], speedUpgradeable[upgradeIndex]);
        }  
        else if (selectedUpgrade.Contains("mavenFuel"))
        {
            MakeUpgrade(selectedUpgrade, price, upgradeIndex, fuelEmpty[upgradeIndex], fuelFilled[upgradeIndex], fuelUpgradeable[upgradeIndex]);
        }
        else if (selectedUpgrade.Contains("mavenJumpPower"))
        {
            MakeUpgrade(selectedUpgrade, price, upgradeIndex, jumpPowerEmpty[upgradeIndex], jumpPowerFilled[upgradeIndex], jumpPowerUpgradeable[upgradeIndex]);
        }
        else if (selectedUpgrade.Contains("mavenMagnetRange"))
        {
            MakeUpgrade(selectedUpgrade, price, upgradeIndex, magnetRangeEmpty[upgradeIndex], magnetRangeFilled[upgradeIndex], magnetRangeUpgradeable[upgradeIndex]);
        }
        else if (selectedUpgrade.Contains("mavenMagnetSpeed"))
        {
            MakeUpgrade(selectedUpgrade, price, upgradeIndex, magnetSpeedEmpty[upgradeIndex], magnetSpeedFilled[upgradeIndex], magnetSpeedUpgradeable[upgradeIndex]);
        }
        else if (selectedUpgrade.Contains("mavenPowerUpDuration"))
        {
            MakeUpgrade(selectedUpgrade, price, upgradeIndex, powerUpDurationEmpty[upgradeIndex], powerUpDurationFilled[upgradeIndex], powerUpDurationUpgradeable[upgradeIndex]);
        }
        else if (selectedUpgrade.Contains("mavenPowerUpCharges"))
        {
            MakeUpgrade(selectedUpgrade, price, upgradeIndex, powerUpChargesEmpty[upgradeIndex], powerUpChargesFilled[upgradeIndex], powerUpChargesUpgradeable[upgradeIndex]);
        }
    }


    /// <summary>
    /// Updates the cloud upgrades according to the changes made during the upgrade session
    /// </summary>
    public void UpdateCloudUpgradesPlayerData()
    {
        loadingAnimation.SetActive(true);
        debugReporter.text = debugReporter.text + "\n" + "UpdateCloudUpgradesPlayerData() called;";

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            try
            {
                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
                {
                    Data = new Dictionary<string, string>()
                    {
                        {"mavenSpeed", (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed)).ToString()},
                        {"mavenFuel", (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel)).ToString()},
                        {"mavenJumpPower", (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower)).ToString()},
                        {"mavenMagnetRange", (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange)).ToString()},
                        {"mavenMagnetSpeed", (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed)).ToString()},
                        {"mavenPowerUpDuration", (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration)).ToString()},
                        {"mavenPowerUpCharges", (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges)).ToString()}
                    }
                },
                result =>
                {
                    loadingAnimation.SetActive(false);
                    debugReporter.text = debugReporter.text + "\n" + "UpdateCloudUpgradesPlayerData() success;";
                    CurrencyController.SyncCurrencyWhenOnline();
                },
                error =>
                {
                    loadingAnimation.SetActive(false);

                    if (!error.GenerateErrorReport().Contains("internet"))
                    {
                        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("UPGRADE FAILED: " + error.GenerateErrorReport());
                    }

                    PlayerPrefs.SetInt(PlayerPrefsStrings.playerDataNeedsSync, 1);
                    debugReporter.text = debugReporter.text + "\n" + "UpdateCloudUpgradesPlayerData() failed with reason: " + error.GenerateErrorReport();
                });
            }
            catch (Exception ex)
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("UPGRADE FAILED: " + ex.Message);
                loadingAnimation.SetActive(false);
                debugReporter.text = debugReporter.text + "\n" + "UpdateCloudUpgradesPlayerData() failed with reason: " + ex.Message;
            }
        }
        else
        {
            PlayerPrefs.SetInt(PlayerPrefsStrings.playerDataNeedsSync, 1);
            debugReporter.text = debugReporter.text + "\n" + "Network not reachable";
            loadingAnimation.SetActive(false);
        }
    }

    /// <summary>
    /// Gets the upgrades player data and sets playerprefs accordingly
    /// </summary>
    public void GetUpgradePlayerData(bool isMenuOnlySync)
    {
        loadingAnimation.SetActive(true);

        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = Auth.playFabId,
            Keys = null
        }, 
        result =>
        {            
            Int32.TryParse(result.Data["mavenSpeed"].Value, out mavenSpeed);
            Int32.TryParse(result.Data["mavenFuel"].Value, out mavenFuel);
            Int32.TryParse(result.Data["mavenJumpPower"].Value, out mavenJumpPower);
            Int32.TryParse(result.Data["mavenMagnetRange"].Value, out mavenMagnetRange);
            Int32.TryParse(result.Data["mavenMagnetSpeed"].Value, out mavenMagnetSpeed);
            Int32.TryParse(result.Data["mavenPowerUpDuration"].Value, out mavenPowerUpDuration);
            Int32.TryParse(result.Data["mavenPowerUpCharges"].Value, out mavenPowerUpCharges);

            PlayerPrefs.SetInt(PlayerPrefsStrings.mavenSpeed, mavenSpeed);
            PlayerPrefs.SetInt(PlayerPrefsStrings.mavenFuel, mavenFuel);
            PlayerPrefs.SetInt(PlayerPrefsStrings.mavenJumpPower, mavenJumpPower);
            PlayerPrefs.SetInt(PlayerPrefsStrings.mavenMagnetRange, mavenMagnetRange);
            PlayerPrefs.SetInt(PlayerPrefsStrings.mavenMagnetSpeed, mavenMagnetSpeed);
            PlayerPrefs.SetInt(PlayerPrefsStrings.mavenPowerUpDuration, mavenPowerUpDuration);
            PlayerPrefs.SetInt(PlayerPrefsStrings.mavenPowerUpCharges, mavenPowerUpCharges);
                       
            UpdateUIElements();

            if (!isMenuOnlySync)
            {
                SetGameplayValuesByUpgradeIndex();
            }           

            loadingAnimation.SetActive(false);

            debugReporter.text = debugReporter.text + "\n" + "GetUpgradePlayerData() success ";
        }, 
        error =>
        {
            if (error.GenerateErrorReport().Contains("Failed to receive data"))
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("NETWORK CONNECTION FAILED. PLEASE MAKE SURE YOU HAVE AN ACTIVE INTERNET CONNECTION!" + error.GenerateErrorReport());
            }
            else
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("GetUpgradePlayerData: " + error.GenerateErrorReport());
            }
            
            loadingAnimation.SetActive(false);
            debugReporter.text = debugReporter.text + "\n" + "GetUpgradePlayerData() failed with reason " + error.GenerateErrorReport();
        });
    }

    private void UpdateUIElements()
    {
        availableDiamonds.text = CurrencyController.diamondCurrencyValue.Value.ToString();
        
        if(mavenSpeed < 8)
        {
            mavenSpeedPrice.text = pricesForUpgrades[mavenSpeed + 1].ToString();
        }
        else
        {
            mavenSpeedPrice.text = string.Empty;
        }

        if (mavenFuel < 8)
        {
            mavenFuelPrice.text = pricesForUpgrades[mavenFuel + 1].ToString();
        }
        else
        {
            mavenFuelPrice.text = string.Empty;
        }

        if (mavenJumpPower < 8)
        {
            mavenJumpPowerPrice.text = pricesForUpgrades[mavenJumpPower + 1].ToString();
        }
        else
        {
            mavenJumpPowerPrice.text = string.Empty;
        }

        if (mavenMagnetRange < 8)
        {
            mavenMagnetRangePrice.text = pricesForUpgrades[mavenMagnetRange + 1].ToString();
        }
        else
        {
            mavenMagnetRangePrice.text = string.Empty;
        }

        if (mavenMagnetSpeed < 8)
        {
            mavenMagnetSpeedPrice.text = pricesForUpgrades[mavenMagnetSpeed + 1].ToString();
        }
        else
        {
            mavenMagnetSpeedPrice.text = string.Empty;
        }

        if (mavenPowerUpCharges < 8)
        {
            mavenPowerUpChargesPrice.text = pricesForUpgrades[mavenPowerUpCharges + 1].ToString();
        }
        else
        {
            mavenPowerUpChargesPrice.text = string.Empty;
        }

        if (mavenPowerUpDuration < 8)
        {
            mavenPowerUpDurationPrice.text = pricesForUpgrades[mavenPowerUpDuration + 1].ToString();
        }
        else
        {
            mavenPowerUpDurationPrice.text = string.Empty;
        }       

        //Whats out so that all upgrades always have the same indexes
        for (int i = 0; i < speedFilled.Length; i++)
        {
            if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration) >= i))
            {
                powerUpDurationFilled[i].SetActive(true);
                powerUpDurationEmpty[i].SetActive(false);
                powerUpDurationUpgradeable[i].SetActive(false);               
            }
            else if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration) < i))
            {
                powerUpDurationFilled[i].SetActive(false);
                powerUpDurationEmpty[i].SetActive(true);
                powerUpDurationUpgradeable[i].SetActive(false);                
            }

            if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges) >= i))
            {
                powerUpChargesFilled[i].SetActive(true);
                powerUpChargesEmpty[i].SetActive(false);
                powerUpChargesUpgradeable[i].SetActive(false);             
            }
            else if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges) < i))
            {
                powerUpChargesFilled[i].SetActive(false);
                powerUpChargesEmpty[i].SetActive(true);
                powerUpChargesUpgradeable[i].SetActive(false);               
            }

            if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed) >= i))
            {
                speedFilled[i].SetActive(true);
                speedEmpty[i].SetActive(false);
                speedUpgradeable[i].SetActive(false);               
            }
            else if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed) < i))
            {
                speedFilled[i].SetActive(false);
                speedEmpty[i].SetActive(true);
                speedUpgradeable[i].SetActive(false);
            }

            if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel) >= i))
            {
                fuelFilled[i].SetActive(true);
                fuelEmpty[i].SetActive(false);
                fuelUpgradeable[i].SetActive(false);
            }
            else if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel) < i))
            {
                fuelFilled[i].SetActive(false);
                fuelEmpty[i].SetActive(true);
                fuelUpgradeable[i].SetActive(false);
            }

            if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower) >= i))
            {
                jumpPowerFilled[i].SetActive(true);
                jumpPowerEmpty[i].SetActive(false);
                jumpPowerUpgradeable[i].SetActive(false);
            }
            else if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower) < i))
            {
                jumpPowerFilled[i].SetActive(false);
                jumpPowerEmpty[i].SetActive(true);
                jumpPowerUpgradeable[i].SetActive(false);
            }

            if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange) >= i))
            {
                magnetRangeFilled[i].SetActive(true);
                magnetRangeEmpty[i].SetActive(false);
                magnetRangeUpgradeable[i].SetActive(false);
            }
            else if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange) < i))
            {
                magnetRangeFilled[i].SetActive(false);
                magnetRangeEmpty[i].SetActive(true);
                magnetRangeUpgradeable[i].SetActive(false);
            }

            if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed) >= i))
            {
                magnetSpeedFilled[i].SetActive(true);
                magnetSpeedEmpty[i].SetActive(false);
                magnetSpeedUpgradeable[i].SetActive(false);
            }
            else if ((PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed) < i))
            {
                magnetSpeedFilled[i].SetActive(false);
                magnetSpeedEmpty[i].SetActive(true);
                magnetSpeedUpgradeable[i].SetActive(false);
            }
        }

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed) < 8)
        {
            if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed) + 1])
            {
                speedUpgradeable[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed) + 1].SetActive(true);
                mavenSpeedPrice.font = fontGreen;
            }
            else
            {
                mavenSpeedPrice.font = fontRed;
            }
        }

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel) < 8)
        {
            if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel) + 1])
            {
                fuelUpgradeable[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel) + 1].SetActive(true);
                mavenFuelPrice.font = fontGreen;
            }
            else
            {
                mavenFuelPrice.font = fontRed;
            }
        }

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower) < 8)
        {
            if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower) + 1])
            {
                jumpPowerUpgradeable[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower) + 1].SetActive(true);
                mavenJumpPowerPrice.font = fontGreen;
            }
            else
            {
                mavenJumpPowerPrice.font = fontRed;
            }
        }

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange) < 8)
        {
            if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange) + 1])
            {
                magnetRangeUpgradeable[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange) + 1].SetActive(true);
                mavenMagnetRangePrice.font = fontGreen;
            }
            else
            {
                mavenMagnetRangePrice.font = fontRed;
            }
        }

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed) < 8)
        {
            if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed) + 1])
            {
                magnetSpeedUpgradeable[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed) + 1].SetActive(true);
                mavenMagnetSpeedPrice.font = fontGreen;
            }
            else
            {
                mavenMagnetSpeedPrice.font = fontRed;
            }
        }

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges) < 8)
        {
            if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges) + 1])
            {
                powerUpChargesUpgradeable[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges) + 1].SetActive(true);
                mavenPowerUpChargesPrice.font = fontGreen;
            }
            else
            {
                mavenPowerUpChargesPrice.font = fontRed;
            }
        }

        if (PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration) < 8)
        {
            if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration) + 1])
            {
                powerUpDurationUpgradeable[PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration) + 1].SetActive(true);
                mavenPowerUpDurationPrice.font = fontGreen;
            }
            else
            {
                mavenPowerUpDurationPrice.font = fontRed;
            }
        }
    }

    /// <summary>
    /// Upgrades one of the upgrade parameters
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="price"></param>
    public void MakeUpgrade(string itemId, int price, int upgradeIndex, GameObject emptyFrame, GameObject filledFrame, GameObject upgradeableFrame) 
    {
        audioManager.PlaySound("buttonSound");

        if (!(CurrencyController.diamondCurrencyValue.Value < price))
        {
            emptyFrame.SetActive(false);
            upgradeableFrame.SetActive(false);
            filledFrame.SetActive(true);

            if (itemId.Contains("mavenSpeed"))
            {
                var currentIndex = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenSpeed);
                PlayerPrefs.SetInt(PlayerPrefsStrings.mavenSpeed, currentIndex + 1);              
                mavenSpeed += 1;
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(price);             
                UpdateUIElements();

                if (upgradeIndex < 8)
                {
                    if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[upgradeIndex + 1])
                    {
                        speedUpgradeable[upgradeIndex + 1].SetActive(true);
                        mavenSpeedPrice.text = pricesForUpgrades[upgradeIndex + 1].ToString();
                    }
                }
                else
                {
                    mavenSpeedPrice.text = string.Empty;
                }
            }
            else if (itemId.Contains("mavenFuel"))
            {
                var currentIndex = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenFuel);
                PlayerPrefs.SetInt(PlayerPrefsStrings.mavenFuel, currentIndex + 1);               
                mavenFuel += 1;
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(price);
                UpdateUIElements();

                if (upgradeIndex < 8)
                {
                    if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[upgradeIndex + 1])
                    {
                        fuelUpgradeable[upgradeIndex + 1].SetActive(true);
                        mavenFuelPrice.text = pricesForUpgrades[upgradeIndex + 1].ToString();
                    }
                }
                else
                {
                    mavenFuelPrice.text = string.Empty;
                }
            }
            else if (itemId.Contains("mavenJumpPower"))
            {
                var currentIndex = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenJumpPower);
                PlayerPrefs.SetInt(PlayerPrefsStrings.mavenJumpPower, currentIndex + 1);                
                mavenJumpPower += 1;
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(price);
                UpdateUIElements();

                if (upgradeIndex < 8)
                {
                    if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[upgradeIndex + 1])
                    {
                        jumpPowerUpgradeable[upgradeIndex + 1].SetActive(true);
                        mavenJumpPowerPrice.text = pricesForUpgrades[upgradeIndex + 1].ToString();
                    }
                }
                else
                {
                    mavenJumpPowerPrice.text = string.Empty;
                }
            }
            else if (itemId.Contains("mavenMagnetRange"))
            {
                var currentIndex = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetRange);
                PlayerPrefs.SetInt(PlayerPrefsStrings.mavenMagnetRange, currentIndex + 1);              
                mavenMagnetRange += 1;
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(price);
                UpdateUIElements();

                if (upgradeIndex < 8)
                {
                    if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[upgradeIndex + 1])
                    {
                        magnetRangeUpgradeable[upgradeIndex + 1].SetActive(true);
                        mavenMagnetRangePrice.text = pricesForUpgrades[upgradeIndex + 1].ToString();
                        UpdateUIElements();
                    }
                }
                else
                {
                    mavenMagnetRangePrice.text = string.Empty;
                }
            }
            else if (itemId.Contains("mavenMagnetSpeed"))
            {
                var currentIndex = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenMagnetSpeed);
                PlayerPrefs.SetInt(PlayerPrefsStrings.mavenMagnetSpeed, currentIndex + 1);               
                mavenMagnetSpeed += 1;
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(price);
                UpdateUIElements();

                if (upgradeIndex < 8)
                {
                    if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[upgradeIndex + 1])
                    {
                        magnetSpeedUpgradeable[upgradeIndex + 1].SetActive(true);
                        mavenMagnetSpeedPrice.text = pricesForUpgrades[upgradeIndex + 1].ToString();
                    }
                }
                else
                {
                    mavenMagnetSpeedPrice.text = string.Empty;
                }
            }
            else if (itemId.Contains("mavenPowerUpDuration"))
            {
                var currentIndex = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpDuration);
                PlayerPrefs.SetInt(PlayerPrefsStrings.mavenPowerUpDuration, currentIndex + 1);
                mavenPowerUpDuration += 1;
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(price);
                UpdateUIElements();

                if (upgradeIndex < 8)
                {
                    if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[upgradeIndex + 1])
                    {
                        powerUpDurationUpgradeable[upgradeIndex + 1].SetActive(true);
                        mavenPowerUpDurationPrice.text = pricesForUpgrades[upgradeIndex + 1].ToString();
                    }
                }
                else
                {
                    mavenPowerUpDurationPrice.text = string.Empty;
                }
            }
            else if (itemId.Contains("mavenPowerUpCharges"))
            {
                var currentIndex = PlayerPrefs.GetInt(PlayerPrefsStrings.mavenPowerUpCharges);                
                PlayerPrefs.SetInt(PlayerPrefsStrings.mavenPowerUpCharges, currentIndex + 1);                
                mavenPowerUpCharges += 1;
                FindObjectOfType<CurrencyController>().WithdrawDiamondCurrency(price);
                UpdateUIElements();

                if (upgradeIndex < 8)
                {
                    if (CurrencyController.diamondCurrencyValue.Value >= pricesForUpgrades[upgradeIndex + 1])
                    {
                        powerUpChargesUpgradeable[upgradeIndex + 1].SetActive(true);
                        mavenPowerUpChargesPrice.text = pricesForUpgrades[upgradeIndex + 1].ToString();
                    }
                }
                else
                {
                    mavenPowerUpChargesPrice.text = string.Empty;
                }
            }

            madeUpgrades = true;
            PlayerPrefs.SetInt(PlayerPrefsStrings.currencyNeedsSync, 1);
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("YOU DON'T HAVE ENOUGH DIAMONDS");
            madeUpgrades = false;
        }
    }

    /// <summary>
    /// Button assignment for closing the upgrades panel
    /// </summary>
    public void CloseUpgradesPanel()
    {
        audioManager.PlaySound("buttonSound");

        UpdateCloudUpgradesPlayerData();
        upgradesScreen.SetActive(false);
        mainMenuItems.SetActive(true);

        debugReporter.text = debugReporter.text + "\n" + CurrencyController.diamondCurrencyValue.Value.ToString() + " diamonds are available before sync";

        if (madeUpgrades)
        {
            debugReporter.text = debugReporter.text + "\n" + "SyncCurrencyWhenOnline() called and needs to add: " + PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToAddForSync).ToString();
            debugReporter.text = debugReporter.text + "\n" + "SyncCurrencyWhenOnline() called and needs to substract: " + PlayerPrefs.GetInt(PlayerPrefsStrings.currencyValueToSubstractForSync).ToString();
            CurrencyController.SyncCurrencyWhenOnline();
            madeUpgrades = false;
        }

        debugReporter.text = debugReporter.text + "\n" + CurrencyController.diamondCurrencyValue.Value.ToString() + " diamonds are available after sync";
    }
}
