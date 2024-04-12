using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool paused;

    [SerializeField] private Panel pausePanel, pauseButtonPanel, signsPanel;
    [SerializeField] private GameObject transitionPrefab;
    [SerializeField] private string menuSceneName;
    private ChangeSceneManager sceneManager = Injector.GetSceneManager();

    void Start()
    {
        sceneManager = Injector.GetSceneManager();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    //private List<IPausable> pausables = new List<IPausable>();

    public void PauseGame()
    {
        if (paused) return;

        paused = true;

        //get all monobehaviours in the scene
        MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>();

        //pause all monobehaviours that implement IPausable
        foreach (var monoBehaviour in monoBehaviours)
        {
            if (monoBehaviour is IPausable)
            {
                IPausable pausable = (IPausable)monoBehaviour;
                pausable.Pause();
            }
        }
        
        //hides the pause button and shows the pause panel
        pauseButtonPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!paused) return;

        paused = false;

        //get all monobehaviours in the scene
        MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>();

        //resume all monobehaviours that implement IPausable
        foreach (var monoBehaviour in monoBehaviours)
        {
            if (monoBehaviour is IPausable)
            {
                IPausable pausable = (IPausable)monoBehaviour;
                pausable.Resume();
            }
        }

        //hides the pause panel and shows the pause button
        pauseButtonPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void ShowSigns()
    {
        if (!paused) return;

        signsPanel.SetActive(true);
    }

    public void HideSigns()
    {
        signsPanel.SetActive(false);
    }

    public void GoBackToMenu()
    {
        GameObject transition_obj = Instantiate(transitionPrefab);
        transition_obj.GetComponent<Transition>().targetSceneName = menuSceneName;
    }
}