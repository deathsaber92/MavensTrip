using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;

public class JoyButtonJump : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public static bool jumpInProgress = false;

    [HideInInspector]
    public static ReactiveProperty<bool> jumpPressed = new ReactiveProperty<bool>(false);
    public static float timeSinceJumpPress = 0;
    public static float timeOfRelease = 0;
    public static float timeWhenJumpPressed = 0;
    public static int joyButtonPressCounter = 0;
    public static int numberOfJumpTaps = 1000;

    private void Start()
    {
        this.UpdateAsObservable().Where(_ => Input.GetKeyDown(KeyCode.Space)).Subscribe(_ => OnSpacePressDownEvent()).AddTo(this);
        this.UpdateAsObservable().Where(_ => Input.GetKeyUp(KeyCode.Space)).Subscribe(_ => OnSpaceReleaseEvent()).AddTo(this);
        this.UpdateAsObservable().Subscribe(_ => CalculateTimeSinceJumpPressed()).AddTo(this);        
    }   

    /// <summary>
    /// Variables change when pressing spacebar
    /// </summary>
    public void OnSpacePressDownEvent()   
    {
        if (MavenMovementControl.JumpCharge.Value != 0)
        {           
            IsJumpInProgress(true);        
            jumpPressed.Value = true;
            timeWhenJumpPressed = Time.time;
        }        
    }

    /// <summary>
    /// Variables change when releasing spacebar
    /// </summary>
    public void OnSpaceReleaseEvent()
    {
        jumpPressed.Value = false;
        timeOfRelease = Time.time;
    }

    /// <summary>
    /// Assigns a new jump state
    /// </summary>
    /// <param name="state">true = jumping; false = grounded;</param>
    public static void IsJumpInProgress(bool state)
    {
        jumpInProgress = state;
    }

    /// <summary>
    /// Called when jump is pressed
    /// </summary>
    /// <param name="eventData">pointer press event</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (MavenMovementControl.JumpCharge.Value > 0)
        {
            JumpPressedParametersSet(true, TimeVariables.timeDotTime);
        }
        else
        {
            JumpPressedParametersSet(false, TimeVariables.timeDotTime);
        }
    }        

    /// <summary>
    /// Called when jump is realased
    /// </summary>
    /// <param name="eventData">pointer release event</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        JumpPressedParametersSet(false, TimeVariables.timeDotTime);
    }


    /// <summary>
    /// Used to change state of jump variables
    /// </summary>
    /// <param name="isJumpPressed">true = is jumping</param>
    /// <param name="timeWhenJumpReleased">Time.time of each press</param>
    private void JumpPressedParametersSet(bool isJumpPressed, float timeWhenJumpReleased)
    {
        jumpPressed.Value = isJumpPressed;
        timeOfRelease = timeWhenJumpReleased;
    }

    /// <summary>
    /// Caluculated the time since jump was pressed
    /// </summary>
    private void CalculateTimeSinceJumpPressed()
    {
        timeSinceJumpPress = TimeVariables.timeDotTime - timeWhenJumpPressed;
    }
}
