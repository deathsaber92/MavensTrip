using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class DestroyCollider : MonoBehaviour
{
    public static GameObject deactivatedObject;
    private LightningController lightningController;
    public static bool isInsideDestroy;

    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "startSpawn")
        {
            Destroy(collider.gameObject);
            return;
        }
        else if (collider.tag == "deactivator")
        {
            deactivatedObject = collider.gameObject;
            if (lightningController != null)
            {
                lightningController.AllSetInactive();
            }
            ProceduralGenerator.DeactivateChallenge(deactivatedObject);
        }
        else if (collider.tag == "Player" )
        {
            isInsideDestroy = false;
        }
        else
        {
            return;
        }
    }

    private void OnTriggerStay(Collider collider) 
    {
        if (collider.tag == "Player" && isInsideDestroy != true)
        {
            isInsideDestroy = true;
            return;
        }
    }
}
