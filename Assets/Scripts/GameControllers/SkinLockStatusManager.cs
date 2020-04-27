using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class SkinLockStatusManager : MonoBehaviour
{
    public TextMeshProUGUI debugReporter;
    public static bool currentSkinOwned;
    public static ReactiveProperty<bool> currentSkinLockStatusChanged = new ReactiveProperty<bool>(false);

    void Start()
    {    
        currentSkinLockStatusChanged
            .Where(currentSkinLockStatusChanged => currentSkinLockStatusChanged == true)
            .Subscribe(_ => UpdateSkinLockStatus())
            .AddTo(this);
    }    
    
    /// <summary>
    /// Updates the skin lock status
    /// Look in ConfirmPurchase() from SkinPurchaseManager script. The PlayerPrefs "skinDefaultLock" must have the same name with the button conent of the skin selector scroll view in order to work
    /// </summary>
    public void UpdateSkinLockStatus()
    {
        debugReporter.text = debugReporter.text + "\n" + " UpdateSkinLockStatus(): Trying to refresh lock status. Will call GetLockStatus()";       
        FindObjectOfType<PlayFabInventoryManager>().GetSkinsAndUpgradesLockStatus();

        currentSkinLockStatusChanged.Value = false;
        debugReporter.text = debugReporter.text + "\n" + " UpdateSkinLockStatus(): Changed currentSkinLockStatusChanged back to false: " + currentSkinLockStatusChanged.Value;      
    }
}
