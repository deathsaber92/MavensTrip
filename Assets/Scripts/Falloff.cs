using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Falloff : MonoBehaviour
{
    private Transform mavenTransform;

    private void Start()
    {
        mavenTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        this.UpdateAsObservable()
            .Buffer(TimeSpan.FromSeconds(0.8f)) 
            .Where(isNotNull => mavenTransform != null)
            .Subscribe(_ => UpdatePositionOfFalloff())
            .AddTo(this);
    }

    private void UpdatePositionOfFalloff()
    {
        transform.position = new Vector3(mavenTransform.position.x, -20, mavenTransform.position.z);
    }
}
