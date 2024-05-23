using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHowToMoveFinger : MonoBehaviour, IPausable
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
    [SerializeField] private WaveValueInterpolator waveyInterpolator;
    private RectTransform rectTransform;

    private Vector2 size;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        size = rectTransform.sizeDelta;
    }

    void Update()
    {
        if (paused) return;
        
        float value = waveyInterpolator.Update();
        
        rectTransform.offsetMin = new Vector2(value, rectTransform.offsetMin.y);
        rectTransform.offsetMax = new Vector2(value, rectTransform.offsetMax.y);

        rectTransform.sizeDelta = size;
    }
}