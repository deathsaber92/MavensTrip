using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignNewCameraAndRespawn : MonoBehaviour
{
    public Transform cameraSpawnTransform;
    public Transform mavenSpawnTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameManager.set_cameraSpawnTransform = cameraSpawnTransform;
            GameManager.set_playerSpawnTransform = mavenSpawnTransform;
            ProceduralGenerator.currentPlatform = gameObject.transform.parent.gameObject;        
            ////Debug.Log(ProceduralGenerator.currentPlatform.name);
        }        
    }
}
