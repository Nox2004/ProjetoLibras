using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool paused;

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Panel pausePanel, pauseButtonPanel, signsPanel, gameOverPanel;
    [SerializeField] private GameObject transitionPrefab;
    [SerializeField] private string menuSceneName;

    [SerializeField] private StarHighScore highScoreUIElement;

    private ChangeSceneManager sceneManager = Injector.GetSceneManager();

    //private List<IPausable> pausables = new List<IPausable>();

    void Start()
    {
        sceneManager = Injector.GetSceneManager();
    }
    
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

    public void PauseButton()
    {
        PauseGame();

        //hides the pause button and shows the pause panel
        pauseButtonPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ActivateGameOverScreen()
    {
        PauseGame();

        //hides the pause button and shows the game over panel
        pauseButtonPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        highScoreUIElement.fill(levelManager.gameModeID);
    }

    public void DeactivateGameOverScreen()
    {
        ResumeGame();

        //hides the pause button and shows the game over panel
        pauseButtonPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowSigns()
    {
        if (!paused) return;

        signsPanel.SetActive(true);
        pausePanel.SetButtonsActive(false);
    }

    public void HideSigns()
    {
        signsPanel.SetActive(false);
        pausePanel.SetButtonsActive(true);
    }

    public void GoBackToMenu()
    {
        MonetizationManager manager = MonetizationManager.Instance;

        if(!manager.monetization.HasPurchased("removeads"))
        {
            if (Random.Range(0, 2) % 2 == 1)
            {
#if ENABLE_LOG
                Debug.Log("Ad");
#endif
                manager.monetization.ShowInterstitialAd();

            }
        }
        MenuEvents.firstTime = false;
        GameObject transition_obj = Instantiate(transitionPrefab);
        transition_obj.GetComponent<Transition>().targetSceneName = menuSceneName;
    }

    public void RestartLevel()
    {
        GameObject transition_obj = Instantiate(transitionPrefab);
        transition_obj.GetComponent<Transition>().mode = Transition.Mode.Restart;
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
                PauseButton();
            }
        }
    }
}