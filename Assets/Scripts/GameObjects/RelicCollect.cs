using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicCollect : MonoBehaviour
{    private void OnTriggerEnter(Collider crystal)
    {
        if (crystal.gameObject.tag == "Player")
        {
            RelicUsageController.RelicChargeRecharge();
            ProceduralGenerator.AddChargeBackToQueue(gameObject);
            GameManager.collectParticleEffect.Play();
            GameManager.collectedDiamonds.Value += 1;
        }
    }
}
