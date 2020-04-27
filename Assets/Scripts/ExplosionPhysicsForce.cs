using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class ExplosionPhysicsForce : MonoBehaviour 
{
    public GameObject explosion;
    public float explosionForce = 4;
    public bool exploded = false;
    public float multiplier = 1;
    public int radiusOfExplosion = 10;
    private Rigidbody maven;

    private void Start()
    {
        this.UpdateAsObservable()
            .Where(_ => !explosion.activeSelf)
            .Subscribe(_ => ResetExplodedBool())
            .AddTo(this);

        this.FixedUpdateAsObservable()
            .Where(_ => explosion.activeSelf)
            .Subscribe(_ => Explode())
            .AddTo(this);

        maven = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Checks explosion conditions
    /// </summary>
    private void CheckExplosionCondition()
    {
        if (!exploded)
        {
            Explode();
            exploded = true;
        }

    }

    /// <summary>
    /// Resets the exploded bool
    /// </summary>
    private void ResetExplodedBool()
    {
        exploded = false;
    }

    /// <summary>
    /// Physics explode
    /// </summary>
    private void Explode()
    {
        if (MavenMovementControl.movementAfterRespawn.Value)
        {
            var cols = Physics.OverlapSphere(transform.position, radiusOfExplosion);
            var rigidbodies = new List<Rigidbody>();
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].attachedRigidbody != null && !rigidbodies.Contains(cols[i].attachedRigidbody))
                {
                    rigidbodies.Add(cols[i].attachedRigidbody);
                }
            }

            foreach (var rb in rigidbodies)
            {
                rb.AddExplosionForce(explosionForce * multiplier / maven.mass, transform.position, radiusOfExplosion, multiplier, ForceMode.VelocityChange);
            }
        }
    }
}
