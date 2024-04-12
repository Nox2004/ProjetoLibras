using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : Panel
{
    private Vector3 unactivePosition, activePosition, initialPosition, targetPosition;
    [SerializeField] private CurveValueInterpolator moveAnimation;

    [SerializeField] private AudioClip moveSound;
    
    override protected void Start()
    {
        base.Start();

        rect.localPosition = new Vector3(0f, -rect.sizeDelta.y, 0);

        //set the active and unactive positions up
        unactivePosition = rect.localPosition;
        activePosition = new Vector3(0f, 0f, 0);

        if (active)
        {
            initialPosition = activePosition; targetPosition = activePosition;
        }
        else 
        {
            initialPosition = unactivePosition; targetPosition = unactivePosition;
        }        
    }

    override protected void Update()
    {
        base.Update();

        if (initialPosition != targetPosition)
        {
            rect.localPosition = Vector3.LerpUnclamped(initialPosition, targetPosition, moveAnimation.Update(Time.deltaTime));
            if (moveAnimation.Finished())
            {
                initialPosition = targetPosition;
            }
        }
    }

    override public void SetActive(bool active)
    {
        base.SetActive(active);

        audioManager.PlaySound(moveSound);

        initialPosition = rect.localPosition;
        targetPosition = active ? activePosition : unactivePosition;

        moveAnimation.Reset();
    }
}
