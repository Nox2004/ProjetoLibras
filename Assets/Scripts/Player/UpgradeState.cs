using System.Collections;
using UnityEngine;

public class UpgradeState : IPlayerState
{
    private UpgradeEventManager upgradeManager;

    private float touchTime = 0f, touchEndTime = 0f;
    private float tapThreshold = 0.2f;

    private int numOfSigns;
    private float initialXPos;
    private float xSpace;

    private int position_index;

    private bool shoot = false, shooted = false;
    private float shootDistanceToTargetTreshold = 0.3f;

    public UpgradeState(LevelManager levelManager)
    {
        upgradeManager = levelManager.GetUpgradeEventManager();
    }

    public void EnterState(PlayerController me)
    {
        UpgradeEventCurrentInfo info = upgradeManager.GetCurrentInfo();

        shoot = false; shooted = false;

        //tap stuff
        touchTime = 0f; touchEndTime = 0f;

        //upgrade stuff
        numOfSigns = info.numOfSignOptions;
        initialXPos = info.startAnswerX;
        xSpace = info.spaceBetweenAnswers;

        //get nearest X position
        float worldpos_x = me.transform.position.x;
        float min_distance = float.MaxValue;
        
        for (int i = 0; i < numOfSigns; i++)
        {
            float x = initialXPos + i * xSpace;
            float dist = Mathf.Abs(worldpos_x - x);
            
            if (dist < min_distance)
            {
                min_distance = dist;
                position_index = i;
            }
        }
    }

    public void UpdateState(PlayerController me)
    {
        if (Input.touchCount > 0 && !shoot) 
        {
            Touch t = Input.touches[Input.touches.Length - 1];

            //get nearest X position
            float worldpos_x = me.GetTouchX(t);
            float min_distance = float.MaxValue;
            int p_index = -1;
            
            for (int i = 0; i < numOfSigns; i++)
            {
                float x = initialXPos + i * xSpace;
                float dist = Mathf.Abs(worldpos_x - x);
                
                if (dist < min_distance)
                {
                    min_distance = dist;
                    p_index = i;
                }
            }

            #region // Handle taps

            if (Input.touchCount == 1 && p_index == position_index)
            {
                // Check touch time
                if (t.phase == TouchPhase.Began)
                {
                    touchTime = Time.time;
                }

                // Check touch end time
                if (t.phase == TouchPhase.Ended)
                {
                    touchEndTime = Time.time;

                    // if within tap threshold
                    if (touchEndTime - touchTime < tapThreshold)
                    {
                        // Tap stuff
                        
                        shoot = true;
                        
                        return;
                    }
                }
            }
            else 
            {
                position_index = p_index;
            }

            #endregion
        }

        float target_x = initialXPos + position_index * xSpace;

        me.SmoothHorizontalMovement(target_x);

        upgradeManager.SetHighlitedPlayerTarget(position_index);

        if (shoot && !shooted && (Mathf.Abs(me.transform.position.x - target_x) < shootDistanceToTargetTreshold))
        {
            me.SimpleShoot();
            //shooted = true;
            shoot = false;
        }
    }

    public void ExitState(PlayerController me)
    {
        //throw new System.NotImplementedException();
    }
}