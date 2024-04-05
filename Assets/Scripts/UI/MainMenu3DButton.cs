using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainMenu3DButton : Button3D
{
    private enum Animation {
        Show,
        Hide,
        None
    }
    
    private Animation currentAnimation = Animation.Show;
    private Vector3 originalRotation, originalPosition;

    [SerializeField, Range(-.55f,.55f)] private float xInCanvas;
    [SerializeField] private Canvas canvas;

    [SerializeField] private CurveValueInterpolator showInterpolator, hideInterpolator;
    private float yRotationOffset, zPositionOffset;

    [SerializeField] float waveMinAmplitude, waveMaxAmplitude, waveMinDuration, waveMaxDuration;
    private WaveValueInterpolator xRotWave, yRotWave, zRotWave;

    [SerializeField] private float zPositionOffsetWhenTouching, zPositionOffsetSmoothRatio;

    [SerializeField] private AudioClip touchSound;
    private AudioSource audioSource;

    override protected void Start()
    {
        base.Start();

        audioSource = gameObject.AddComponent<AudioSource>();

        //Adjust x position in the canvas
        var rect = GetComponent<RectTransform>();
        float new_x_pos = xInCanvas * canvas.pixelRect.width;
        new_x_pos = new_x_pos / canvas.scaleFactor; //Adjust this value to in world coordinates

        rect.anchoredPosition = new Vector2(new_x_pos, rect.anchoredPosition.y); //Set the new x position

        //Get original transform values
        originalRotation = transform.rotation.eulerAngles;
        originalPosition = transform.localPosition;
        
        //Set up the randomic wave interpolators
        float amp = Random.Range(waveMinAmplitude, waveMaxAmplitude);
        float dur = Random.Range(waveMinDuration, waveMaxDuration);

        xRotWave = new WaveValueInterpolator(-amp/2, amp/2, dur);

        amp = Random.Range(waveMinAmplitude, waveMaxAmplitude);
        dur = Random.Range(waveMinDuration, waveMaxDuration);

        yRotWave = new WaveValueInterpolator(-amp/2, amp/2, dur);

        amp = Random.Range(waveMinAmplitude, waveMaxAmplitude);
        dur = Random.Range(waveMinDuration, waveMaxDuration);

        zRotWave = new WaveValueInterpolator(-amp/2, amp/2, dur);

        xRotWave.Play(); yRotWave.Play(); zRotWave.Play();
    }

    override protected void Update()
    {
        base.Update();

        #region // Show/Hide animations

        switch (currentAnimation)
        {
            case Animation.Show:
            {
                yRotationOffset = showInterpolator.Update(Time.deltaTime);

                if (showInterpolator.Finished())
                {
                    control = true;
                    currentAnimation = Animation.None;
                }
            }
            break;
            case Animation.Hide:
            {
                yRotationOffset = hideInterpolator.Update(Time.deltaTime);

                if (hideInterpolator.Finished())
                {

                }
            }
            break;
            case Animation.None:
            {
                
            }    
            break;
        }

        #endregion

        #region // Touching animations

        var zOffsetTarget = 0f;
        if (beingTouched)
        {
            zOffsetTarget = zPositionOffsetWhenTouching;
        }

        zPositionOffset += (zOffsetTarget - zPositionOffset) / (zPositionOffsetSmoothRatio / Time.deltaTime);

        beingTouched = false;

        #endregion

        //Apply the animations
        Vector3 tmp = originalRotation;

        tmp.y += yRotationOffset;

        tmp += new Vector3( xRotWave.Update(Time.deltaTime), 
                            yRotWave.Update(Time.deltaTime), 
                            zRotWave.Update(Time.deltaTime));

        transform.rotation = Quaternion.Euler(tmp);

        tmp = originalPosition;
        tmp.z += zPositionOffset;
        transform.localPosition = tmp;
    }

    public void Hide()
    {
        control = false;

        hideInterpolator.Reset();
        currentAnimation = Animation.Hide;
    }

    public void Show()
    {
        showInterpolator.Reset();
        currentAnimation = Animation.Show;
    }

    protected override void OnTouchEnd()
    {
        base.OnTouchEnd();

        audioSource.PlayOneShot(touchSound);
    }
}
