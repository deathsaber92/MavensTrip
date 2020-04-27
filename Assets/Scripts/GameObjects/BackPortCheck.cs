using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPortCheck : MonoBehaviour
{    private void OnTriggerEnter(Collider collider) 
    {
        if (collider.tag == "Player")
        {
            GameOverScreenController.gameOver.Value = true;
        }
    }
}
