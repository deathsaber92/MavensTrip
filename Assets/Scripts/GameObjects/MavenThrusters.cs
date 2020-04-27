using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;

public class MavenThrusters : MonoBehaviour {

    private Rigidbody maven;    
    public float thrusterStrength;
    public float thrusterDistance;
    public Transform[] thrusters;
    private Vector3 upwardForce;   
    private float totalDistance;
    public static ReactiveProperty<bool> isGrounded = new ReactiveProperty<bool>(true);
    RaycastHit hit;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            maven = GetComponent<Rigidbody>();

            this.UpdateAsObservable()
                .Subscribe(_ => DoThrustersMagic())
                .AddTo(this);

            this.FixedUpdateAsObservable()
                .Subscribe(_ => CalculateUpwardForce())
                .AddTo(this);
        }       
    }

    /// <summary>
    /// Checking grounded state
    /// </summary>
    private void DoThrustersMagic()
    {       
        totalDistance = 0;

        for (int i = 0; i < thrusters.Length; i++)
        {
            if (Physics.Raycast(thrusters[i].position, thrusters[i].up * -1, out hit, thrusterDistance))
            {
                if (hit.transform.tag == "ground")
                {
                    //The thruster is within thrusterDistance to the gound. How far away?
                    totalDistance += 1 - (hit.distance / thrusterDistance);
                    //Debug.Log("Total distance is calculated: " + totalDistance);
                    isGrounded.Value = true;
                }
            }
            else
            {
                isGrounded.Value = false;
            }
            ///Debug.DrawRay(thrusters[i].position, thrusters[i].up * -5, Color.red);
        }

        totalDistance = totalDistance / thrusters.Length;

        //Calculate how much force to push and correct it by time and mass:
        upwardForce = transform.up * (thrusterStrength * totalDistance * maven.mass);
        //Debug.Log("Upward force calculated: " + upwardForce);
    }

    /// <summary>
    /// Add upward force on maven
    /// </summary>
    private void CalculateUpwardForce()
    {
        maven.AddForceAtPosition(upwardForce, maven.position);
    }
}
