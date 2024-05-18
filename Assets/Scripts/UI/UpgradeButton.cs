using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class UpgradeButton : Button2D, IPausable
{
    #region //IPausable implementation

    protected bool paused = false;

    public void Pause()
    {
        paused = true;
        startedTouchWhenActive = false;
        control = false;
    }

    public void Resume()
    {
        paused = false;
        control = true;
    }

    #endregion

    [Serializable] public class PlayerUpgradeEvent : UnityEvent <PlayerUpgrade> {}
    [SerializeField] private PlayerUpgradeEvent selectEvent;
    [HideInInspector] public PlayerUpgrade upgrade;
    [HideInInspector] public bool selected = false;

    private float currentScale;
    [SerializeField] private float normalScale = 1f, touchScale = 1f, scaleSmoothRatio;
    [SerializeField] private float extraScaleWhenSelected;

    [SerializeField] private AudioClip touchSound;
    private AudioManager audioManager;



    override protected void Start()
    {
        base.Start();

        GetComponent<Image>().sprite = upgrade.icon;

        audioManager = Injector.GetAudioManager(gameObject);
    }

    override protected void LateUpdate()
    {
        if (paused) return;

        base.LateUpdate();

        float target_scale = beingTouched ? touchScale : normalScale;
        currentScale += (target_scale-currentScale) / (scaleSmoothRatio / Time.deltaTime);

        beingTouched = false;

        transform.localScale = Vector3.one * (currentScale + (selected ? extraScaleWhenSelected : 0f));
    }

    override protected void OnTouchEnd()
    {
        selectEvent.Invoke(upgrade);

        audioManager.PlaySound(touchSound);
    }
}
