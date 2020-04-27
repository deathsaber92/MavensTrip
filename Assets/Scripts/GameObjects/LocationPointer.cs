using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationPointer : MonoBehaviour
{
    public float range;
    public LayerMask myLayerMask = new LayerMask();
    public GameObject locationPointer;
    public Transform mavenObject;
    private static RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        locationPointer.SetActive(true);
    }

    // Update is called once per frame
    void Update() 
    {
        if (Physics.Raycast(mavenObject.position, -mavenObject.up, out hit, range, myLayerMask))
        {
            locationPointer.transform.position = hit.point + new Vector3(0, 0.07f, 0);
            locationPointer.SetActive(true);
            //Debug.DrawRay(mavenObject.position, -mavenObject.up * range, Color.yellow, 3);
        }
        else
        {
            locationPointer.SetActive(false);
        }
    }
}
