using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class TimeVariables : MonoBehaviour
{
    public static float timeDeltaTime;
    public static float timeDotTime;
    public static float timeFixedUpdateTime;

    private void Start()
    {
        this.UpdateAsObservable().Subscribe(_ => UpdateTimeDeltaTime());
        this.UpdateAsObservable().Subscribe(_ => UpdateTimeDotTime());
        this.UpdateAsObservable().Subscribe(_ => UpdateTimeFixedUpdateTime());
    }

    /// <summary>
    /// Updates timeDeltaTime variable once for all solution usage
    /// </summary>
    private void UpdateTimeDeltaTime() 
    {
        timeDeltaTime = Time.deltaTime;
    }

    /// <summary>
    /// Updates timeDotTime variable once for all solution usage
    /// </summary>
    private void UpdateTimeDotTime()
    {
        timeDotTime = Time.time;
    }

    private void UpdateTimeFixedUpdateTime()
    {
        timeFixedUpdateTime = Time.fixedDeltaTime;
    }
}
