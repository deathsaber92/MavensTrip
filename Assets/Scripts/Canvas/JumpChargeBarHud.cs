using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class JumpChargeBarHud : MonoBehaviour {

    private float barScale;
    private Vector3 positionModifier;
    private Vector3 scale;

    private void Start()
    {
        this.UpdateAsObservable()
            .Where(levelLoaded => MainMenuHandler.levelLoaded.Value == true)
            .Subscribe(_ => UpdateBarScale())
            .AddTo(this);
    }

    /// <summary>
    /// Updates bar scale when jumping
    /// </summary>
    private void UpdateBarScale()
    {
        if (MavenMovementControl.JumpCharge.Value > -1)
        {
            barScale = MavenMovementControl.JumpCharge.Value / MavenMovementControl.jumpMaxCharge;
            scale = transform.localScale;
            if (barScale > 1)
            {
                scale.Set(1, 1, 1);
            }
            else
            {
                scale.Set(barScale, 1, 1);
            }
            
            transform.localScale = scale;
        }        
    }
}
