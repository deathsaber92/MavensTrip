using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;

public class ShowErrorMessageController : MonoBehaviour
{
    public GameObject errorWindow;
    public GameObject versionErrorWindow;    
    public GameObject successWindow;
    public GameObject newsWindow;
    public GameObject connectionAnimation;
    public GameObject connectingPricesAnimation;
    public GameObject loadingAnimation;  
    public TextMeshProUGUI errorMessage;
    public TextMeshProUGUI successMessage;
    public TextMeshProUGUI versionErrorText;
    public TextMeshProUGUI newsMessage;

    public void SetErrorMessage(string message)
    {
        errorMessage.text = string.Empty;
        errorMessage.text = message;

        DisplayErrorWindow();
    }

    public void SetVersionErrorMessage(string message) 
    {
        versionErrorText.text = string.Empty;
        versionErrorText.text = message;

        DisplayVersionErrorWindow();
    }

    public void SetNewsMessage(string message)
    {
        newsMessage.text = string.Empty;
        newsMessage.text = message;

        DisplayNewsWindow();
    }

    private void DisplayNewsWindow()
    {
        newsWindow.SetActive(true);        
    }

    internal void SetSuccessMessage(string message)
    {
        successMessage.text = string.Empty;
        successMessage.text = message;
        DisplaySuccessWindow();
    }

    public void DisplayErrorWindow()
    {
        errorWindow.SetActive(true);
        connectionAnimation.SetActive(false);
        connectingPricesAnimation.SetActive(false);
    }

    public void DisplayVersionErrorWindow() 
    {
        versionErrorWindow.SetActive(true);
        connectionAnimation.SetActive(false);
        connectingPricesAnimation.SetActive(false);
    }

    public void DisplaySuccessWindow()
    {
        successWindow.SetActive(true);
        connectionAnimation.SetActive(false);
        connectingPricesAnimation.SetActive(false);
    }

    public void CloseErrorWindow()
    {
        errorWindow.SetActive(false);
        
        if (Auth.isAuthenticated)
        {
            FindObjectOfType<InterfaceHandler>().mainMenuItems.SetActive(true);          
            FindObjectOfType<IAPProducts>().shop.SetActive(false);
        }
        else
        {
            FindObjectOfType<InterfaceHandler>().welcomeScreen.SetActive(true);
        }
    }

    public void CloseVersionErrorWindow()
    {
        versionErrorWindow.SetActive(false);
        Application.Quit();        
    }

    public void CloseNewsWindow()
    {
        newsWindow.SetActive(false);
    }

    public void DontDisplayAgainNewsWindow()
    {
        PlayerPrefs.SetString(PlayerPrefsStrings.newsMessageDisplayed, newsMessage.text);
        newsWindow.SetActive(false);
    }

    public void CloseSuccessWindow()
    {
        successWindow.SetActive(false);
        FindObjectOfType<IAPProducts>().shop.SetActive(false);
        FindObjectOfType<InterfaceHandler>().mainMenuItems.SetActive(true);
    }
}
