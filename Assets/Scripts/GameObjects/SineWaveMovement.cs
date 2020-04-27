using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineWaveMovement : MonoBehaviour
{
    public GameObject movingObject;
    public GameObject resetStartPoint;
    public float speedOfMovement;
    public float distanceOfMovement;
    public bool moveOnX;
    public bool moveOnY;
    public bool moveOnZ;
    private Transform startTransform;

    void Start()
    {
        transform.position = resetStartPoint.transform.position;   
    }

    void Update()
    {
        if (moveOnX)
            transform.position = transform.position + new Vector3(Mathf.Sin(Time.time * distanceOfMovement), 0.0f, 0.0f) * TimeVariables.timeDeltaTime * speedOfMovement;
        else if (moveOnY)
            transform.position = transform.position + new Vector3(0.0f, Mathf.Sin(Time.time * distanceOfMovement), 0.0f) * TimeVariables.timeDeltaTime * speedOfMovement;
        else if (moveOnZ)
            transform.position = transform.position + new Vector3(0.0f, 0.0f, Mathf.Sin(Time.time * distanceOfMovement)) * TimeVariables.timeDeltaTime * speedOfMovement;
    }
}
