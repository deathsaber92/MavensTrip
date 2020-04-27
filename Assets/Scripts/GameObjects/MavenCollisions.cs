using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MavenCollisions : MonoBehaviour
{
    public Rigidbody maven;
    public GameObject particleDanger;
    public static float timeSinceFalloff;

    private Animator currentCheckpointAnimator;

    /// <summary>
    /// Exit trigger collision reactions for maven
    /// </summary>
    /// <param name="playerCollision"></param>
    private void OnTriggerExit(Collider playerCollision)
    {
        if (playerCollision.gameObject.tag == "jump_counter")
        {
            ProceduralGenerator.jumpsTaken.Value++;
            ProceduralGenerator.AddJumpCounterbackToQueue(playerCollision.gameObject);
        }

        if (playerCollision.gameObject.tag == "end_of_level")
        {
            particleDanger.SetActive(false);

            if (DestroyCollider.isInsideDestroy)
            {                
                GameOverScreenController.gameOver.Value = true;               
            } 
        }
    }

    /// <summary>
    /// Enter trigger collistion reactions for maven
    /// </summary>
    /// <param name="playerCollider"></param>
    private void OnTriggerEnter(Collider playerCollider)
    {
        if (playerCollider.gameObject.tag == "falloff")
        {
            GameOverScreenController.gameOver.Value = true;
        }

        if (playerCollider.gameObject.tag == "end_of_level")
        {
            particleDanger.SetActive(true);
        }
    }  
}
