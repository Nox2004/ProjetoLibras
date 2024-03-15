using System.Collections;
using UnityEngine;

public class ShootingState : IPlayerState
{
    private float xStart;
    private float xRange;
    private float touchXTreshold;
    private float smoothMoveRatio;
    private float xTarget;

    public ShootingState(float xStart, float xRange, float touchXTreshold, float smoothMoveRatio)
    {
        this.xStart = xStart;
        this.xRange = xRange;
        this.touchXTreshold = touchXTreshold;
        this.smoothMoveRatio = smoothMoveRatio;
    }
    

    public void EnterState(PlayerController me)
    {
        //Starts the shooting coroutine
        me.SetShooting(true);
    }

    public void UpdateState(PlayerController me)
    {
        #region //Moves horizontally based on touch position on the screen

        if (Input.touchCount > 0)
        {
            //Gets the last touch
            Touch touch = Input.GetTouch(Input.touchCount-1);
            
            //Calculates treshold for touch position to be considered as a movement
            float s_width = Screen.width;
            float treshold = s_width * touchXTreshold; 

            //Gets touch X position and converts it to a relative position (0 - left, 1 - right)
            float relative_touch_x = (touch.position.x - treshold) / (s_width - (treshold * 2));
            relative_touch_x = Mathf.Clamp(relative_touch_x, 0, 1); //Clamps the relative position to be between 0 and 1

            relative_touch_x = relative_touch_x - 0.5f; //shifts the relative position to be centered at 0

            //Calculates desired X position of the object in world using xRange property
            xTarget = xStart + xRange * relative_touch_x;

            //Smoothly moves the player to the target X position
            float step = Mathf.Abs(me.transform.position.x - xTarget) / (smoothMoveRatio / Time.deltaTime);

            Vector3 target_pos = me.transform.position;
            target_pos.x = xTarget;
            me.transform.position = Vector3.MoveTowards(me.transform.position, target_pos, step);
        }

        #endregion
    }

    public void ExitState(PlayerController me)
    {
        //Stops the shooting coroutine
        me.SetShooting(false);
    }
}