using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateExplosionMarker : MonoBehaviour
{
    public int roty;

    void Update()
    {
        transform.Rotate(0, 0, roty);
    }
}
