using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryrtalCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider crystal)
    {
        if (crystal.gameObject.tag == "Player")
        {
            ProceduralGenerator.AddDiamondBackToQueue(gameObject);
            GameManager.collectedDiamonds.Value += 1;
            GameManager.collectParticleEffect.Play();                  
        }
    }   
}
