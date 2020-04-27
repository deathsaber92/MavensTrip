using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;

public class CameraMove : MonoBehaviour
{
    private Transform target;
    public Vector3 cameraDistance; 
    public float timeToTargetPositionInput;

    public static Vector3 positionVelocity = Vector3.zero;
    public static Quaternion rotationVelocity;

    private Vector3 _newPosition;
    private Vector3 _focalSmoothPoint;  
    private Vector3 _cameraPosition;

    private void Start()
    {
        if( SceneManager.GetActiveScene().buildIndex == 1)
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

            _cameraPosition = GameManager.set_cameraSpawnTransform.position;
            this.FixedUpdateAsObservable().Subscribe(_ => UpdatePositionToTargetPosition()).AddTo(this);
            this.FixedUpdateAsObservable().Subscribe(_ => MoveTheCamera()).AddTo(this);
            this.UpdateAsObservable().Subscribe(_ => CameraLookAtTarget()).AddTo(this);
        }
    }
    
    /// <summary>
    /// Update the camera target position to be the target's position
    /// </summary>
    private void UpdatePositionToTargetPosition()
    {
        _newPosition = target.position;
    }   

    /// <summary>
    /// Smoothly moving the cameara from the current position to the target position
    /// </summary>
    private void MoveTheCamera() 
    {
        transform.position = Vector3.SmoothDamp(transform.position, _newPosition + _cameraPosition + cameraDistance, ref positionVelocity, timeToTargetPositionInput * Time.deltaTime);
    }

    /// <summary>
    /// Camera is facing the target position
    /// </summary>
    private void CameraLookAtTarget()  
    {
        transform.LookAt(_newPosition);
    }

}

   

