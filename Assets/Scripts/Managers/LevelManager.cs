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
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion

    [SerializeField] private bool debug; private string debugTag = "LevelManager: ";

    [Header("Reference")]
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] private PlayerController playerController;

    enum LevelManagerState
    {
        SpawningEnemies,
        SignQuizEvent,
        Boss,
        GameOver
    }

    private LevelManagerState state;

    [Header("Score")]
    [SerializeField] public StartProgressionSystem starProgressionSystem;

    public void AddPointsToProgression(int points)
    {
        starProgressionSystem.AddScore(points);
    }

    [Header("Difficulty")]
    public float difficultyValue = 0f;
    [SerializeField] private float difficultyValueIncreasePerPostgameLevel;

    [Header("Spawning")]
    [SerializeField] private float floorWidth;
    [SerializeField] private float zLimit;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] public float objectsSpeed;
    [SerializeField] private float startObjectsSpeed, endObjectsSpeed;
    [SerializeField] private float postGameObjectsSpeedMultiplier;

    [Header("Enemy Spawning")]
    private float enemySpawnTimer;
    [SerializeField] private float enemySpawnCooldown;
    [SerializeField] private float startEnemySpawnCooldown, endEnemySpawnCooldown;
    [SerializeField] private float postGameEnemySpawnCooldownMultiplier;
    [SerializeField] private EnemyPool currentEnemyPool;

    private List<EnemyController> aliveEnemies = new List<EnemyController>();

    [Header("Upgrades")]
    [SerializeField] private SignQuizEventManager signQuizEventManager;


    //Upgrade event handling
    [SerializeField] private SignSelector signSelector;

    [Header("GameOver")]
    [SerializeField] private float gameOverCountdown;
    [SerializeField] private LevelManagerState stateBeforeGameOver;
    [SerializeField] private int reviveUses = 1;
    [SerializeField] private Button2D reviveButton;

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
        enemy_controller.difficultyValue = difficultyValue;
        enemy_controller.speed = objectsSpeed;
        enemy_controller.spawnPosition = spawnPosition;
        enemy_controller.floorWidth = floorWidth;
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

    public SignQuizEventManager GetSignQuizEventManager()
    {
        return signQuizEventManager;
    }
    
    public void GameOver()
    {
        //start game over screen countdown
        state = LevelManagerState.GameOver;

        if (reviveUses <= 0)
        {
            reviveButton.gameObject.SetActive(false);
        }

        //!!!change later - Restart scene
        //Debug.Log("Restarting scene");
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void Revive()
    {
        //start game over screen countdown
        state = stateBeforeGameOver;
        playerController.Revive();
        reviveUses--;

        pauseManager.DeactivateGameOverScreen();

        //!!!change later - Restart scene
        //Debug.Log("Restarting scene");
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnNewSubstar()
    {
        playerController.OnNewSubstar();
    }

    private void OnNewStar()
    {
        //Updates high score
        if (starProgressionSystem.numberOfStarsAchieved > GameManager.maxStarsAchieved)
        {
            GameManager.maxStarsAchieved = starProgressionSystem.numberOfStarsAchieved;
        }
        if (starProgressionSystem.postGameLevel > GameManager.maxPostGameScore)
        {
            GameManager.maxPostGameScore = starProgressionSystem.postGameLevel;
        }

        playerController.OnNewStar();
    }

    private SubStar lastSubStar;
    private int lastStarIndex;

    void Start()
    {
        if (debug) Debug.Log(debugTag + "Started");

        //Initialize enemy pool and difficulty
        UpdateDifficulty();

        //Initialize state
        state = LevelManagerState.SpawningEnemies;

        //Initialize Upgrade Event Manager
        signQuizEventManager.Initialize(this, signSelector, playerController, objectsSpeed, spawnPosition, zLimit, floorWidth);
    
        //Initialize Star Progression System
        //starProgressionSystem.Initialize(starIndex,subStarIndex);

        lastStarIndex = starProgressionSystem.currentStarIndex;
        lastSubStar = starProgressionSystem.currentSubstar;
    }

    void UpdateDifficulty()
    {
        difficultyValue = starProgressionSystem.totalStarProgression;
        if (starProgressionSystem.isInPostGame)
        {
            difficultyValue += starProgressionSystem.postGameLevel * difficultyValueIncreasePerPostgameLevel;
        }

        objectsSpeed = Mathf.LerpUnclamped(startObjectsSpeed, endObjectsSpeed, difficultyValue);
        enemySpawnCooldown = Mathf.LerpUnclamped(startEnemySpawnCooldown, endEnemySpawnCooldown, difficultyValue);
        
        currentEnemyPool = starProgressionSystem.currentSubstar.enemyPool;
    }

    void Update()
    {
        if (paused) return;

        switch (state)
        {
            case LevelManagerState.SpawningEnemies:
            {
                UpdateDifficulty();

                enemySpawnTimer -= Time.deltaTime;
                if (enemySpawnTimer <= 0)
                {
                    enemySpawnTimer = enemySpawnCooldown;
                    SpawnEnemy(currentEnemyPool.GetRandomEnemyPrefab(), floorWidth);
                }

                //When player reaches a substar
                if (starProgressionSystem.ReadyForReward())
                {
                    //if (debug) Debug.Log(debugTag + "Player reached a substar");

                    if (starProgressionSystem.currentReward is SignQuizReward)
                    {
                    
                        if (debug) Debug.Log("LevelManager: Starting upgrade event");

                        int num = signQuizEventManager.GetCurrentInfo().numOfSignOptions;
                        SignSelection signSelection = signSelector.SelectSigns(num);

                        signQuizEventManager.StartSignQuizEvent(signSelection.signs,
                                                SignSetManager.GetSoureSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                                SignSetManager.GetTargetSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                                signSelection.correctSignIndex,
                                                objectsSpeed,
                                                starProgressionSystem.currentReward as SignQuizReward);

                        if (debug) Debug.Log(debugTag + "Creating upgrade event manager");

                        state = LevelManagerState.SignQuizEvent;
                    }
                }
            }
            break;
            case LevelManagerState.SignQuizEvent:
            {
                if (signQuizEventManager.Finished())
                {
                    if (debug) Debug.Log(debugTag + "Upgrade event finished");

                    starProgressionSystem.ResolveReward(signQuizEventManager.AnsweredRight());
                    
                    //goes back to spawning enemies
                    state = LevelManagerState.SpawningEnemies;
                }
            }
            break;
            case LevelManagerState.Boss:
            {

            }
            break;
            case LevelManagerState.GameOver:
            {
                objectsSpeed *= Mathf.Pow(0.1f, Time.deltaTime);
                gameOverCountdown -= Time.deltaTime;
                if (gameOverCountdown <= 0)
                {
                    pauseManager.ActivateGameOverScreen();
                }
            }
            break;
        }

        //Trigger new star or substar event
        if (lastStarIndex != starProgressionSystem.currentStarIndex)
        {
            OnNewStar();
            lastStarIndex = starProgressionSystem.currentStarIndex;
        }
        else if (lastSubStar != starProgressionSystem.currentSubstar)
        {
            OnNewSubstar();
            lastSubStar = starProgressionSystem.currentSubstar;
        }
    }
}
