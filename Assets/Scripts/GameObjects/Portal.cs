using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform portalIn;
    public Transform portalOut;

    protected Transform maven;
    public bool isUsed;

    private void Start()
    {
        maven = GameObject.FindGameObjectWithTag("Player").transform;
        //Debug.Log(maven.name);
    }

    private void OnTriggerExit(Collider playerCollision)
    {
        if (playerCollision.gameObject.tag == "Player")
        {
            isUsed = false;
        }
    }

    /// <summary>
    /// When entering a portal setting isArrived to true so Maven cannot go back inside the portal
    /// </summary>
    /// <param name="playerCollision"></param>
    private void OnTriggerEnter(Collider playerCollision)
    {
        if (playerCollision.gameObject.tag == "Player")
        {
            if (isUsed == false)
            {
                GameManager.collectedDiamonds.Value += 3 * Convert.ToInt32(DustStormAdvance.dustStormSpeed.x);
                Debug.Log("Portal gave: " + 3 * Convert.ToInt32(DustStormAdvance.dustStormSpeed.x));
                portalOut.GetComponent<Portal>().isUsed = true;
                isUsed = true;
                maven.transform.position = portalOut.transform.position;
                Debug.Log(maven.name + " should move");
            }
        }
    }
}
