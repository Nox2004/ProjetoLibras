using System.Collections;
using UnityEngine;

public class DeadState : IPlayerState
{

    public DeadState()
    {

    }
    

    public void EnterState(PlayerController me)
    {
        //Starts the shooting coroutine
        me.SetShooting(false);
    }

    public void UpdateState(PlayerController me)
    {
        
    }

    public void ExitState(PlayerController me)
    {
        
    }
}