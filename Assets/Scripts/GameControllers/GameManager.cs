using UnityEngine;
using TMPro;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public AudioManager audioManager;

    //Game states static booleans
    public static bool gamePaused = false;     

    //Diamonds collected durint a level
    public static ReactiveProperty<int> collectedDiamonds = new ReactiveProperty<int>(0);

    //Collected diamonds count
    private TextMeshProUGUI _numberOfDiamonds;

    //Game state static integers
    public static int tutorialMessageIndex = 0;

    public static Transform set_cameraSpawnTransform;
    public static Transform set_playerSpawnTransform;    

    //Controlled game objects
    private Rigidbody maven;
    private Transform cameraTransform;    

    private GameObject _inGameUIElements;
    private GameObject _duringGameUI;
    private GameObject _pauseGamePopup; 

    //Collect and Relic particle effect
    public static ParticleSystem collectParticleEffect;
    public static GameObject particleSystemRelic;

    private void Awake()
    {
        MainMenuHandler.menuLoaded.Value = false;
        Time.timeScale = 1;
        collectedDiamonds.Value = 0;      
    }

    private void Start()
    {
        GetUIElements();
        GameOverScreenController.gameOver.Value = false;
        audioManager.PlaySound("menuTheme");

        GameOverScreenController.gameOver
            .Where(gameOver => GameOverScreenController.gameOver.Value == true) 
            .Subscribe(_ =>
            {
                maven.isKinematic = true;
                maven.isKinematic = false;
            })
            .AddTo(this);

        MainMenuHandler.levelLoaded
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ =>
            {
                _duringGameUI.SetActive(true);
                collectedDiamonds.Value = 0;

                audioManager.PlayRandomInGameTheme();                

                UpdateCollectedDiamondsValue();
                UpdateComponentsOfGame();
                UpdateMavenPositionOnReload();
                //TBD if anything about upgrads break this could be the reason: uncomment below
                //FindObjectOfType<UpgradesAndGameplayController>().GetUpgradePlayerData(false);              
            })
            .AddTo(this);

        collectedDiamonds.Subscribe(_ =>
        {
            if (MainMenuHandler.levelLoaded.Value)
            {
                UpdateCollectedDiamondsValue();
                audioManager.PlaySound("diamondCollect");                
            }           
        })
        .AddTo(this);

        GameOverScreenController.revive
            .Where(revive => GameOverScreenController.revive.Value == true)
            .Subscribe(_ =>
            {
                ResetPlayerPosition();
                GameOverScreenController.revive.Value = false;
            })
            .AddTo(this);        

//#if UNITY_EDITOR    
//        this.UpdateAsObservable().Where(_ => Input.GetKeyDown("r")).Subscribe(_ => ResetPlayerPosition());
//#endif        
    }

    /// <summary>
    /// Updates the required game components each time the level loads
    /// </summary>
    private void UpdateComponentsOfGame()
    {
        FindObjectOfType<RelicUsageController>().PopulateRelicButtonInterfaceWithObjects();
        particleSystemRelic = GameObject.FindGameObjectWithTag("ParticlesBoost");
        particleSystemRelic.SetActive(false);
        GameObject collectParticleEffectGameObject = GameObject.FindGameObjectWithTag("ParticlesCollect");         
        collectParticleEffect = collectParticleEffectGameObject.GetComponent<ParticleSystem>();
        collectParticleEffectGameObject.SetActive(true);
        maven = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        cameraTransform = Camera.main.GetComponent<Transform>();
        cameraTransform = set_playerSpawnTransform;        
    }

    /// <summary>
    /// Helper to update maven position when reloading already instanced scene without transition through menu
    /// </summary>
    public void UpdateMavenPositionOnReload()
    {
        maven.transform.position = set_playerSpawnTransform.position;
        maven.transform.rotation = set_playerSpawnTransform.rotation;
    }

    /// <summary>
    /// Load the UI elements
    /// </summary>
    private void GetUIElements()
    {
        InterfaceHandler handler = FindObjectOfType<InterfaceHandler>();
        _inGameUIElements = handler.duringGameUI;
        _numberOfDiamonds = handler.numberOfDiamonds;
        _pauseGamePopup = handler.pauseGamePopup;
        _duringGameUI = handler.duringGameUI;
    }

    /// <summary>
    /// Resets the player position 
    /// </summary>
    public void ResetPlayerPosition()
    {
        GameOverScreenController.gameOver.Value = false;
        GameOverScreenController.revive.Value = false;
        MavenMovementControl.movementAfterRespawn.Value = false;
        JoyButtonJump.jumpPressed.Value = false;
        FixedJoystick.isPressed = false;
        cameraTransform = set_cameraSpawnTransform;
        maven.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        maven.isKinematic = true;
        maven.isKinematic = false;
        maven.collisionDetectionMode = CollisionDetectionMode.Continuous;
        UpdateMavenPositionOnReload();
        MavenMovementControl.JumpCharge.Value = MavenMovementControl.jumpMaxCharge;
    } 

    /// <summary>
    /// Pausing the game
    /// </summary>
    public static void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0;
    }

    /// <summary>
    /// Resuming the game
    /// </summary>
    public static void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1;
    }  

    /// <summary>
    /// Updates the collected diamonds value
    /// </summary>
    private void UpdateCollectedDiamondsValue()
    {
        _numberOfDiamonds.text = (collectedDiamonds).ToString();      
    }  
}

