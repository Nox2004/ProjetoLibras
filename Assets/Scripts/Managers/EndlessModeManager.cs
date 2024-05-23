using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndlessModeLevelManager : LevelManager
{
    enum LevelManagerState
    {
        SpawningEnemies,
        SignQuizEvent,
        ChoosingUpgrade,
        Boss,
        GameOver
    }

    private LevelManagerState state;
    
    [Header("Endless Mode Star Progression System")]
    //[SerializeField] private Star starBase;
    public int score;
    public int initialStarPointsRequired;
    public int starPointsRequiredIncreasePerStar;

    public EnemyPool[] enemyPools;  

    protected override void OnSubstar()
    {
        base.OnSubstar();
    }
    protected override void OnStar()
    {
        base.OnStar();
        
        //Goes to the next star
        score++;
        foreach (SubStar substar in currentStar.subStars)
        {
            substar.enemyPool = enemyPools[Random.Range(0, enemyPools.Length)];
        }

        currentStar.pointsRequired += starPointsRequiredIncreasePerStar;
        objectsSpeed += objectsSpeedIncreasePerStar;
        enemySpawnCooldown -= enemySpawnCooldownDecreasePerStar;
        currentStarScore = 0;

        //Updates high score
        if (score > GameManager.GetGameMode(gameModeID).highScore)
        {
            GameManager.SetHighScore(gameModeID, score);
        }

        base.OnStar();
    }

    [Header("Difficulty")]
    public float difficultyValue = 0f, difficultyValueIncreasePerStar;
    [SerializeField] private float startObjectsSpeed, objectsSpeedIncreasePerStar;

    [Header("Enemy Spawning")]
    private float enemySpawnTimer;
    [SerializeField] private float enemySpawnCooldown;
    [SerializeField] private float startEnemySpawnCooldown, enemySpawnCooldownDecreasePerStar;

    [SerializeField] private EnemyPool currentEnemyPool;

    [Header("GameOver")]
    [SerializeField] private float gameOverCountdown;
    [SerializeField] private LevelManagerState stateBeforeGameOver;
    [SerializeField] private int reviveUses = 1;
    [SerializeField] private Button2D reviveButton;

    //Instantiate an enemy prefab
    protected override EnemyController SpawnEnemy(GameObject prefab, float xOffsetRange)
    {
        EnemyController enemy = base.SpawnEnemy(prefab, xOffsetRange);

        enemy.difficultyValue = difficultyValue;

        return enemy;
    }
    
    public override void GameOver()
    {
        base.GameOver();
        //start game over screen countdown
        state = LevelManagerState.GameOver;

        if (reviveUses <= 0)
        {
            reviveButton.gameObject.SetActive(false);
        }
    }

    public override void Revive()
    {
        //start game over screen countdown
        state = stateBeforeGameOver;
        playerController.Revive();
        reviveUses--;

        pauseManager.DeactivateGameOverScreen();
    }

    protected override void Start()
    {
        base.Start();
        
        if (debug) Debug.Log(debugTag + "Started");

        //Initialize state
        state = LevelManagerState.SpawningEnemies;

        foreach (SubStar substar in currentStar.subStars)
        {
            substar.enemyPool = enemyPools[Random.Range(0, enemyPools.Length)];
        }

        currentEnemyPool = currentSubstar.enemyPool;

        enemySpawnCooldown = startEnemySpawnCooldown;
        objectsSpeed = startObjectsSpeed;
        currentStar.pointsRequired = initialStarPointsRequired;
    }

    protected override void Update()
    {
        if (paused) return;

        switch (state)
        {
            case LevelManagerState.SpawningEnemies:
            {
                //update difficulty
                difficultyValue = score * difficultyValueIncreasePerStar;
                enemySpawnCooldown = startEnemySpawnCooldown + score * enemySpawnCooldownDecreasePerStar;
                objectsSpeed = startObjectsSpeed + score * objectsSpeedIncreasePerStar;
                currentEnemyPool = currentSubstar.enemyPool;

                enemySpawnTimer -= Time.deltaTime;
                if (enemySpawnTimer <= 0)
                {
                    enemySpawnTimer = enemySpawnCooldown;
                    SpawnEnemy(currentEnemyPool.GetRandomEnemyPrefab(), floorWidth);
                }

                //When player reaches a substar
                if (StarReadyForReward())
                {
                    if (currentReward is SignQuizReward)
                    {
                        if (debug) Debug.Log(debugTag + "Starting upgrade event");

                        int num = signQuizEventManager.GetCurrentInfo().numOfSignOptions;
                        SignSelection signSelection = signSelector.SelectSigns(num);

                        signQuizEventManager.StartSignQuizEvent(signSelection.signs,
                                                SignSetManager.GetSoureSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                                SignSetManager.GetTargetSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                                signSelection.correctSignIndex);

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
                    
                    SignQuizReward reward = currentReward as SignQuizReward;

                    if (reward is UpgradeReward)
                    {
                        if (signQuizEventManager.AnsweredRight())
                        {
                            StartUpgradeSelection(reward as UpgradeReward);
                        
                            //goes to upgrade selection state
                            state = LevelManagerState.ChoosingUpgrade;
                        }
                        else
                        {
                            //Goes back to spawning enemies
                            state = LevelManagerState.SpawningEnemies;
                        }
                    }
                    else if (reward is BossReward)
                    {

                    }
                    else 
                    {
                        //Goes back to spawning enemies
                        state = LevelManagerState.SpawningEnemies;
                    }
                    
                    ResolveReward(signQuizEventManager.AnsweredRight());
                }
            }
            break;
            case LevelManagerState.ChoosingUpgrade:
            {
                if (upgraded == true)
                {
                    upgraded = false;

                    //Goes back to spawning enemies
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

        base.Update();
    }
}
