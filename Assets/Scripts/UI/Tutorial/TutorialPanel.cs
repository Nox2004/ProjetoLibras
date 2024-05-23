using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour, IPausable
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

    [SerializeField] private bool pauseWhenActive = true;
    [SerializeField] private PauseManager pauseManager;

    private float timeToReappear;
    private float timer = 0f;
    private Panel _panel;
    private Panel panel { get { if (_panel == null) _panel = GetComponent<Panel>(); return _panel; } }

    public bool on = false;
    private bool panelIsOn = false;

    void Start()
    {
        
    }

    public void SetActive(float time)
    {
        panel.SetActive(true);

        timeToReappear = time;
        timer = timeToReappear;

        on = true;
        panelIsOn = true;

        if (pauseWhenActive) pauseManager.PauseGame();
    }

    public void SetUnactive()
    {
        panel.SetActive(false);

        on = false;
        panelIsOn = false;

        if (pauseWhenActive) pauseManager.ResumeGame();
    }

    void Update()
    {
        if (paused) return;
        
        if (on)
        {
            if (Input.touchCount > 0) //if player touches, hides the panel
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    panel.SetActive(false);
                    panelIsOn = false;
                    if (pauseWhenActive) pauseManager.ResumeGame();
                }
            }

            if (!panelIsOn)
            {
                //show panel again if player is confused
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    panel.SetActive(true);
                    panelIsOn = true;
                    timer = timeToReappear;
                }
            }
        }
    }
}