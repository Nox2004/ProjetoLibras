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

    [Serializable] public class PlayerUpgradeIdEvent : UnityEvent <PlayerUpgradeId> {}
    [SerializeField] private PlayerUpgradeIdEvent UpgradeEvent;
    [HideInInspector] public PlayerUpgradeId status;

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
        if (paused) return;

        base.Update();

        float target_scale = beingTouched ? touchScale : normalScale;
        currentScale += (target_scale-currentScale) / (scaleSmoothRatio / Time.deltaTime);

        beingTouched = false;

        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    override protected void OnTouchEnd()
    {
        UpgradeEvent.Invoke(status);

        audioManager.PlaySound(touchSound);
    }
}
