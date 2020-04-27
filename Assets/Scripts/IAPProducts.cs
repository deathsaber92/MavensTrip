using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;

public class IAPProducts : MonoBehaviour, IStoreListener
{
    private List<CatalogItem> Catalog;
    // The Unity Purchasing system
    private static IStoreController m_StoreController;
    public GameObject shop;
    public TextMeshProUGUI debugReporter;
    public GameObject loadingAnimation;
    public GameObject mainMenuItems;

    public bool IsInitialized
    {
        get
        {
            return m_StoreController != null && Catalog != null;
        }
    }

    /// <summary>
    /// All interface shop elements should point to this. Here we compare the clicked BUTTON with the itemId in catalog. If it matches, purchase that id!
    /// </summary>
    public void OnButtonPress()
    {
        debugReporter.text = debugReporter.text + "\n" + "OnButtonPress() called";
        // Draw menu to purchase items
        foreach (var item in Catalog)
        {
            debugReporter.text = debugReporter.text + "\n" + "OnButtonPress() item from catalog: " + item.ItemId;

            if (EventSystem.current.currentSelectedGameObject.name == item.ItemId)
            {
                // On button click buy a product
                BuyProductID(item.ItemId);
                debugReporter.text = debugReporter.text + "\n" + "OnButtonPress() trying to buy: " + item.ItemId;
            }
        }
    }

