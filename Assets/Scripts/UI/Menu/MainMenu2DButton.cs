using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainMenu2DButton : Button2D
{
    private float currentScale;
    [SerializeField] private float normalScale = 1f, touchScale = 1f, scaleSmoothRatio;

    [SerializeField] private AudioClip touchSound;
    private AudioManager audioManager;

    override protected void Start()
    {
        base.Start();

        audioManager = Injector.GetAudioManager(gameObject);
    }

    override protected void Update()
    {
        base.Update();

        float target_scale = beingTouched ? touchScale : normalScale;
        currentScale += (target_scale-currentScale) / (scaleSmoothRatio / Time.deltaTime);

        beingTouched = false;

        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    override protected void OnTouchEnd()
    {
        base.OnTouchEnd();

        audioManager.PlaySound(touchSound);
    }
}
