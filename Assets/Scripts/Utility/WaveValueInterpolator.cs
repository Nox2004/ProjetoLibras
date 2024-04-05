using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//This class is used to interpolate a value using an animation curve
[Serializable]
public class WaveValueInterpolator
{
    //Animation curve and value 
    [SerializeField] private float val1, val2;

    //Duration of the animation
    [SerializeField] private float duration = 1f;

    [SerializeField] private bool isPlaying = true;
    private float timer = 0f;
    private float rawValue = 0f;
    private float lerpedValue = 0f;

    public WaveValueInterpolator(float val1, float val2, float duration)
    {
        this.val1 = val1;
        this.val2 = val2;
        this.duration = duration;

        isPlaying = true;
    }

    public WaveValueInterpolator Clone()
    {
        return new WaveValueInterpolator(val1, val2, duration);
    }

    //Operational methods
    public void Reset()
    {
        timer = 0f;
        isPlaying = true;
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }
    
    //Info methods
    public bool IsPlaying()
    {
        return isPlaying;
    }

    public float GetValue()
    {
        return lerpedValue;
    }

    public float GetRawValue()
    {
        return rawValue;
    }

    public float Update(float t = -1f)
    {
        if (t == -1f) t = Time.deltaTime;
        
        //In animation
        if (isPlaying)
        {
            timer += t;            
        }
        
        //Get a wavey value between 0 and 1 using sin function
        rawValue = Mathf.Sin((timer/duration) * (Mathf.PI * 2));
        rawValue = (rawValue + 1) / 2;   

        //Lerp the value between val1 and val2
        lerpedValue = Mathf.Lerp(val1, val2, rawValue);

        return lerpedValue;
    }
}
