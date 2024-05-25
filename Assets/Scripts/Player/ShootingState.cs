using System.Collections;
using UnityEngine;

public class ShootingState : IPlayerState
{

    public ShootingState()
    {

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

            me.SmoothHorizontalMovement(me.GetTouchX(touch));
        }

        #endregion
    }

    public void ExitState(PlayerController me)
    {
        //Stops the shooting coroutine
        me.SetShooting(false);
    }
}