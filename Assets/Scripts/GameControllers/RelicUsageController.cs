using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelicUsageController : MonoBehaviour
{
    public AudioManager audioManager;
    private GameObject speedRelicButton;
    private GameObject jumpRelicButton;
    private GameObject explosionResistRelicButton;
    private GameObject relicHighlight;
    private GameObject relicTimeHolder;
    private GameObject particleRelicActive;
    private TextMeshProUGUI relicChargeMax;
    private TextMeshProUGUI relicCharge;
    private TextMeshProUGUI relicTimer;
    private Rigidbody maven;

    private static float relicDurationInSeconds;
    public static int relicChargesMax;
    public static bool autoInteract = false;
    public static ReactiveProperty<int> relicChargeValue = new ReactiveProperty<int>(2);
    private static Color colorRelic;

    protected static bool relicChargeAvailableSet = true;
    private bool parametersReset = true; 
    private float normalGroundedFlyPower;
    private static float timeWhenRelicPressed;
    public static ReactiveProperty<int> timeSinceRelicPressed = new ReactiveProperty<int>(0);
    private MavenMovementControl mavenMovementControl;
    private InterfaceHandler interfaceHandler;
    private static float _normalAcceleration;

    public static bool isRelicActive = false;

    private string relicTypeTransfer;

    private void Awake()
    {
        interfaceHandler = FindObjectOfType<InterfaceHandler>();        
    }

    private void Start()
    {
        relicHighlight = interfaceHandler.highlightRelic;
        relicCharge = interfaceHandler.relicCharge;
        relicTimer = interfaceHandler.relicTimer;
        relicTimeHolder = interfaceHandler.relicTimeHolder;
        relicChargeMax = interfaceHandler.relicChargeMax;

        relicTypeTransfer = RelicController.relicType;

        SelectedRelicSetActive();

        this.UpdateAsObservable()
            .Where(_ => isRelicActive == true)
            .Subscribe(_ => 
            {
                CalculateTimeSinceRelicPressed();
                DisplayTimeSinceRelicPress();
            })
            .AddTo(this);

        relicChargeValue
            .Where(_ => relicChargeValue.Value > 0)
            .Subscribe(_ =>
            {
                if (MavenMovementControl.movementAfterRespawn.Value && MainMenuHandler.levelLoaded.Value)
                {
                    audioManager.PlaySound("relicChargeCollect");
                }  

                if (isRelicActive == false && MainMenuHandler.levelLoaded.Value)
                {
                    SetAlphaToMax();
                }   
                
                RelicChargeTextUpdate();
            })
            .AddTo(this);

        relicChargeValue
            .Where(_ => relicChargeValue.Value == 0)
            .Subscribe(_ =>
            {
                SetAlphaToLower();
                RelicChargeTextUpdate();
            })
            .AddTo(this);

        MainMenuHandler.levelLoaded
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ =>
            {
                //Debug.Log("level loaded is: " + MainMenuHandler.levelLoaded.Value);
                audioManager = FindObjectOfType<AudioManager>().GetComponent<AudioManager>();
                SelectedRelicSetActive();               
                relicTimeHolder.SetActive(false);
                relicHighlight.SetActive(false);
                _normalAcceleration = FindObjectOfType<MavenMovementControl>().acceleration;
                //Debug.Log("normal acceleration: " + _normalAcceleration);
            })
            .AddTo(this);
    }

    public void SetMavenPowerUpParameters(float powerUpDuration, int powerUpCharges)
    {
        relicDurationInSeconds = powerUpDuration;
        relicChargesMax = powerUpCharges;
        relicChargeValue.Value = 1;
        relicCharge.text = relicChargeValue.ToString();
        relicChargeMax.text = relicChargesMax.ToString();
    }

    private void Update()
    {
        if (particleRelicActive == null && SceneManager.GetActiveScene().buildIndex != 0)
        {
            particleRelicActive = GameManager.particleSystemRelic;
        }

        if (!isRelicActive && SceneManager.GetActiveScene().buildIndex != 0 && !parametersReset)
        {
            Relic_ResumeJumpParameters();
            Relic_ResumeSpeedParameters();
            Relic_ResumeExplosionForceParameters();
            parametersReset = true;
        }  

        if (isRelicActive)
        {            
            SetAlphaToLower();
        }
        else
        {
            if (MavenMovementControl.movementAfterRespawn.Value)
            {
                audioManager.StopSound("powerActive");
            }            
        }

        if (maven == null && SceneManager.GetActiveScene().buildIndex == 1)
        {
            maven = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// Makes the selected relic active
    /// </summary>
    private void SelectedRelicSetActive()
    {
        relicTypeTransfer = RelicController.relicType;
        switch (relicTypeTransfer)
        {
            case "jump_power":
                jumpRelicButton = interfaceHandler.jumpRelicButton;
                jumpRelicButton.SetActive(true);
                if (speedRelicButton != null)
                    speedRelicButton.SetActive(false);
                if (explosionResistRelicButton != null)
                    explosionResistRelicButton.SetActive(false);
                break;

            case "movement_speed":
                speedRelicButton = interfaceHandler.speedRelicButton;
                speedRelicButton.SetActive(true);
                if (jumpRelicButton != null)
                    jumpRelicButton.SetActive(false);
                if (explosionResistRelicButton != null)
                    explosionResistRelicButton.SetActive(false);
                break;

            case "explosion_resistance":
                explosionResistRelicButton = interfaceHandler.explosionResistRelicButton;
                explosionResistRelicButton.SetActive(true);
                if (speedRelicButton != null)
                    speedRelicButton.SetActive(false);
                if (jumpRelicButton != null)
                    jumpRelicButton.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Setting alpha to low to the relic button
    /// </summary>
    private void SetAlphaToLower()
    {
        relicTypeTransfer = RelicController.relicType;
        switch (relicTypeTransfer)
        {
            case "jump_power":
                colorRelic = jumpRelicButton.GetComponent<RawImage>().color;
                colorRelic.a = 0.2f;
                jumpRelicButton.GetComponent<RawImage>().color = colorRelic;
                break;

            case "movement_speed":
                colorRelic = speedRelicButton.GetComponent<RawImage>().color;
                colorRelic.a = 0.2f;
                speedRelicButton.GetComponent<RawImage>().color = colorRelic;
                break;

            case "explosion_resistance":
                colorRelic = explosionResistRelicButton.GetComponent<RawImage>().color;
                colorRelic.a = 0.2f;
                explosionResistRelicButton.GetComponent<RawImage>().color = colorRelic;
                break;
        }
       
    }

    /// <summary>
    /// Setting alpha to high for the relic button
    /// </summary>
    public void SetAlphaToMax()
    {
        relicTypeTransfer = RelicController.relicType;
        switch (relicTypeTransfer)
        {
            case "jump_power":
                colorRelic = jumpRelicButton.GetComponent<RawImage>().color;
                colorRelic.a = 1f;
                jumpRelicButton.GetComponent<RawImage>().color = colorRelic;              
                break;

            case "movement_speed":
                colorRelic = speedRelicButton.GetComponent<RawImage>().color;
                colorRelic.a = 1f;
                speedRelicButton.GetComponent<RawImage>().color = colorRelic;
                break;

            case "explosion_resistance":
                colorRelic = explosionResistRelicButton.GetComponent<RawImage>().color;
                colorRelic.a = 1f;
                explosionResistRelicButton.GetComponent<RawImage>().color = colorRelic;
                break;
        }
    }

    /// <summary>
    /// Change the speed parameters
    /// </summary>
    private void Relic_ChangeSpeedParameters()
    {
        Debug.Log("Speed before using relic: " + FindObjectOfType<MavenMovementControl>().acceleration);
        SetRelicPressTime();
        mavenMovementControl.acceleration = _normalAcceleration + _normalAcceleration * 0.5f; // +50% SPEED
        parametersReset = false;
        Debug.Log("Speed after using relic: " + FindObjectOfType<MavenMovementControl>().acceleration);
    }

    /// <summary>
    /// Resume the speed parameters
    /// </summary>
    private void Relic_ResumeSpeedParameters()
    {
        isRelicActive = false;
        mavenMovementControl.acceleration = _normalAcceleration;
        Debug.Log("Speed after relic expired: " + FindObjectOfType<MavenMovementControl>().acceleration);
        Debug.Log("Normal Speed after relic expired: " + _normalAcceleration);
    }

    /// <summary>
    /// Change the jump parameters
    /// </summary>
    private void Relic_ChangeJumpParameters()
    {
        SetRelicPressTime();
        MavenMovementControl.jumpMaxCharge = MavenMovementControl.jumpMaxCharge + MavenMovementControl.jumpMaxCharge * 0.5f; // +30% JUMPCHARGE
        parametersReset = false;
    }

    /// <summary>
    /// Resume the jump parameters
    /// </summary>
    private void Relic_ResumeJumpParameters()
    {
        isRelicActive = false;
        MavenMovementControl.jumpMaxCharge = MavenMovementControl.jumpMaxChargeResume;
    }

    /// <summary>
    /// Change explosion force parameters
    /// </summary>
    private void Relic_ChangeExplosionForceParameters()
    {
        SetRelicPressTime();
        maven.mass = maven.mass + maven.mass * 50;
    }

    private void Relic_ResumeExplosionForceParameters()
    {
        isRelicActive = false;
        maven.mass = 1;
    }

    /// <summary>
    /// Set the time when the relic was pressed
    /// </summary>
    private void SetRelicPressTime()
    {
        audioManager.PlaySound("powerActive");
        isRelicActive = true;
        timeWhenRelicPressed = Time.time;
    }

    /// <summary>
    /// Calculates the time since the relic was pressed
    /// </summary>
    private void CalculateTimeSinceRelicPressed()
    {
        if (MainMenuHandler.levelLoaded.Value == true)
        {
            timeSinceRelicPressed.Value = (int)(Time.time - timeWhenRelicPressed);
        }
        else
        {
            timeSinceRelicPressed.Value = 0;
        }
    }

    /// <summary>
    /// Displayes the relic timer in seconds
    /// </summary>
    private void DisplayTimeSinceRelicPress()
    {
        relicTimer.text = (relicDurationInSeconds - timeSinceRelicPressed.Value).ToString();

        if (timeSinceRelicPressed.Value == relicDurationInSeconds || timeSinceRelicPressed.Value > relicDurationInSeconds)
        {            
            relicHighlight.SetActive(false);
            relicTimeHolder.SetActive(false);
            particleRelicActive.SetActive(false);
            isRelicActive = false;
            if (relicChargeValue.Value != 0)
                SetAlphaToMax();
        }
    }

    /// <summary>
    /// Use the speed relic
    /// </summary>
    public void OnSpeedRelicUse()
    {
        if (relicChargeValue.Value > 0 && !isRelicActive && MavenMovementControl.movementAfterRespawn.Value)
        {
            SetRelicPressTime();            
            Relic_ChangeSpeedParameters();
            relicHighlight.SetActive(true);
            relicTimeHolder.SetActive(true);
            particleRelicActive.SetActive(true);
            RelicChargeUse();
        }
    }

    /// <summary>
    /// Use the jump relic
    /// </summary>
    public void OnJumpRelicUse()
    {
        if (relicChargeValue.Value > 0 && !isRelicActive && MavenMovementControl.movementAfterRespawn.Value)
        {
            SetRelicPressTime();           
            Relic_ChangeJumpParameters();
            relicHighlight.SetActive(true);
            relicTimeHolder.SetActive(true);
            particleRelicActive.SetActive(true);
            RelicChargeUse();
        }
    }

    /// <summary>
    /// Use the jump relic
    /// </summary>
    public void OnExplosionForceReducedRelicUse()
    {
        if (relicChargeValue.Value > 0 && !isRelicActive && MavenMovementControl.movementAfterRespawn.Value)
        {
            SetRelicPressTime();           
            Relic_ChangeExplosionForceParameters();
            relicHighlight.SetActive(true);
            relicTimeHolder.SetActive(true);
            particleRelicActive.SetActive(true);
            RelicChargeUse();
        }
    }

    /// <summary>
    /// Populate relic button interface with objects when level is loaded
    /// </summary>
    public void PopulateRelicButtonInterfaceWithObjects()
    {
        //Set normal values for changed parameters so they can be modified
        mavenMovementControl = FindObjectOfType<MavenMovementControl>();
        normalGroundedFlyPower = mavenMovementControl.setFlyPower;
        _normalAcceleration = mavenMovementControl.acceleration;

        speedRelicButton = interfaceHandler.speedRelicButton;
        jumpRelicButton = interfaceHandler.jumpRelicButton;
        explosionResistRelicButton = interfaceHandler.explosionResistRelicButton;
    }

    /// <summary>
    /// Uses a relic charge and decrease relicChargeValue count
    /// </summary>
    public void RelicChargeUse()
    {
        if (relicChargeValue.Value > 0)
        {
            relicChargeValue.Value -= 1;
        }
    }

    /// <summary>
    /// Updates the relic charge value
    /// </summary>
    public void RelicChargeTextUpdate()
    {
        relicCharge.text = relicChargeValue.ToString();
    }

    /// <summary>
    /// Recharges a relic charge
    /// </summary>
    public static void RelicChargeRecharge()
    {
        if (relicChargeValue.Value < relicChargesMax)
        {
            relicChargeAvailableSet = false;
            relicChargeValue.Value += 1;
        }
    }
}
