using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NormalModeLevelManager : LevelManager
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
    
    [Header("Normal Mode Star Progression System")]
    [SerializeField] protected Star[] stars;
    public int currentStarIndex;
    public int numberOfStarsAchieved { get { return Mathf.Min(currentStarIndex,stars.Length); } } //Total number of stars (excluding post game progress)

    protected override Star GetStar()
    {
        if (isInPostGame) 
        {
            return postGameStar;
        }
        return stars[currentStarIndex]; 
    }

    //Total score of the player (considering all the past stars)
    public int currentTotalScore { get { 
        int total = 0;

        if (isInPostGame) 
        {
            total = 0;
            for (int i = 0; i < stars.Length; i++)
            {
                total += stars[i].pointsRequired;
            }

            for (int i = stars.Length; i < currentStarIndex; i++)
            {
                total += postGameStar.pointsRequired;
            }
            return total + currentStarScore;
        }

        for (int i = 0; i < currentStarIndex; i++)
        {
            total += stars[i].pointsRequired;
        }
        return total+currentStarScore;
    } }

    //Progression up to reach the last star in a range from 0 to 1
    private float totalStarProgression { get {
        float maxScore = 0;
        for (int i = 0; i < stars.Length; i++)
        {
            maxScore += stars[i].pointsRequired;
        }
        return Mathf.Clamp01((float)currentTotalScore / maxScore);
    } }
    

    [SerializeField] private Star postGameStar;
    public bool isInPostGame { get { return currentStarIndex >= stars.Length; } }
    public int postGameLevel { get { return Mathf.Max(0,(currentStarIndex - stars.Length)+1); } } 

    public EnemyPool[] postGameEnemyPools;

    protected override void ResolveReward(bool rightAnswer)
    {
        //only advences to the next star if the player answered right
        if (isInPostGame && !rightAnswer) 
        {
            int _substar_length = currentStar.pointsRequired / currentStar.numOfSubstars;

            currentStarScore = currentSubStarIndex * _substar_length;
        
            return;
        }

        base.ResolveReward(rightAnswer);
    }    
    
    protected override void OnSubstar()
    {
        base.OnSubstar();
    }

    protected override void OnStar()
    {
        base.OnStar();

        //Goes to the next star
        currentStarIndex++;
        currentSubStarIndex = 0;
        currentStarScore = 0;

        if (isInPostGame)
        {
            foreach (SubStar sub in currentStar.subStars)
            {
                sub.enemyPool = postGameEnemyPools[UnityEngine.Random.Range(0, postGameEnemyPools.Length)];
            }
        }

        //Updates high score
        if (numberOfStarsAchieved + postGameLevel > GameManager.GetGameMode(gameModeID).highScore)
        {
            GameManager.SetHighScore(gameModeID, numberOfStarsAchieved + postGameLevel);

            if (numberOfStarsAchieved >= 3) //!!!!! change later
            {
                GameManager.UnlockGameMode(GameModeID.Endless);
            }
        }

        progressionBar.UpdateBar(currentStarScore);
        progressionBar.UpdateSubstars(currentStar, (isInPostGame) ? postGameLevel.ToString() : "");
    }

    [Header("Difficulty")]
    public float difficultyValue = 0f;
    [SerializeField] private float difficultyValueIncreasePerPostgameLevel;
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
    private LevelManagerState stateBeforeGameOver;
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

    private void UpdateDifficulty()
    {
        difficultyValue = totalStarProgression;
        if (isInPostGame)
        {
            difficultyValue += postGameLevel * difficultyValueIncreasePerPostgameLevel;
        }

        objectsSpeed = Mathf.LerpUnclamped(startObjectsSpeed, endObjectsSpeed, difficultyValue);
        objectsSpeed = Mathf.Min(objectsSpeed, objectsSpeedCap);
        enemySpawnDistance = Mathf.LerpUnclamped(startEnemySpawnDistance, endEnemySpawnDistance, difficultyValue);
        enemySpawnDistance = Mathf.Max(enemySpawnDistance, enemyDistanceCap);
        
        currentEnemyPool = currentSubstar.enemyPool;
    }

    protected override void Start()
    {
        base.Start();
        
        if (debug) Debug.Log(debugTag + "Started");

        //Initialize state
        state = LevelManagerState.SpawningEnemies;
    }

    protected override void Update()
    {
        if (paused) return;

        if (state != LevelManagerState.GameOver) 
        {
            UpdateDifficulty();
            stateBeforeGameOver = state;
        }

        switch (state)
        {
            case LevelManagerState.SpawningEnemies:
            {
                enemySpawnDistanceCount -= Time.deltaTime * objectsSpeed;
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
