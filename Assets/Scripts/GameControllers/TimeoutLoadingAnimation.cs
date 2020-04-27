using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeoutLoadingAnimation : MonoBehaviour
{
    public GameObject loadingAnimation;
    public float timeToTimeout = 10;
    private bool timeoutCycleDone = false;
    private static float _refreshTimer;
    
    void Start()
    {
        _refreshTimer = timeToTimeout;
    } 
    
    void Update()
    {
        if (loadingAnimation.activeSelf && timeoutCycleDone == true)
        {
            loadingAnimation.SetActive(false);
        }

        if (loadingAnimation.activeSelf)
        {
            timeToTimeout -= Time.deltaTime;
        }
        else
        {
            if (timeToTimeout != _refreshTimer)
            {
                timeToTimeout = _refreshTimer;
                timeoutCycleDone = false;
            }          
        }

        if (timeToTimeout <= 0 && timeoutCycleDone == false)
        {
            timeToTimeout = _refreshTimer;           
            loadingAnimation.SetActive(false);
            timeoutCycleDone = true;
        } 
    }
}
