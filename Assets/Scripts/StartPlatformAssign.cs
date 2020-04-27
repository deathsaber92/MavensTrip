using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPlatformAssign : MonoBehaviour
{
    public Transform cameraSpawnTransform;
    public Transform mavenSpawnTransform;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.set_cameraSpawnTransform = cameraSpawnTransform;
        GameManager.set_playerSpawnTransform = mavenSpawnTransform;
    }    
}
