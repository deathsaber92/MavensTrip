using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using TMPro;
using System;

public class PlayFabInventoryManager : MonoBehaviour
{
    public TextMeshProUGUI debugReporter;
   
    public TextMeshProUGUI priceFounders;
    public TextMeshProUGUI priceExplorer;
    public TextMeshProUGUI priceRhino;
    public TextMeshProUGUI pricePharaon;

    public GameObject lockFounders;
    public GameObject lockExplorer;
    public GameObject lockRhino;
    public GameObject lockPharaon;

    public GameObject availableDefault;  
    public GameObject availableFounders;
    public GameObject availableExplorer;
    public GameObject availableRhino;
    public GameObject availablePharaon;

    public GameObject loadingAnimation;

    /// <summary>
    /// Retrieved and loads the skins prices and their lock status
    /// </summary>
    public void GetSkinPrices() 
    {
        GetCatalogItemsRequest request = new GetCatalogItemsRequest();
        request.CatalogVersion = "Skins";

        PlayFabClientAPI.GetCatalogItems(request, result =>
        {
            debugReporter.text = debugReporter.text + "\n" + "GetSkinPrices(): Got all skins catalog items.";
            List<CatalogItem> skins = result.Catalog;
            debugReporter.text = debugReporter.text + "\n" + "GetSkinPrices(): Catalog has: " + skins.Count + " skins";
            foreach (CatalogItem skin in skins)
            {
                switch (skin.ItemId)
                {  
                    case "mavenFounders":
                        priceFounders.text = Convert000_To_K(skin.VirtualCurrencyPrices["DM"]);
                        debugReporter.text = debugReporter.text + "\n" + "GetSkinPrices(): price of mavenFounders: " + priceFounders.text;
                        break;

                    case "mavenExplorer":
                        priceExplorer.text = Convert000_To_K(skin.VirtualCurrencyPrices["DM"]);
                        debugReporter.text = debugReporter.text + "\n" + "GetSkinPrices(): price of mavenExplorer: " + priceFounders.text;
                        break;

                    case "mavenRhino":
                        priceRhino.text = Convert000_To_K(skin.VirtualCurrencyPrices["DM"]);
                        debugReporter.text = debugReporter.text + "\n" + "GetSkinPrices(): price of mavenRhino: " + priceRhino.text;
                        break;

                    case "mavenPharaon":
                        pricePharaon.text = Convert000_To_K(skin.VirtualCurrencyPrices["DM"]);
                        debugReporter.text = debugReporter.text + "\n" + "GetSkinPrices(): price of mavenPharaon: " + priceRhino.text;
                        break;
                } 
            }
        },
        error =>
        {
            debugReporter.text = debugReporter.text + "\n" + "GetSkinPrices(): Failed getting catalog items" + error.GenerateErrorReport();
        });
    }

    /// <summary>
    /// Convers 000 to K so prices take less space
    /// </summary>
    /// <param name="price"></param>
    /// <returns></returns>
    private string Convert000_To_K(uint price)
    {
        debugReporter.text = debugReporter.text + "\n" + " Convert000_To_K(): price % 1000 = " + (price % 1000).ToString();

        if (price / 1000 >= 1)
        {
            uint restOfDivision1000 = price / 1000;
            return restOfDivision1000.ToString() + " " + "K";           
        }
        else
        {
            return price.ToString();
        }
    }

