using System.Collections;
using UniRx;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UniRx.Triggers;
using System;

public class DustStormAdvance : MonoBehaviour
{
    public Transform mavenTarget;
    public PostProcessVolume postProcessVolume;
    public GameObject destroyCollider;

    //min max values for duststorm advance
    public float moveTowardsSpeed;
    public float maxDustStormSpeed = 7.5f;
   
    public float difficultyIncreaseSpeed;  
    public static Vector3 dustStormSpeed;

    public static float realMoveTowardsSpeed;

    private static float _difficultyIncreaseSpeed;
    private ColorGrading _colorGrading;
    private float _distanceBetween;
    protected float _exposureFormula;

    private Vector3 _currentPosition;
    protected Vector3 _moveTowards;    

    private void Start()
    {
        _distanceBetween = mavenTarget.position.x - transform.position.x;

        ProceduralGenerator.jumpsTaken
            .Where(_ => dustStormSpeed.x < maxDustStormSpeed)
            .Subscribe(_ => { UpdateGameSpeed(); });

        MainMenuHandler.levelLoaded
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ => 
            {               
                realMoveTowardsSpeed = moveTowardsSpeed;
                AssignStartDataSets();               
            })
            .AddTo(this);


        AssignStartDataSets();

        this.UpdateAsObservable()
            .Subscribe(_ => UpdateCurrentPosition());
    }  

    /// <summary>
    /// Controls game speed for each jump in MavenCollision.cs
    /// </summary>
    public static void UpdateGameSpeed()
    {
        dustStormSpeed.x += _difficultyIncreaseSpeed;
    }   

    /// <summary>
    /// Update the current player object position
    /// </summary>
    private void UpdateCurrentPosition()
    {
        _currentPosition = transform.position;
    }  

    void Update()
    {
        //Debug.Log("speed for field is: " + dustStormSpeed.x);
    }

    /// <summary>
    /// Assigning the default needed components - to be used in Start() only
    /// </summary>
    public void AssignStartDataSets()
    {
        dustStormSpeed.x = moveTowardsSpeed;
        realMoveTowardsSpeed = moveTowardsSpeed;    
        _difficultyIncreaseSpeed = difficultyIncreaseSpeed;
        _moveTowards = mavenTarget.position;
        _currentPosition = transform.position;
        postProcessVolume.profile.TryGetSettings(out _colorGrading);
        _colorGrading.enabled.value = true;

        //Debug.Log("realMoveTowardsSpeed: " + realMoveTowardsSpeed);
        //Debug.Log("_difficultyIncreaseSpeed: " + _difficultyIncreaseSpeed);
        //Debug.Log("dustStormSpeed.x: " + dustStormSpeed.x);        
    }
}
