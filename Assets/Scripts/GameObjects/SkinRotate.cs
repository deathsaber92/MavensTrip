using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinRotate : MonoBehaviour
{
    public Transform skinRotatorTransform;
    public Quaternion rotationSpeedByAxis;

    private static float _deltaTime;
   
    // Update is called once per frame
    void Update()
    {
        _deltaTime = TimeVariables.timeDeltaTime;
        skinRotatorTransform.transform.Rotate(rotationSpeedByAxis.x * _deltaTime, rotationSpeedByAxis.y * _deltaTime, rotationSpeedByAxis.z * _deltaTime, Space.Self);
    }
}
