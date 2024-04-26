using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour, IPausable
{  
    #region //IPausable implementation

    private bool paused = false;

    public void Pause()
    {
        paused = true;
        StopCoroutine(enemySpawningCoroutine);
    }

    public void Resume()
    {
        paused = false;
        StartCoroutine(enemySpawningCoroutine);
    }

    #endregion

    [SerializeField] private bool debug; private string debugTag = "LevelManager: ";

    [Header("Reference")]
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] private PlayerController playerController;

    [Header("Score")]
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverCurrentScoreText;
    private int _currentScore = 0;
    [HideInInspector] public int currentScore
    {
        get { return _currentScore; }
        set
        {
            _currentScore = value;
            scoreText.text = _currentScore.ToString();
        }
    }

    [Header("Spawning")]
    [SerializeField] private float floorWidth;
    [SerializeField] private float zLimit;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] public float objectsSpeed;
    [SerializeField] private float objectsSpeedIncreasePerEvent;
    [SerializeField] private float maxObjectsSpeed;

    [Header("Enemy Spawning")]
    [SerializeField] private float enemySpawnCooldown;
    [SerializeField] private float enemySpawnCooldownDecreasePerEvent;
    [SerializeField] private float minEnemySpawnCooldown;
    [SerializeField] private EnemyPoolUpdate[] enemyPoolUpdates;
    private GameObject[] currentEnemyPrefabPool;

    [System.Serializable]
    private struct EnemyPoolUpdate
    {
        public int numberOfUpgradeEvents;
        public GameObject[] enemyPrefabs;
    }

    private IEnumerator enemySpawningCoroutine;
    private List<EnemyController> aliveEnemies = new List<EnemyController>();

    [Header("Upgrades")]
    [SerializeField] private UpgradeEventManager upgradeEventManager;
    [SerializeField] private float upgradeCooldown;
    private int numberOfUpgradeEvents = 0;

    [SerializeField] private int numberOfEventsToStopRewardingUpgrades;


    //Available signs and upgrade event handling
    private float upgradeTimer = 0f;
    [SerializeField] private SignSelector signSelector;

    private bool upgradeEventOnGoing = false;

    //Instantiate an enemy prefab
    private void SpawnEnemy(GameObject prefab, float xOffsetRange)
    {
        if (debug) Debug.Log(debugTag + "Spawning enemy [" + prefab.name + "]");

        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
        EnemyController enemy_controller = enemy.GetComponent<EnemyController>();
        InsertEnemy(enemy_controller);

        enemy.transform.position += Vector3.right * Random.Range(-xOffsetRange/2f+enemy_controller.width/2, xOffsetRange/2f-enemy_controller.width/2);
        
        enemy_controller.levelManager = this;
        enemy_controller.particleManager = particleManager;
        enemy_controller.zLimit = zLimit;
        enemy_controller.speed = objectsSpeed;
        enemy_controller.spawnPosition = spawnPosition;
        enemy_controller.floorWidth = floorWidth;
    }

    private void UpdateEnemyPool(int num_of_events)
    {
        if (debug) Debug.Log(debugTag + "Updating enemy pool");

        for (int i = 0; i < enemyPoolUpdates.Length; i++)
        {
            if (num_of_events == enemyPoolUpdates[i].numberOfUpgradeEvents)
            {
                currentEnemyPrefabPool = enemyPoolUpdates[i].enemyPrefabs;
                break;
            }
        }
    }

    public void InsertEnemy(EnemyController enemy)
    {
        aliveEnemies.Add(enemy);
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        aliveEnemies.Remove(enemy);
    }
    
    public List<EnemyController> GetAliveEnemies()
    {
        return aliveEnemies;
    }
    
    //Initialize a new upgrade event
    private bool StartUpgradeEvent(SignCode[] signs, int correct_answer)
    {
        if (debug) Debug.Log(debugTag + "Populating QuestonSignController");

        upgradeEventManager.StartUpgradeEvent(  signs,
                                                SignSetManager.GetSoureSign(signs[correct_answer]).signTexture,
                                                SignSetManager.GetTargetSign(signs[correct_answer]).signTexture,
                                                correct_answer,
                                                objectsSpeed,
                                                numberOfUpgradeEvents < numberOfEventsToStopRewardingUpgrades);

        if (debug) Debug.Log(debugTag + "Creating upgrade event manager");

        return true;
    }

    //Coroutine for spawning enemies
    private IEnumerator SpawnEnemyCoroutine()
    {
        while (true)
        {
            if (debug) Debug.Log(debugTag + "Trying to spawn enemy");

            if (upgradeEventOnGoing) 
            {
                if (debug) Debug.Log(debugTag + "Upgrade event on going, waiting to spawn enemy");
                yield return new WaitForSeconds(1); 
                continue;
            }

            SpawnEnemy(currentEnemyPrefabPool[Random.Range(0,currentEnemyPrefabPool.Length)], floorWidth);

            yield return new WaitForSeconds(enemySpawnCooldown);
        }
    }

    public UpgradeEventManager GetUpgradeEventManager()
    {
        return upgradeEventManager;
    }
    
    public void GameOver()
    {
        if (currentScore > GameManager.highScore)
        {
            GameManager.highScore = currentScore;
        }

        gameOverHighScoreText.text = "high score: " + GameManager.highScore.ToString();
        gameOverCurrentScoreText.text = currentScore.ToString();

        pauseManager.ActivateGameOverScreen();
        //!!!change later - Restart scene
        //Debug.Log("Restarting scene");
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        if (debug) Debug.Log(debugTag + "Started");

        currentScore = 0;

        //Initialize enemy pool
        UpdateEnemyPool(0);

        enemySpawningCoroutine = SpawnEnemyCoroutine(); 
        StartCoroutine(enemySpawningCoroutine); //Start enemy spawning coroutine
        
        //Initialize Upgrade Event Manager
        upgradeEventManager.Initialize(this, signSelector, playerController, objectsSpeed, spawnPosition, zLimit, floorWidth);
    }

    void Update()
    {
        if (paused) return;

        //Upgrade event cooldown
        if (!upgradeEventOnGoing) 
        {
            upgradeTimer += Time.deltaTime;
            enemySpawnCooldown -= (enemySpawnCooldownDecreasePerEvent / upgradeCooldown) * Time.deltaTime;
            enemySpawnCooldown = Mathf.Max(enemySpawnCooldown, minEnemySpawnCooldown);
            objectsSpeed += (objectsSpeedIncreasePerEvent / upgradeCooldown) * Time.deltaTime;
            objectsSpeed = Mathf.Min(objectsSpeed, maxObjectsSpeed);
        }

        //When upgrade event cooldown is over
        if (upgradeTimer > upgradeCooldown)
        {
            //If there is no upgrade event on going, start a new upgrade event
            if (!upgradeEventOnGoing)
            {
                if (debug) Debug.Log("LevelManager: Starting upgrade event");

                int num = upgradeEventManager.GetCurrentInfo().numOfSignOptions;
                SignSelection signSelection = signSelector.SelectSigns(num);

                upgradeEventOnGoing = StartUpgradeEvent(signSelection.signs, signSelection.correctSignIndex);
            }
            else //If there is an upgrade event on going, check if it is finished
            {
                if (upgradeEventManager.Finished())
                {
                    if (debug) Debug.Log(debugTag + "Upgrade event finished");

                    upgradeEventOnGoing = false;
                    upgradeTimer = 0;

                    numberOfUpgradeEvents++;
                    UpdateEnemyPool(numberOfUpgradeEvents);
                }
            }
        }
    }
}
