using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHowToMoveRobot : MonoBehaviour, IPausable
{
    #region //IPausable implementation

    protected bool paused = false;

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion

    [Header("Interpolator")]
    [SerializeField] private RectTransform follow;
    [SerializeField] private float followRatio;
    private float xx;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        xx = rectTransform.anchoredPosition.x;
    }

    void Update()
    {
        if (paused) return;
        
        xx += (follow.anchoredPosition.x - xx) / (followRatio / Time.deltaTime);
        
        rectTransform.anchoredPosition = new Vector2(xx, rectTransform.anchoredPosition.y);
    }
}