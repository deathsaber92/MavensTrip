using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class SkinPurchaseManager : MonoBehaviour
{
    public TextMeshProUGUI debugReporter;
    private GameObject skinPurchasePopup;
    private GameObject notEnoughCurrencyPopup;
    public GameObject loadingAnimation;
    private TextMeshProUGUI purchaseValueInPopup;

    private static string selectedSkinPriceWithoutK;
    private string tappedButton; //this name should be the same as the PlayerPrefs from  UpdateSkinLockStatus() in SkinLockStatusManager script
    private string selectedSkinPrice;
    private int selectedSkinPriceValue;

    private void Start()
    {
        InterfaceHandler handler = FindObjectOfType<InterfaceHandler>();
        skinPurchasePopup = handler.skinPurchasePopup;
        notEnoughCurrencyPopup = handler.notEnoughCurrencyPopup;
        purchaseValueInPopup = handler.purchaseValueInPopup;
    }

    /// <summary>
    /// The button called when clicking any skin item to try and unlock a skin
    /// </summary>
    public void UnlockCurrentSkin()
    {
        debugReporter.text = debugReporter.text + "\n" + "UnlockCurrentSkin() called. Trying to unlock current skin...";        

        selectedSkinPrice = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        debugReporter.text = debugReporter.text + "\n" + "UnlockCurrentSkin() selected skin price is:" + selectedSkinPrice;

        if (selectedSkinPrice != string.Empty && selectedSkinPrice != null)
        {
            selectedSkinPriceWithoutK = selectedSkinPrice.Replace(" K", "000");
            debugReporter.text = debugReporter.text + "\n" + "UnlockCurrentSkin() selected skin price without k is:" + selectedSkinPriceWithoutK;

            selectedSkinPriceValue = Int32.Parse(selectedSkinPriceWithoutK);
            debugReporter.text = debugReporter.text + "\n" + "UnlockCurrentSkin() selectedSkinPriceValue is:" + selectedSkinPriceValue;

            if (selectedSkinPriceValue <= CurrencyController.diamondCurrencyValue.Value)
            {
                purchaseValueInPopup.text = selectedSkinPrice;
                skinPurchasePopup.SetActive(true);
            }
            else
            {
                notEnoughCurrencyPopup.SetActive(true);
            }
        }        
    }

    /// <summary>
    /// Confirm the purchase for the displayed price (fot from playfab server)
    /// </summary>
    public void ConfirmPurchase()
    {
        loadingAnimation.SetActive(true);
        debugReporter.text = debugReporter.text + "\n" + "ConfirmPurchase(): user was ok with the purchase, trying to call playfab api to purchase the skin... ";
        try
        {
            FindObjectOfType<PlayFabInventoryManager>().MakeSkinPurchase(SkinSelectorController.currentSkin, selectedSkinPriceValue);
            debugReporter.text = debugReporter.text + "\n" + "ConfirmPurchase(): trying to purchase the following selected skin: " + SkinSelectorController.currentSkin;
        }
        catch(Exception e)
        {
            debugReporter.text = debugReporter.text + "\n" + "ConfirmPurchase() caught an error: " + e.Message;
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Purchase failed: " + e.Message);
        }
    }

    public void PurchaseComplete()
    {
        FindObjectOfType<SkinLockStatusManager>().UpdateSkinLockStatus();        
        skinPurchasePopup.SetActive(false);
        PlayerPrefs.SetInt(tappedButton, 0);
    }

    public void PurchaseFailed()
    {
        skinPurchasePopup.SetActive(false);
        PlayerPrefs.SetInt(tappedButton, 1);      
    }

    /// <summary>
    /// Declines the purchase for the displayed price
    /// </summary>
    public void DeclinePurchase()
    {
        skinPurchasePopup.SetActive(false);
        debugReporter.text = debugReporter.text + "\n" + "DeclinePurchase(): user refused purchase before calling PlayFab api...";
    }

    /// <summary>
    /// Button in the not enough currency popup
    /// </summary>
    public void NotEnoughCurrencyConfirm()
    {
        notEnoughCurrencyPopup.SetActive(false);
        debugReporter.text = debugReporter.text + "\n" + "NotEnoughCurrencyConfirm(): user acknowledged that there is not enough currency";
    }
}
