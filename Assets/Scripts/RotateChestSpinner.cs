﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateChestSpinner : MonoBehaviour
{
    public GameObject spinner;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, speed);
    }
}
