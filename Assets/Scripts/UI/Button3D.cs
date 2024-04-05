using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Button3D : MonoBehaviour
{
    protected bool control, beingTouched;
    
    //OnTouch event
    [SerializeField] protected UnityEvent onTouchEvent;

    public void HandleTouch(bool ended)
    {
        if (!control) return;
        
        if (ended)
        {
            OnTouchEnd();
        }

        DuringTouch();
    }
    
    virtual protected void DuringTouch()
    {
        if (!beingTouched) beingTouched = true;
    }

    virtual protected void OnTouchEnd()
    {
        onTouchEvent.Invoke();
    }

    virtual protected void Start()
    {
        control = true;
    }

    virtual protected void Update()
    {
        
    }
}
