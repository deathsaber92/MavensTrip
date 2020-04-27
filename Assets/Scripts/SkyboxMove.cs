using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxMove : MonoBehaviour
{
    public Transform maven;  
   
    void LateUpdate() 
    {
        transform.position = maven.transform.position;
    }
}
