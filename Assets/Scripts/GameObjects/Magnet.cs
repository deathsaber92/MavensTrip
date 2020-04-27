using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    public float attractorSpeed;

    public SphereCollider magnetSphereCollider;

    private void OnTriggerStay(Collider other) 
    {        
        if (other.gameObject.tag == "collectible")
        {            
            if (MavenMovementControl.movementAfterRespawn.Value)
            {
                //Debug.Log("Movement after respawn");
                other.transform.position = Vector3.MoveTowards(other.transform.position, transform.position, attractorSpeed * TimeVariables.timeDeltaTime);
            }            
        }   

        if(other.gameObject.tag == "relic_charge")
        {          
            if (MavenMovementControl.movementAfterRespawn.Value)
            {
                //Debug.Log("Movement after respawn");
                other.transform.position = Vector3.MoveTowards(other.transform.position, transform.position, attractorSpeed * TimeVariables.timeDeltaTime);
            }
        }
    }

    public void SetMavenMagnetParameters(float magnetSpeed, float magnetRange)
    {
        attractorSpeed = magnetSpeed;
        magnetSphereCollider.radius = magnetRange;
    }
}
