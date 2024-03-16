using System.Collections;
using UnityEngine;

public class UpgradeState : IPlayerState
{
    private LevelManager levelManager;
    private float touchXTreshold;

    private float touchTime = 0f, touchEndTime = 0f;
    private float tapThreshold = 0.2f;

    private int numOfUpgrades;
    private float initialXPos;
    private float xSpace;

    private int position_index;

    public UpgradeState(LevelManager levelManager, float touchXTreshold)
    {
        this.levelManager = levelManager;
        this.touchXTreshold = touchXTreshold;
    }

    public void EnterState(PlayerController me)
    {
        UpgradeEventCurrentInfo info = levelManager.GetUpgradeEventCurrentInfo();

        //tap stuff
        touchTime = 0f; touchEndTime = 0f;

        //upgrade stuff
        numOfUpgrades = info.numOfUpgrades;
        initialXPos = info.startAnswerX;
        xSpace = info.spaceBetweenAnswers;

        //get nearest X position
        float worldpos_x = me.transform.position.x;
        float min_distance = float.MaxValue;
        
        for (int i = 0; i < numOfUpgrades; i++)
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
        if (Input.touchCount > 0) 
        {
            Touch t = Input.touches[Input.touches.Length - 1];

            #region // Handle taps

            if (Input.touchCount == 1)
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

                        return;
                    }
                }
            }

            #endregion

            float floor_width = Screen.width;
            float treshold = floor_width * touchXTreshold;

            float xx = t.position.x - treshold;
            floor_width -= treshold * 2;

            float relative_touch_x = xx / floor_width;
            relative_touch_x = Mathf.Clamp(relative_touch_x,0,0.99f);

            position_index = (int) Mathf.Floor(relative_touch_x * numOfUpgrades);
        }

        float target_x = initialXPos + position_index * xSpace;

        me.SmoothHorizontalMovement(target_x);

        levelManager.SetHighlitedPlayerTarget(position_index);
    }

    public void ExitState(PlayerController me)
    {
        //throw new System.NotImplementedException();
    }
}