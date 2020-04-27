using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using UniRx;

public class LevelLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public GameObject menuInterface;
    public GameObject menuEnvironment;
    public GameObject gameOverScreen;
    public GameObject pauseGamePopup;
    public GameObject duringGameUI;   
    public FixedJoystick fixedJoystick;
    public Slider sliderLevelLoader; 
    private int sceneIndex = 0;

    private void Start()
    {
        MainMenuHandler.levelLoaded.Value = false;      
    }  

    /// <summary>
    /// Load level by it's index
    /// </summary>
    /// <param name="index">Index to be loaded</param>
    public void LoadLevel(int index)
    {

        sceneIndex = index;
        menuInterface.SetActive(false);
        menuEnvironment.SetActive(false);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadAsynchronously());
        
    }
    
    /// <summary>
    /// Load async 
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadAsynchronously ()
    {
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneIndex);

        loadingScreen.SetActive(true);

        while (!loadingOperation.isDone)
        {
            float loadingProgress = Mathf.Clamp01(loadingOperation.progress / .9f);
            sliderLevelLoader.value = loadingProgress;   
            yield return null; //Waiting a frame before continuing after the previous line
        }

        if (loadingOperation.isDone)
        {
            loadingScreen.SetActive(false);           
            duringGameUI.SetActive(true);

            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                if (SceneManager.GetActiveScene().buildIndex != 2)
                {
                    FindObjectOfType<UpgradesAndGameplayController>().SetGameplayValuesByUpgradeIndex();
                    MainMenuHandler.levelLoaded.Value = true;

                    if (PlayerPrefs.HasKey(PlayerPrefsStrings.dontDisplayAgainTutorial))
                    {
                        //If value is one, don't display was checked by the user
                        if (PlayerPrefs.GetInt(PlayerPrefsStrings.dontDisplayAgainTutorial) == 1)
                        {
                            // do nothing
                        }
                        else
                        {
                            TutorialController.ActivateTutorialObjects();
                        }
                    }
                    else
                    {
                        PlayerPrefs.SetInt(PlayerPrefsStrings.dontDisplayAgainTutorial, 0);
                        TutorialController.ActivateTutorialObjects();
                    }


                    GameOverScreenController.gameOver.Value = false;
                    FindObjectOfType<UpgradesAndGameplayController>().SetGameplayValuesByUpgradeIndex();
                    FindObjectOfType<RelicUsageController>().SetAlphaToMax();
                }                
            }
            else
            {
                MainMenuHandler.levelLoaded.Value = false;
                duringGameUI.SetActive(false);
                GameObject.FindGameObjectWithTag("audioManager").GetComponent<AudioManager>().PlaySound("menuTheme");
            }
        }
    }
}
