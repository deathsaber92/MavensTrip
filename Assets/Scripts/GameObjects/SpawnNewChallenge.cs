using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNewChallenge : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ////Debug.Log("IS SPAWN TIME !!! BEFORE ::::::::::::::::::" + ProceduralGenerator.isSpawnTime.Value);
            ProceduralGenerator.isSpawnTime.Value = true;            
        }
    }
}
