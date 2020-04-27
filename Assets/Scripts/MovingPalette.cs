using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPalette : MonoBehaviour
{
    public int rotationSpeed;

    private void Start()
    {
        rotationSpeed = 50;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationSpeed * TimeVariables.timeDeltaTime, 0, 0, Space.Self);
    }
}
