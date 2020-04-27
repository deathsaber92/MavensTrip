using System;
using TMPro;
using UnityEngine;
using UnityEngine.Monetization;

public class UnityAdsController : MonoBehaviour
{
    //game ids
    private readonly string gameGoogleStoreID = "3198014";
    private readonly string gameAppleStoreID = "3198015";
    private readonly bool testMode = false;

    //store ids
    private readonly string googlePlayStoreId = "NOT CONFIGURED";
    private readonly string appleStoreId = "NOT CONFIGURED";

    //placements
    private readonly string video = "video";
    private readonly string rewardedVideo = "rewardedVideo";

    public static bool rewardedAdFinished = false;

    public TextMeshProUGUI debugReporter;

    private void Start()
    {
        try
        {
            Monetization.Initialize(gameGoogleStoreID, testMode);
            debugReporter.text = debugReporter.text + "\n" + "Monetization initialized: " + Monetization.isInitialized;
        }
        catch (Exception ex)
        {
            debugReporter.text = debugReporter.text + "\n" + "exception: " + ex.Message;
        }
    }

    /// <summary>
    /// Plays a rewarded video ad
    /// </summary>
    public void PlayRewardedAd(string placeToCallFrom)
    {
        debugReporter.text = debugReporter.text + "\n" + "trying to play ad";
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            debugReporter.text = debugReporter.text + "\n" + "Internet reachable and state of monetization is: " + Monetization.IsReady(rewardedVideo);
            if (Monetization.IsReady(rewardedVideo))
            {
                ShowAdPlacementContent ad = null;
                ad = Monetization.GetPlacementContent(rewardedVideo) as ShowAdPlacementContent;

                if (ad != null)
                { 
                    if (placeToCallFrom.Contains("chestReward"))
                    {
                        ad.Show(OnUnityAdChestFinished);
                        return;
                    }
                    else if (placeToCallFrom.Contains("reviveReward"))
                    {

                        ad.Show(OnUnityAdReviveFinished);
                    }                    
                }
                else
                {
                    FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("No ad available at the moment!");
                }
            }
            else
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("We are sorry, there is no ad available at the moment!");
            }
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("Network connection not available!");
        }
    }

    private void OnUnityAdChestFinished(ShowResult finishState)
    {
        //TBD: ADD A STRING WHICH DEFINES WHERE THE ADD WAS CALLED FROM, BASED ON IT THIS SHOULD HAVE DIFFERENT RESULTS
        if (finishState == ShowResult.Finished)
        {
            rewardedAdFinished = true;
            FindObjectOfType<ChestObject>().ReviveAdWatched();
        }
        else if (finishState == ShowResult.Skipped)
        {
            rewardedAdFinished = false;
            FindObjectOfType<ChestObject>().ReviveAdFailed();
        }
        else if (finishState == ShowResult.Failed)
        {
            rewardedAdFinished = false;
            FindObjectOfType<ChestObject>().ReviveAdFailed();
        }
    }

    /// <summary>
    /// Handler methord for the rewards based on which you give or not the reward to the player
    /// </summary>
    /// <param name="finishState"></param>
    private void OnUnityAdReviveFinished(ShowResult finishState)
    {
        //TBD: ADD A STRING WHICH DEFINES WHERE THE ADD WAS CALLED FROM, BASED ON IT THIS SHOULD HAVE DIFFERENT RESULTS
        if (finishState == ShowResult.Finished)
        {
            rewardedAdFinished = true;
            FindObjectOfType<GameOverScreenController>().ReviveAdWatched();
        }
        else if (finishState == ShowResult.Skipped)
        {
            rewardedAdFinished = false;
            FindObjectOfType<GameOverScreenController>().ReviveAdFailed();
        }
        else if (finishState == ShowResult.Failed)
        {
            rewardedAdFinished = false;
            FindObjectOfType<GameOverScreenController>().ReviveAdFailed();
        }
    }
}
