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
    public float starPointsRequiredMultiplier;

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
        currentStarScore = 0;

        //Updates difficulty
        currentStar.pointsRequired = (int) (currentStar.pointsRequired * starPointsRequiredMultiplier);
        difficultyValue += difficultyValueIncreasePerStar;

        UpdateDifficulty();

        //Updates high score
        if (score > GameManager.GetGameMode(gameModeID).highScore)
        {
            GameManager.SetHighScore(gameModeID, score);
        }

        progressionBar.UpdateBar(currentStarScore);
        progressionBar.UpdateSubstars(currentStar,thisStarHasABoss() ? "" : (score+1).ToString());
    }

    [Header("Difficulty")]
    [SerializeField] private StarReward normalReward;
    [SerializeField] private StarReward bossReward;
    [SerializeField] private int bossStarInterval;

    public float difficultyValue = 0f, difficultyValueIncreasePerStar;

    [SerializeField] private float startObjectsSpeed, endObjectsSpeed;
    [SerializeField] private float objectsSpeedCap;

    //Enemy spawning
    private float enemySpawnDistanceCount;
    private float enemySpawnDistance;
    [SerializeField] private float startEnemySpawnDistance, endEnemySpawnDistance;
    [SerializeField] private float enemyDistanceCap;

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

        currentStar.pointsRequired = initialStarPointsRequired;
        UpdateDifficulty();

        progressionBar.UpdateBar(currentStarScore);
        progressionBar.UpdateSubstars(currentStar,(score+1).ToString());
    }

    protected bool thisStarHasABoss()
    {
        return (score+1) % bossStarInterval == 0 && (score+1) != 0;
    }

    protected void UpdateDifficulty()
    {
        //new enemy pool
        foreach (SubStar substar in currentStar.subStars)
        {
            substar.enemyPool = enemyPools[Random.Range(0, enemyPools.Length)];
        }

        //change reward if boss
        if (thisStarHasABoss())
        {
            currentStar.subStars[currentStar.subStars.Length - 1].reward = bossReward;
        }
        else
        {
            currentStar.subStars[currentStar.subStars.Length - 1].reward = normalReward;
        }

        //update speed and spawning
        objectsSpeed = Mathf.Lerp(startObjectsSpeed, endObjectsSpeed, difficultyValue);
        objectsSpeed = Mathf.Min(objectsSpeed, objectsSpeedCap);
        
        enemySpawnDistance = Mathf.Lerp(startEnemySpawnDistance, endEnemySpawnDistance, difficultyValue);
        enemySpawnDistance = Mathf.Max(enemySpawnDistance, enemyDistanceCap);
    }

    protected override void Update()
    {
        if (paused) return;

        if (state != LevelManagerState.GameOver)
        {
            stateBeforeGameOver = state;
        }

        switch (state)
        {
            case LevelManagerState.SpawningEnemies:
            {
                //update enemy pool
                currentEnemyPool = currentSubstar.enemyPool;

                enemySpawnDistanceCount -= objectsSpeed * Time.deltaTime;
                if (enemySpawnDistanceCount <= 0)
                {
                    enemySpawnDistanceCount = enemySpawnDistance;
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
                    else if (currentReward is BossReward)
                    {
                        if (debug) Debug.Log(debugTag + "Starting boss event");

                        BossReward bossReward = currentReward as BossReward;
                        currentBoss = SpawnBoss(bossReward.bossPrefab);
                        currentBoss.difficultyValue = difficultyValue;
                        
                        //Start boss event
                        state = LevelManagerState.Boss;
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
                if (!currentBoss.alive)
                {
                    currentBoss = null;

                    BossReward bossReward = currentReward as BossReward;
                    StartUpgradeSelection(bossReward.upgradeReward);

                    ResolveReward(true);
                    
                    //goes to upgrade selection state
                    state = LevelManagerState.ChoosingUpgrade;
                }
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
