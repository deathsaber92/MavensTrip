using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class LightningController : MonoBehaviour
{
    public GameObject[] explosions;
    public GameObject platform;
    private static int _randomizer;

    // Start is called before the first frame update
    void Start()
    {
        this.UpdateAsObservable()
            .Buffer(TimeSpan.FromSeconds(ProceduralGenerator._explosionTimeInterval))     
            .Where(_ => MavenMovementControl.movementAfterRespawn.Value == true)        
            .Where(_ => ProceduralGenerator.currentPlatform == platform)
            .Subscribe(_ => RandomSetActive());
    }

    /// <summary>
    /// Activates a random explosion challenge
    /// </summary>
    private void RandomSetActive()
    {
        _randomizer = UnityEngine.Random.Range(0, explosions.Length - 1);
        if (!explosions[_randomizer].activeSelf)
        {
            explosions[_randomizer].SetActive(true);
            explosions[_randomizer].transform.GetChild(0).gameObject.SetActive(true);        
        }       
    }

    /// <summary>
    /// Sets all explosions inactive
    /// </summary>
    public void AllSetInactive()
    {
        for (int i = 0; i < explosions.Length - 1; i++)
        {
            explosions[i].SetActive(false);
        }
    }
}