    /// <summary>
    /// Gets the lock status for all the skins from the user inventory and updates the playerprefs 
    /// </summary>
    public void GetSkinsAndUpgradesLockStatus()
    {
        debugReporter.text = debugReporter.text + "GetskinsAndUpgradesLockStatus() called";

        loadingAnimation.SetActive(true);

        PlayerPrefs.SetInt(PlayerPrefsStrings.skinDefaultLock, 0);
        PlayerPrefs.SetInt(PlayerPrefsStrings.skinFoundersLock, 1);
        PlayerPrefs.SetInt(PlayerPrefsStrings.skinExplorerLock, 1);
        PlayerPrefs.SetInt(PlayerPrefsStrings.skinRhinoLock, 1);
        PlayerPrefs.SetInt(PlayerPrefsStrings.skinPharaonLock, 1);

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), result => 
        {
            debugReporter.text = debugReporter.text + "\n" + "GetUserInventory(): Got all inventory items.";
            loadingAnimation.SetActive(false);
            SetDefaultSkinStates();

            foreach (var item in result.Inventory)
            {
                debugReporter.text = debugReporter.text + "\n" + "GetLockStatus(): foreach inventory retrieved item: " + item.ItemId;

                switch (item.ItemId)
                {
                    case "mavenFounders":
                        PlayerPrefs.SetInt(PlayerPrefsStrings.skinFoundersLock, 0);
                        availableFounders.SetActive(true);
                        lockFounders.SetActive(false);
                        debugReporter.text = debugReporter.text + "\n" + "GetLockStatus(): mavenFounders owned and set accordingly " + PlayerPrefs.GetInt(PlayerPrefsStrings.skinFoundersLock).ToString();
                        break;

                    case "mavenExplorer":
                        PlayerPrefs.SetInt(PlayerPrefsStrings.skinExplorerLock, 0);
                        availableExplorer.SetActive(true);
                        lockExplorer.SetActive(false);
                        debugReporter.text = debugReporter.text + "\n" + "GetLockStatus(): mavenExplorer owned and set accordingly " + PlayerPrefs.GetInt(PlayerPrefsStrings.skinExplorerLock).ToString();
                        break;

                    case "mavenRhino":
                        PlayerPrefs.SetInt(PlayerPrefsStrings.skinRhinoLock, 0);
                        availableRhino.SetActive(true);
                        lockRhino.SetActive(false);
                        debugReporter.text = debugReporter.text + "\n" + "GetLockStatus(): mavenRhino owned and set accordingly " + PlayerPrefs.GetInt(PlayerPrefsStrings.skinExplorerLock).ToString();
                        break;

                    case "mavenPharaon":
                        PlayerPrefs.SetInt(PlayerPrefsStrings.skinPharaonLock, 0);
                        availablePharaon.SetActive(true);
                        lockPharaon.SetActive(false);
                        debugReporter.text = debugReporter.text + "\n" + "GetLockStatus(): mavenPharaon owned and set accordingly " + PlayerPrefs.GetInt(PlayerPrefsStrings.skinExplorerLock).ToString();
                        break;
                }
            }

            //possible that this makes the skin being available when the game is ready
            FindObjectOfType<SkinSelectorController>().SendMessageFromSkinItem(SkinSelectorController.closestItemToCenter);
        },
        error => 
        {
            debugReporter.text = debugReporter.text + "\n" + "GetLockStatus(): Failed getting inventory items. Setting skins to default state. Error was: " + error.GenerateErrorReport();
            loadingAnimation.SetActive(false);
            SetDefaultSkinStates();
        });
    }

    /// <summary>
    /// Sets a new or offline user default skin states
    /// </summary>
    private void SetDefaultSkinStates()
    {
        debugReporter.text = debugReporter.text + "\n" + "SetDefaultSkinStates(): player is either offline or has no other skins than the default one";

        availableDefault.SetActive(true);        
        availableFounders.SetActive(false);
        availableExplorer.SetActive(false);
        availableRhino.SetActive(false);
        availablePharaon.SetActive(false);

        lockFounders.SetActive(true);
        lockExplorer.SetActive(true);
        lockRhino.SetActive(true);
        lockPharaon.SetActive(true);
    }

    /// <summary>
    /// This is used to give a skin tot he user's inventory
    /// </summary>
    /// <param name="skinPurchasedString"></param> 
    public void MakeSkinPurchase(string skinPurchasedItemID, int skinPrice)
    {
        debugReporter.text = debugReporter.text + "\n" + " MakePurchase(): Trying to purchase item: " + skinPurchasedItemID;

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest()
            {
                CatalogVersion = "Skins",
                ItemId = skinPurchasedItemID,
                Price = skinPrice,
                VirtualCurrency = "DM"
            }, result =>
            {
                loadingAnimation.SetActive(false);
                //TBD: Set skin purchase success message
                SkinLockStatusManager.currentSkinLockStatusChanged.Value = true;
                debugReporter.text = debugReporter.text + "\n" + " MakePurchase(): Succesfully purchased item: " + skinPurchasedItemID;
                FindObjectOfType<SkinPurchaseManager>().PurchaseComplete();
                FindObjectOfType<CurrencyController>().GetCurrencyValue(false);
            },
            error =>
            {
                loadingAnimation.SetActive(false);
                debugReporter.text = debugReporter.text + "\n" + " MakePurchase(): Failed to purchase item: " + skinPurchasedItemID + " with error: " + error.GenerateErrorReport();
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Skin purchased failed with error: " + error.GenerateErrorReport());
                FindObjectOfType<SkinPurchaseManager>().PurchaseFailed();
            });
        }   
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("PLEASE CONNECT TO THE INTERNET TO PURCHASE NEW SKINS!");
        }
    }
}
