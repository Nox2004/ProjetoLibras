using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class Toggle2D : Button2D
{
    [Serializable] public class BoolEvent : UnityEvent <bool> {}
    [SerializeField] private BoolEvent onTouchBoolEvent;

    private float currentScale;
    [SerializeField] private float normalScale = 1f, touchScale = 1f, scaleSmoothRatio;

    [SerializeField] private AudioClip touchSound;
    private AudioManager audioManager;

    public bool isOn = false;
    [SerializeField] private Sprite onSprite, offSprite;
    private Image image;

    override protected void Start()
    {
        base.Start();

        audioManager = Injector.GetAudioManager(gameObject);
        image = GetComponent<Image>();
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
        isOn = !isOn;
        image.sprite = isOn ? onSprite : offSprite;

        onTouchBoolEvent.Invoke(isOn);

        audioManager.PlaySound(touchSound);
    }

    public void SetValue(bool value)
    {
        if (image == null) image = GetComponent<Image>();
        isOn = value;
        image.sprite = isOn ? onSprite : offSprite;
    }
}