    /// <summary>
    /// Request catalog of items from PlayFab when accessing shop BUTTON
    /// </summary>
    public void LaunchShop()
    {
        loadingAnimation.SetActive(true);
        GameObject.FindGameObjectWithTag("audioManager").GetComponent<AudioManager>().PlaySound("buttonSound");

        GetCatalogItemsRequest request = new GetCatalogItemsRequest();
        request.CatalogVersion = "Utility";
       
        debugReporter.text = debugReporter.text + "\n" + "RefreshIAPItems() getting catalog items";

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            try
            {
               PlayFabClientAPI.GetCatalogItems(request,
               result =>
               {
                   Catalog = result.Catalog;
                   // Make UnityIAP initialize
                   debugReporter.text = debugReporter.text + "\n" + "RefreshIAPItems() got catalog items";
                   loadingAnimation.SetActive(false);
                   shop.SetActive(true);
                   mainMenuItems.SetActive(false);
                   InitializePurchasing();
               },
               error =>
               {
                   FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("RefreshIAPItems(): " + error.GenerateErrorReport());
                   debugReporter.text = debugReporter.text + "\n" + "RefreshIAPItems() failed to get catalog items";
                   loadingAnimation.SetActive(false);
               });
            }
            catch (Exception ex)
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("RefreshIAPItems(): " + ex.Message);
                loadingAnimation.SetActive(false);
            }
        }
        else
        {
            loadingAnimation.SetActive(false);
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Connect to the internet first!");
        }
       
    }

    /// <summary>
    /// This should happen when store is opened
    /// </summary>
    public void InitializePurchasing()
    {
        // If IAP is already initialized, return gently
        if (IsInitialized) return;

        // Create a builder for IAP service        
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));

        // Register each item from the catalog
        foreach (var item in Catalog)
        {
            builder.AddProduct(item.ItemId, ProductType.Consumable);
            debugReporter.text = debugReporter.text + "\n" + "InitializePurchasing() adding to builder product: " + item.ItemId;
        }

        // Trigger IAP service initialization
        UnityPurchasing.Initialize(this, builder);
    }


    // This is invoked manually to initiate purchase
    void BuyProductID(string productId)
    {
        // If IAP service has not been initialized, fail hard
        try
        {
            if (!IsInitialized) throw new Exception("SHOP IS NOT YET INITIALIZED! PLEASE TRY AGAIN IN A FEW MOMENTS!");
        }
        catch(Exception ex)
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("BuyProductID(): " + ex.Message);
            debugReporter.text = debugReporter.text + "\n" + "BuyProductID() purchasing is not initialized ";
        }

        // Pass in the product id to initiate purchase
        m_StoreController.InitiatePurchase(productId);
        debugReporter.text = debugReporter.text + "\n" + "InitializePurchasing() adding to builder product: " + productId;
    }

    // This is automatically invoked automatically when IAP service is initialized
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        debugReporter.text = debugReporter.text + "\n" + "OnInitialized()";
        loadingAnimation.SetActive(false);
    }
    // This is automatically invoked automatically when IAP service failed to initialized
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("BuyProductID(): " + error);
        debugReporter.text = debugReporter.text + "\n" + "OnInitializeFailed()";
        loadingAnimation.SetActive(false);
    }

    // This is automatically invoked automatically when purchase failed
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        FindObjectOfType<ShowErrorMessageController>().SetErrorMessage(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        debugReporter.text = debugReporter.text + "\n" + "OnPurchaseFailed()";
        loadingAnimation.SetActive(false);
    }

    // This is invoked automatically when successful purchase is ready to be processed
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        // NOTE: this code does not account for purchases that were pending and are
        // delivered on application start.
        // Production code should account for such case:
        // More: https://docs.unity3d.com/ScriptReference/Purchasing.PurchaseProcessingResult.Pending.html

        loadingAnimation.SetActive(false);

        if (!IsInitialized)
        {
            return PurchaseProcessingResult.Complete;
        }

        // Test edge case where product is unknown
        if (e.purchasedProduct == null)
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Attempted to process purchase with unknown product. Ignoring");
            return PurchaseProcessingResult.Complete;
        }

        // Test edge case where purchase has no receipt
        if (string.IsNullOrEmpty(e.purchasedProduct.receipt))
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Attempted to process purchase with no receipt: ignoring");
            return PurchaseProcessingResult.Complete;
        }

        debugReporter.text = debugReporter.text + "\n" + "Processing transaction: " + e.purchasedProduct.transactionID.ToString();

        // Deserialize receipt
        var googleReceipt = GooglePurchase.FromJson(e.purchasedProduct.receipt);

        // Invoke receipt validation
        // This will not only validate a receipt, but will also grant player corresponding items
        // only if receipt is valid.
        try
        {
            PlayFabClientAPI.ValidateGooglePlayPurchase(new ValidateGooglePlayPurchaseRequest()
            {
                // Pass in currency code in ISO format
                CurrencyCode = e.purchasedProduct.metadata.isoCurrencyCode,
                // Convert and set Purchase price
                PurchasePrice = (uint)(e.purchasedProduct.metadata.localizedPrice * 100),
                // Pass in the receipt
                ReceiptJson = googleReceipt.PayloadData.json,
                // Pass in the signature
                Signature = googleReceipt.PayloadData.signature
            },
            result =>
            {
                debugReporter.text = debugReporter.text + "\n" + "Processing transaction: " + "Validation successful!";
                FindObjectOfType<CurrencyController>().GetCurrencyValue(true);
                FindObjectOfType<ShowErrorMessageController>().SetSuccessMessage("Purchase complete! Thank you!");
                shop.SetActive(false);
            },
            error =>
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Validation failed: " + error.GenerateErrorReport());
                debugReporter.text = debugReporter.text + "\n" + "PurchaseProcessingResult: " + "Validation failed!";
            });
        }
        catch (Exception ex)
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Validation failed: " + ex.Message);
        }
       

        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Class for GooglePurchase 
    /// </summary>
    public class GooglePurchase
    {
        public PayloadData PayloadData;

        // JSON Fields, ! Case-sensitive
        public string Store;
        public string TransactionID;
        public string Payload;

        public static GooglePurchase FromJson(string json)
        {
            var purchase = JsonUtility.FromJson<GooglePurchase>(json);
            purchase.PayloadData = PayloadData.FromJson(purchase.Payload);
            return purchase;
        }
    }

    /// <summary>
    /// Class for the JsonData
    /// </summary>
    public class JsonData
    {
        // JSON Fields, ! Case-sensitive

        public string orderId;
        public string packageName;
        public string productId;
        public long purchaseTime;
        public int purchaseState;
        public string purchaseToken;
    }

    /// <summary>
    /// Class for PayloadData
    /// </summary>
    public class PayloadData
    {
        public JsonData JsonData;

        // JSON Fields, ! Case-sensitive
        public string signature;
        public string json;

        public static PayloadData FromJson(string json)
        {
            var payload = JsonUtility.FromJson<PayloadData>(json);
            payload.JsonData = JsonUtility.FromJson<JsonData>(payload.json);
            return payload;
        }
    }
}
