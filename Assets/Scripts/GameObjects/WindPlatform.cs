using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindPlatform : MonoBehaviour
{
    private void OnTriggerStay(Collider windCollided)
    {
        if (windCollided.gameObject.tag == "Player")
        {
            windCollided.GetComponent<Rigidbody>().AddForce(this.transform.forward * 40);
        }
    }
}
