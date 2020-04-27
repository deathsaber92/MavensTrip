using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    public AudioManager audioManager;
    public TextMeshProUGUI debugReporter;
    public TMP_FontAsset currentUserDisplayFont;
    public GameObject leaderboardPanel;
    public GameObject listingPrefab;
    public GameObject mainMenuItems;
    public GameObject playerLeaderboardListing;
    public GameObject loadingAnimation;
    public Transform listingContainer;
    private GetLeaderboardRequest requestTopLeaderboard;
    private GetLeaderboardAroundPlayerRequest requestLeaderboardAroundPlayer;
    private static bool playerIsInTop100 = false;
    private static GameObject mainMenuScreen;

    /// <summary>
    /// Updates the leaderboard button
    /// </summary>
    public void GetLeaderboard()
    {
        audioManager.PlaySound("buttonSound");

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            loadingAnimation.SetActive(true);

            if (PlayerPrefs.GetInt(PlayerPrefsStrings.scoreNeedsSync) == 1)
            {
                SetStats(PlayerPrefs.GetInt(PlayerPrefsStrings.highScoreOfflineForSync), true);
            }
            else
            {
                RequestTheLeaderboard();
            }
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("PLEASE CONNECT TO THE INTERNET TO SEE THE LEADERBOARDS!");
            debugReporter.text = debugReporter.text + "\n" + "GetLeaderboard(): Please connect to the internet first!";
        }
    }

    /// <summary>
    /// updates the highscore playerpref
    /// </summary>
    public void GetHighscore()
    {
        FindObjectOfType<GameOverScreenController>().SyncHighscoreWhenOnline();

        try
        {
            requestLeaderboardAroundPlayer = new GetLeaderboardAroundPlayerRequest { StatisticName = "Top Scores" };
            PlayFabClientAPI.GetLeaderboardAroundPlayer(requestLeaderboardAroundPlayer, UpdatePlayerHighscore, UpdatePlayerHighScoreError);
        }
        catch (Exception ex)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("FAILED TO UPDATE HIGHSCORE: " + ex.Message);
            }
            else
            {
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("FOR THE BEST EXPERIENCE YOU SHOULD CONNECT TO THE INTERNET!");
               
            }
            
        }        
    }

    /// <summary>
    /// does literally shit nothing
    /// </summary>
    /// <param name="result"></param>
    private void UpdatePlayerHighScoreError(PlayFabError result)
    {
        //stay silent do not dirupt the user
    }

    /// <summary>
    /// updates the player's highscore on login
    /// </summary>
    /// <param name="result"></param>
    private void UpdatePlayerHighscore(GetLeaderboardAroundPlayerResult result)
    {
        for (int i = 0; i < result.Leaderboard.Count; i++)
        {
            if (Auth.playFabId == result.Leaderboard[i].PlayFabId.ToString())
            {               
                PlayerPrefs.SetInt(PlayerPrefsStrings.playerAccountHighscore, result.Leaderboard[i].StatValue);
            }
        }
    }


    /// <summary>
    /// Leaderboard update failed
    /// </summary>
    /// <param name="result"></param>
    private void OnErrorLeaderboard(PlayFabError result)
    {
        debugReporter.text = debugReporter.text + "\n" + "error getting leaderboard " + " : " + result.GenerateErrorReport();
        if (result.GenerateErrorReport().Contains("SSL"))
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("NETWORK CONNECTION FAILED! PLEASE MAKE SURE YOU HAVE AN ACTIVE INTERNET CONNECTION!");
        }
        else
        {
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("ERROR RETRIEVING LEADERBOARDS: " + result.GenerateErrorReport());
        }
        
        loadingAnimation.SetActive(false);
    }

    /// <summary>
    /// populates with player display name and score value
    /// </summary>
    /// <param name="result"></param>
    private void OnGetLeaderBoard(GetLeaderboardResult result)
    {
        leaderboardPanel.SetActive(true);
        mainMenuItems.SetActive(false);

        for (int i = 0; i < 100; i++)
        {
            if (i < result.Leaderboard.Count)
            {
                PlayerLeaderboardEntry player = result.Leaderboard[i];


                GameObject tempListing = Instantiate(listingPrefab, listingContainer);
                LeaderboardListing leaderboardListing = tempListing.GetComponent<LeaderboardListing>();
                LeaderboardListing playerStatsListing = playerLeaderboardListing.GetComponent<LeaderboardListing>();

                if (player.DisplayName == null)
                {
                    Destroy(tempListing);
                    debugReporter.text = debugReporter.text + "\n" + "Null player name destroyed";
                }
                else
                {
                    if (player.PlayFabId == Auth.playFabId)
                    {
                        playerStatsListing.playerName.text = player.DisplayName.ToString();
                        playerStatsListing.playerScore.text = player.StatValue.ToString();
                        playerStatsListing.playerRank.text = (player.Position + 1).ToString();

                        leaderboardListing.playerName.text = player.DisplayName.ToString();
                        leaderboardListing.playerScore.text = player.StatValue.ToString();
                        leaderboardListing.playerRank.text = (player.Position + 1).ToString();

                        leaderboardListing.playerName.font = currentUserDisplayFont;
                        leaderboardListing.playerScore.font = currentUserDisplayFont;
                        leaderboardListing.playerRank.font = currentUserDisplayFont;
                    }
                    else
                    {
                        leaderboardListing.playerName.text = player.DisplayName.ToString();
                        leaderboardListing.playerScore.text = player.StatValue.ToString();
                        leaderboardListing.playerRank.text = (player.Position + 1).ToString();
                    }
                }
            }
        }
       
        loadingAnimation.SetActive(false);
    }

    /// <summary>
    /// Also displays the player in the end if player is not found in top 100
    /// </summary>
    /// <param name="result"></param>
    private void OnGetLeaderBoardAroundPlayer(GetLeaderboardAroundPlayerResult result)
    {
        debugReporter.text = debugReporter.text + "\n" + "OnGetLeaderBoardAroundPlayer() : Success leaderboard retrieve";

        PlayerLeaderboardEntry player = new PlayerLeaderboardEntry();

        for (int i = 0; i < result.Leaderboard.Count; i++)
        {
            if (Auth.playFabId == result.Leaderboard[i].PlayFabId.ToString())
            {
                player = result.Leaderboard[i];
                PlayerPrefs.SetInt(PlayerPrefsStrings.playerAccountHighscore, player.StatValue);
            }
        }
       
        LeaderboardListing playerStatsListing = playerLeaderboardListing.GetComponent<LeaderboardListing>();

        playerStatsListing.playerName.text = player.DisplayName.ToString();
        playerStatsListing.playerScore.text = player.StatValue.ToString();
        playerStatsListing.playerRank.text = (player.Position + 1).ToString();

        loadingAnimation.SetActive(false);
    }

    /// <summary>
    /// Destroy all game objects created for the leaderboard
    /// </summary>
    public void CloseLeaderboardPanel()
    {
        audioManager.PlaySound("buttonSound");

        leaderboardPanel.SetActive(false);
        mainMenuItems.SetActive(true);

        for (int i = listingContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(listingContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Sets stats of player
    /// </summary>
    public void SetStats(int collectedDiamonds, bool isRequestToOpenLeaderboard)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                     StatisticName = "Top Scores", Value = collectedDiamonds
                }
            }
        },
        result =>
        {
            debugReporter.text = debugReporter.text + "\n" + "User statistics updated with diamonds: " + collectedDiamonds;            
            PlayerPrefs.SetInt(PlayerPrefsStrings.highScoreOfflineForSync, 0);
            PlayerPrefs.SetInt(PlayerPrefsStrings.scoreNeedsSync, 0);           
            
            if (isRequestToOpenLeaderboard)
            {
                RequestTheLeaderboard();
            }
        },
        error =>
        {
            PlayerPrefs.SetInt(PlayerPrefsStrings.highScoreOfflineForSync, 1);
            PlayerPrefs.SetInt(PlayerPrefsStrings.highScoreOfflineForSync, collectedDiamonds);

            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                debugReporter.text = debugReporter.text + "\n" + error.GenerateErrorReport();  
                FindObjectOfType<ShowErrorMessageController>().SetErrorMessage("UNABLE TO RECORD SCORE TO LEADERBOARD: " + error.GenerateErrorReport());
            }
            else
            {
                //stay silent if highscore fails to be published on leaderboard, you don't want to disrupt the player when playing offline
            }
        });       
    }

    private void RequestTheLeaderboard()
    {
        try
        {
            requestTopLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "Top Scores", MaxResultsCount = 100 };
            requestLeaderboardAroundPlayer = new GetLeaderboardAroundPlayerRequest { StatisticName = "Top Scores" };
            PlayFabClientAPI.GetLeaderboard(requestTopLeaderboard, OnGetLeaderBoard, OnErrorLeaderboard);
            loadingAnimation.SetActive(true);
            PlayFabClientAPI.GetLeaderboardAroundPlayer(requestLeaderboardAroundPlayer, OnGetLeaderBoardAroundPlayer, OnErrorLeaderboard);
        }
        catch (Exception ex)
        {
            loadingAnimation.SetActive(false);
            FindObjectOfType<ShowErrorMessageController>().SetErrorMessage(ex.Message);
            debugReporter.text = debugReporter.text + "\n" + "GetLeaderboard(): Failed with error: " + ex.Message;
        }
    }
}
