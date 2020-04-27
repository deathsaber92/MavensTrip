using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MavenMovementControl : MonoBehaviour
{
    private AudioManager audioManager;

    //Maven object
    private Rigidbody mavenRigid;
    public GameObject mavenObject;

    //Joystick controller
    private FixedJoystick joystick;
    public Transform cameraTransform;

    //Values to control Maven
    public float acceleration;
    public float groundedDistance;
    public float setRotationFactor;
    public float rotationResetSpeed;

    public static ReactiveProperty<bool> movementAfterRespawn = new ReactiveProperty<bool>(false);

    //Refference variables:
    private float rotationVelocity;
    private float headingHorizontal;
    private float headingVertical;
    private float heading;
    private float rotationFactor;
    private Vector3 startRotations;
    private Vector3 direction;
    private Vector3 forceToAdd;
    [HideInInspector]
    public Vector3 newRotation;
    private Vector3 headingSet;
    private Vector3 forwardForceJoy;
    private Vector3 keyDirection;
    private RaycastHit checkGroundHit;
 
    //Jump parameters
    public static float jumpMaxCharge = 15000;
    public static float jumpMaxChargeResume;
    public float batteryPowerFacor = 1.5f;
    public static ReactiveProperty<float> JumpCharge = new ReactiveProperty<float>(jumpMaxCharge);
    public float dischargeRate;
    public float jumpChargeRate;
    public float setFlyPower;
    private float flyPower;
    private GameObject[] jumpThrusterParticlesObjects;
    private ParticleSystem jumpThrusterParticles1;
    private ParticleSystem jumpThrusterParticles2;
    public ParticleSystem jumpThrusterParticlesMp1;
    public ParticleSystem jumpThrusterParticlesMp2;

    private void Awake()
    {
        jumpThrusterParticles1 = new ParticleSystem();
        jumpThrusterParticles2 = new ParticleSystem(); 
    }

    private void Start()
    {      
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            mavenRigid = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
            cameraTransform = Camera.main.transform;
            SetStartSubscriptions();
        } 
   
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            cameraTransform = Camera.main.transform;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            movementAfterRespawn.Value = true;

            foreach (GameObject player in players)
            {
                SetStartSubscriptions();
            }

        }

        Debug.Log("PUNDEBUG : MavenMovementControl - Start() : Maven component is " + mavenRigid.name);

        
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 2)
        {
            if (jumpThrusterParticles1 == null)
            {
                AssignJumpParticleComponents();
            }
        }
    }
   
    private void SetStartSubscriptions()
    {
        startRotations = mavenRigid.transform.eulerAngles;

        joystick = FindObjectOfType<InterfaceHandler>().fixedJoystick;
        audioManager = GameObject.FindGameObjectWithTag("audioManager").GetComponent<AudioManager>();
        jumpMaxChargeResume = jumpMaxCharge;

        this.UpdateAsObservable()
            .Where(gameOver => GameOverScreenController.gameOver.Value == false)
            .Subscribe(_ =>
            {
                if (SceneManager.GetActiveScene().buildIndex == 1)
                {
                    CalculateForwardForceJoy();
                    FakeRotateOnDrag();
                }
                else
                {
                    CalculateForwardForceJoy();
                    FakeRotateOnDrag();
                }
            })
            .AddTo(this);


        this.UpdateAsObservable()
            .Where(_ => JumpCharge.Value != jumpMaxCharge)
            .Where(gameOver => GameOverScreenController.gameOver.Value == false)
            .Where(jumpPressed => !JoyButtonJump.jumpPressed.Value)
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ => ChargeJump())
            .AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => FixedJoystick.isPressed)
            .Where(gameOver => GameOverScreenController.gameOver.Value == false)
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ =>
            {
                if (SceneManager.GetActiveScene().buildIndex == 1)
                {
                    SetMavenHeading();
                    movementAfterRespawn.Value = true;
                    SetHeadingByCameraPosition();
                }
                else
                {
                    SetMavenHeading();
                    movementAfterRespawn.Value = true;
                    SetHeadingByCameraPosition();
                }
            })
            .AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => JoyButtonJump.jumpPressed.Value && JumpCharge.Value == 0)
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ =>
            {
                JumpCharge.Value = 0;
                JumpParticlesStop();
                JoyButtonJump.jumpPressed.Value = false;
            })
            .AddTo(this);

        this.UpdateAsObservable()
             .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
             .Subscribe(_ => CorrectDischarge())
             .AddTo(this);

        JoyButtonJump.jumpPressed
            .Where(jumpPressed => JoyButtonJump.jumpPressed.Value == true)
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ =>
            {
                if (JumpCharge.Value > 0)
                {
                    JumpParticlesStart();
                }

                flyPower = setFlyPower;
            })
            .AddTo(this);

        JoyButtonJump.jumpPressed
            .Where(jumpPressed => JoyButtonJump.jumpPressed.Value == false)
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ =>
            {
                JumpParticlesStop();
                flyPower = 0;
            })
            .AddTo(this);

        this.UpdateAsObservable()
           .Where(jumpPressed => JoyButtonJump.jumpPressed.Value == true)
           .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
           .Subscribe(_ =>
           {
               DischargeJump();
           })
           .AddTo(this);

        this.FixedUpdateAsObservable()
            .Where(_ => FixedJoystick.isPressed || JoyButtonJump.jumpPressed.Value == true)
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ =>
            {
                if (SceneManager.GetActiveScene().buildIndex == 1)
                {
                    MoveMaven();
                }
                else
                {
                    MoveMaven();
                }
            })
            .AddTo(this);
    }

    public void SetMavenMovementParameters(float speed, float jumpPower, float fuel)
    {
        acceleration = speed;      
        setFlyPower = jumpPower;
        dischargeRate = fuel;
        Debug.Log("acceleration is: " + acceleration);
    }

    /// <summary>
    /// Set heading for maven
    /// </summary>
    private void SetMavenHeading()
    {
        heading = Mathf.Atan2(headingSet.x, headingSet.z) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Assign the jump partile components
    /// </summary>
    private void AssignJumpParticleComponents()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            jumpThrusterParticlesObjects = GameObject.FindGameObjectsWithTag("jumpthrusters");
            jumpThrusterParticles1 = jumpThrusterParticlesObjects[0].GetComponent<ParticleSystem>();
            jumpThrusterParticles2 = jumpThrusterParticlesObjects[1].GetComponent<ParticleSystem>();
        }
    }


    /// <summary>
    /// Jump particles start
    /// </summary>
    private void JumpParticlesStart()
    {
        audioManager.PlaySound("thrusters");

        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            jumpThrusterParticlesMp1.Play();
            jumpThrusterParticlesMp2.Play();
        }
        else
        {
            jumpThrusterParticles1.Play();
            jumpThrusterParticles2.Play();
        }
    }


    /// <summary>
    /// Jump particles stop  
    /// </summary>
    private void JumpParticlesStop()
    {
        audioManager.StopWithFadeAtEnd("thrusters");

        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            jumpThrusterParticlesMp1.Stop();
            jumpThrusterParticlesMp2.Stop();
        }
        else
        {
            jumpThrusterParticles1.Stop();
            jumpThrusterParticles2.Stop();
        }
    }

    /// <summary>
    /// Calculates the force to be applied on maven 
    /// </summary>
    private void CalculateForwardForceJoy()
    {
        //Calibrate movement with camera
        forwardForceJoy = PoolInput(); //Get the heading of maven in the forward force used to move
        forwardForceJoy = JoyCameraSync(1); //This call is made in order to match the force applied to maven with the camera axis ***

        //Calculate forward force:       
        forwardForceJoy = acceleration * forwardForceJoy;
    }

    /// <summary>
    /// Move maven based on force
    /// </summary>
    private void MoveMaven()
    {       
        forceToAdd = new Vector3(forwardForceJoy.x, flyPower, forwardForceJoy.z);
        mavenRigid.AddForce(forceToAdd, ForceMode.Acceleration);
        ////Debug.Log(forwardForceJoy.x + " " + flyPower + " " + forwardForceJoy.z);
    }

    //Get the direction joystick is facing without considering the y axis (Maven should not move on y axis)
    private Vector3 PoolInput()
    {
        Vector3 dir = Vector3.zero;

        dir.z = joystick.Vertical * 3;
        dir.x = joystick.Horizontal * 2;
        dir.y = 0;

        return dir;
    }

    //This is called from two places at the same time. Possible crashes
    //Function is used to calibrate movement direction
    private Vector3 JoyCameraSync(int caseCount)
    {
        if (cameraTransform != null)
        {
            //Assign a new vector to return from ths function
            Vector3 dir = Vector3.zero;

            switch (caseCount)
            {
                //heading is calling this = 0
                //This is transforming space from local to world and matches direction of cameratransform with the headingset ***
                case 0:
                    dir = cameraTransform.TransformDirection(headingSet);
                    break;

                //joystick is calling this = 1
                //This is transforming space from local to world and matches direction of cameratransform with the headingset ***
                case 1:
                    dir = cameraTransform.TransformDirection(forwardForceJoy);
                    dir = DirectionCorrect(0, dir);
                    break;
            }

            dir.Set(dir.x, 0, dir.z);
            return dir;
        }
        else
        {
            //Load the camera in case it is null and apply the forwardForceJoy for the frames the camera is null
            cameraTransform = Camera.main.transform;
            return forwardForceJoy;
        }
    }

    private Vector3 DirectionCorrect(int caseNumber, Vector3 directionValue)
    {
        direction = directionValue;

        switch (caseNumber)
        {
            //Joystick is calling
            //Correction of the direction (normalize and force magnitude)
            case 0:
                direction = direction.normalized * forwardForceJoy.magnitude;
                break;
        }
        return direction;
    }

    /// <summary>
    /// Fake lean maven when moveing
    /// </summary>
    private void FakeRotateOnDrag()
    {
        if (!FixedJoystick.isPressed && rotationFactor != 0)
        {
            if (rotationFactor > 0)
            {
                rotationFactor = rotationFactor - rotationResetSpeed; //This reverts rotation of maven when joystick is released
            }
            else
            {
                rotationFactor = 0; //Making rotationfactor always stay on 0 if it is not > 0
            }
        }

        if (rotationFactor < 0)
        {
            newRotation.x = -rotationFactor - 180;
        }
        else
        {
            newRotation.x = +rotationFactor - 180;
        }

        newRotation.z = startRotations.z - 180;

        if (movementAfterRespawn.Value)
        {
            mavenRigid.transform.localEulerAngles = newRotation;
        }
    }

    /// <summary>
    /// Set grounded state for maven
    /// </summary>
    private void SetGrouned()
    {
        JoyButtonJump.jumpInProgress = false;
    }

    /// <summary>
    /// Set not grounded state for maven
    /// </summary>
    private void SetNotGrounded()
    {
        JoyButtonJump.jumpInProgress = true;
    }

    /// <summary>
    /// Discharge jumpCharge when jumping
    /// </summary>
    private void DischargeJump()
    {
        JumpCharge.Value = JumpCharge.Value - dischargeRate * TimeVariables.timeDeltaTime;
    }

    private void CorrectDischarge()
    {
        if (JumpCharge.Value < 0 || JumpCharge.Value == 0)
        {
            JumpCharge.Value = 0;
            JoyButtonJump.jumpPressed.Value = false;
        }
    }

    /// <summary>
    /// Charge the jumpCharge
    /// </summary>
    private void ChargeJump()
    {
        if (JumpCharge.Value < jumpMaxCharge && MavenThrusters.isGrounded.Value)
        {
            JumpCharge.Value = JumpCharge.Value + jumpChargeRate * TimeVariables.timeDeltaTime;
        }
        else if (JumpCharge.Value >= jumpMaxCharge)
        {
            JumpCharge.Value = jumpMaxCharge;
        }
    }

    /// <summary>
    /// Calibrate heading with camera position
    /// </summary>
    private void SetHeadingByCameraPosition()
    {
        //Get the heading from joystick movement
        headingSet = PoolInput();

        //This call is made in order to match the joystick input axis with the camera axis, heading set is used to control Maven heading ***
        headingSet = JoyCameraSync(0);

        //Set rotation factor based on the setRotationFactor changeable value, using absolute values of the x and z axis forces
        rotationFactor = (System.Math.Abs(forwardForceJoy.x) + System.Math.Abs(forwardForceJoy.z)) * setRotationFactor;

        //Set the heading while fixedjoystick is pressed
        newRotation.y = heading;
    }
}
