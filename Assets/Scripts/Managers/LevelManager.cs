using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class StartProgressionSystem
{
    [SerializeField] private Star[] stars;
    private bool reward = false; //Is set to true when player reaches a substar

    public int currentStarIndex; //Index of the star player is currently trying to reach
    public int currentSubStarIndex; //Index of the star player is currently trying to reach

    public int currentStarScore = 0; //Score used to reach the current star


    //Total score of the player (considering all the past stars)
    public int currentTotalScore { get { 
        int total = 0;
        for (int i = 0; i < currentStarIndex; i++)
        {
            total += stars[i].pointsRequired;
        }
        return total+currentStarScore;
    } }
    
    public Star currentStar { get { return stars[currentStarIndex]; } }
    public SubStar currentSubstar { get { return currentStar.subStars[currentSubStarIndex]; } }
    public float currentNumOfSubstars { get { return currentStar.numOfSubstars; } }
    
    //Progression up to reach the current star in a range from 0 to 1
    public float currentStarProgression { get { return (float) currentStarScore / currentStar.pointsRequired; } }
    
    public RewardType currentRewardType { get { return currentSubstar.reward.rewardType; } }

    //Progression up to reach the last star in a range from 0 to 1
    public float totalStarProgression { get {
        float maxScore = 0;
        for (int i = 0; i < stars.Length; i++)
        {
            maxScore += stars[i].pointsRequired;
        }
        return currentTotalScore / maxScore;
    } }

    public bool ReadyForReward() { return reward; }

    public void AddScore(int score)
    {
        if (reward) return;
       
        int _substar = currentStar.pointsRequired / currentStar.numOfSubstars;
        int _max = (currentSubStarIndex+1) * _substar;

        currentStarScore += score;

        //reached a new substar
        if (currentStarScore > _max)
        {
            //caps score and sets reward to true
            currentStarScore = _max;
            reward = true;
        }
    }

    public void ResolveReward()
    {
        //gets to the next substar
        currentSubStarIndex++;

        //reached next star
        if (currentSubStarIndex >= currentStar.numOfSubstars) 
        {
            currentStarIndex++;
            currentSubStarIndex = 0;
        }

        reward = false;
    }
}

[System.Serializable]
public class Star
{
    public int pointsRequired;
    public SubStar[] subStars;
    public int numOfSubstars { get { return subStars.Length; } }
}

[System.Serializable]
public class SubStar
{
    public EnemyPool enemyPool;
    public Reward reward;
}

public enum RewardType
{
    UpgradeEvent,
    FreeUpgreade,
    BossFight,
    Points
}

[System.Serializable]
public class Reward
{
    public RewardType rewardType;
}


[System.Serializable]
public class EnemyPool
{
    public EnemyInPool[] pool;
    private float _totalWeight = 0;
    private float totalWeight {
        get
        {
            if (_totalWeight == 0)
            {
                foreach (EnemyInPool enemy in pool)
                {
                    _totalWeight += enemy.chanceWeight;
                }
            }
            return _totalWeight;
        }
    }

    public GameObject GetRandomEnemy()
    {
        float random = Random.Range(0, totalWeight);
        float count = 0;

        foreach (EnemyInPool enemy in pool)
        {
            count += enemy.chanceWeight;

            if (random <= count) return enemy.enemyPrefab;
        }

        return null;
    }
}

[System.Serializable]
public class EnemyInPool
{
    public GameObject enemyPrefab;
    public float chanceWeight;
}

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
    [SerializeField] public StartProgressionSystem starProgressionSystem;

    public void AddPointsToProgression(int points)
    {
        starProgressionSystem.AddScore(points);
    }

    [Header("Spawning")]
    [SerializeField] private float floorWidth;
    [SerializeField] private float zLimit;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] public float objectsSpeed;
    [SerializeField] private float startObjectsSpeed, endObjectsSpeed;
    [SerializeField] private float maxObjectsSpeed;

    [Header("Enemy Spawning")]
    [SerializeField] private float enemySpawnCooldown;
    [SerializeField] private float startEnemySpawnCooldown, endEnemySpawnCooldown;
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
        pauseManager.ActivateGameOverScreen();
        //!!!change later - Restart scene
        //Debug.Log("Restarting scene");
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        if (debug) Debug.Log(debugTag + "Started");

        //Initialize enemy pool and difficulty
        UpdateDifficulty();
        UpdateEnemyPool(0);

        enemySpawningCoroutine = SpawnEnemyCoroutine(); 
        StartCoroutine(enemySpawningCoroutine); //Start enemy spawning coroutine
        
        //Initialize Upgrade Event Manager
        upgradeEventManager.Initialize(this, signSelector, playerController, objectsSpeed, spawnPosition, zLimit, floorWidth);
    }

    void UpdateDifficulty()
    {
        objectsSpeed = Mathf.Lerp(startObjectsSpeed, endObjectsSpeed, starProgressionSystem.totalStarProgression);
        enemySpawnCooldown = Mathf.Lerp(startEnemySpawnCooldown, endEnemySpawnCooldown, starProgressionSystem.totalStarProgression);
    }

    void Update()
    {
        if (paused) return;

        //Upgrade event cooldown
        if (!upgradeEventOnGoing) 
        {
            UpdateDifficulty();
        }

        //When player reaches a substar
        if (starProgressionSystem.ReadyForReward())
        {
            if (debug) Debug.Log(debugTag + "Player reached a substar");

            switch (starProgressionSystem.currentRewardType)
            {
                case RewardType.UpgradeEvent:
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

                            starProgressionSystem.ResolveReward();
                            //UpdateEnemyPool(numberOfUpgradeEvents);
                        }
                    }
                }
                break;
            }
        }
    }
}
