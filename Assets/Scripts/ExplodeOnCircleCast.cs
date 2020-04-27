using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityStandardAssets.Effects;

public class ExplodeOnCircleCast : MonoBehaviour
{
    public Transform innerCircleTransform;  
    public GameObject explosion;
    private bool exploded = false;
    private GameObject thisGameObject;    

    // Start is called before the first frame update
    void Start()
    {
        thisGameObject = this.gameObject;

        this.UpdateAsObservable()
            .Where(_ => thisGameObject.activeSelf)
            .Where(_ => exploded == false)
            .Subscribe(_ => StartCoroutineFill())
            .AddTo(this);
    }

    /// <summary>
    /// Activates the explosion
    /// </summary>
    private IEnumerator FireTheExplosion()
    {
        explosion.SetActive(true);
        exploded = false;
        yield return StartCoroutine(EndTheExplosionCycle());
    }

    /// <summary>
    /// Start filling the warning aoe circle
    /// </summary>
    private void StartCoroutineFill()
    {
        exploded = true;       
        StartCoroutine(FillCircle());        
    }

    /// <summary>
    /// Coroutine to fill the circle
    /// </summary>
    /// <returns></returns>
    private IEnumerator FillCircle()
    {
        while (innerCircleTransform.localScale.x < 1)
        {            
            innerCircleTransform.localScale = new Vector3(innerCircleTransform.localScale.x + ProceduralGenerator.explosionCircleFillSpeed * TimeVariables.timeDeltaTime, innerCircleTransform.localScale.y + ProceduralGenerator.explosionCircleFillSpeed * TimeVariables.timeDeltaTime, 0);
            yield return new WaitForEndOfFrame();
        }

        yield return StartCoroutine(FireTheExplosion());
    }

    private IEnumerator EndTheExplosionCycle()
    {
        yield return new WaitForSeconds(0.5f);
        innerCircleTransform.localScale = new Vector3(0, 0, 0);
        explosion.SetActive(false);
        thisGameObject.SetActive(false);        
        this.transform.parent.gameObject.SetActive(false);        
    }
}
