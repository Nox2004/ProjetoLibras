using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectModeButton : Button2D
{
    private float currentScale;
    [SerializeField] private float normalScale = 1f, touchScale = 1f, scaleSmoothRatio;

    [SerializeField] private AudioClip touchSound;
    private AudioManager audioManager;

    [SerializeField] private Image modeIcon;
    [SerializeField] private TMPro.TextMeshProUGUI modeName;

    [SerializeField] private int currentIndex;
    [SerializeField] private GameObject locker;
    [SerializeField] private MenuEvents menuEvents;
    [SerializeField] private StarHighScore starHighScoreUI;

    public void RightArrow()
    {
        currentIndex++;
        if (currentIndex > GameManager.GetGameModes().Length - 1) currentIndex = 0;
        UpdateMode();
    }

    public void LeftArrow()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = GameManager.GetGameModes().Length - 1;
        UpdateMode();
    }

    private void UpdateMode()
    {
        GameMode mode = GameManager.GetGameModes()[currentIndex];

        modeName.text = mode.name.translatedText;
        modeIcon.sprite = mode.icon;
        locker.SetActive(!mode.unlocked);
        starHighScoreUI.fill(mode.id);
        //update high score
    }

    public void SelectMode()
    {
        GameMode mode = GameManager.GetGameModes()[currentIndex];

        if (mode.unlocked)
        {
            menuEvents.SelectMode(mode.sceneName);
        }
    }

    override protected void Start()
    {
        base.Start();

        audioManager = Injector.GetAudioManager(gameObject);

        currentScale = normalScale;
        transform.localScale = Vector3.one * currentScale;

        UpdateMode();
    }

    override protected void LateUpdate()
    {
        base.LateUpdate();

        float target_scale = beingTouched ? touchScale : normalScale;
        currentScale += (target_scale-currentScale) / (scaleSmoothRatio / Time.deltaTime);

        beingTouched = false;

        transform.localScale = Vector3.one * currentScale;
    }

    override protected void OnTouchEnd()
    {
        base.OnTouchEnd();

        audioManager.PlaySound(touchSound);
    }
}
