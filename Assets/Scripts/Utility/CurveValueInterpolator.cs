using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//This class is used to interpolate a value using an animation curve
[Serializable]
public class CurveValueInterpolator
{
    //Animation curve and value 
    [SerializeField] private AnimationCurve curve;
    [SerializeField] public float startVal, endVal;

    //Duration of the animation
    [SerializeField] private float duration = 1f;
    
    private float timer = 0f;
    private float lerpedValue = 0f;
    private float curveValue = 0f;

    public CurveValueInterpolator(AnimationCurve curve, float startVal, float endVal, float duration)
    {
        this.curve = curve;
        this.startVal = startVal;
        this.endVal = endVal;
        this.duration = duration;
    }

    public CurveValueInterpolator Clone()
    {
        return new CurveValueInterpolator(curve, startVal, endVal, duration);
    }

    //Operational methods
    public void Reset()
    {
        timer = 0f;
    }
    
    //Info methods
    public bool Finished()
    {
        return (timer >= duration);
    }

    public float GetValue()
    {
        return lerpedValue;
    }

    public float GetRawValue()
    {
        return curveValue;
    }

    public float Update(float t = -1f)
    {
        if (timer >= duration) return lerpedValue;

        if (t == -1f) t = Time.deltaTime;
        
        //Progress timer
        timer += t;
        timer = Mathf.Clamp(timer,0f,duration);

        curveValue = curve.Evaluate(timer / duration); //Get the current curve value (0 - 1)
        lerpedValue = Mathf.LerpUnclamped(startVal, endVal, curveValue); //Set the new evaluated value (start val - end val)

        //Debug.Log("Timer:" + timer + " Duration:" + duration + " CurveValue:" + curveValue + " LerpedValue:" + lerpedValue + " isPlaying:" + (isPlaying ? "True" : "False"));
    
        return lerpedValue;
    }
}
