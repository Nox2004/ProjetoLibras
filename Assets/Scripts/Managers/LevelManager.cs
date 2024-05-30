using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour, IPausable
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

    [SerializeField] protected bool debug; protected string debugTag = "LevelManager: ";

    [Header("Reference")]
    [SerializeField] protected PauseManager pauseManager;
    [SerializeField] protected ParticleManager particleManager;
    [SerializeField] protected PlayerController playerController;
    [SerializeField] protected StarProgressionBar progressionBar;
    public PlayerController PlayerController { get => playerController; }
    [HideInInspector] public BossController currentBoss;

    #region //Star Progression System

    [Header("Star Progression System")]
    [SerializeField] public GameModeID gameModeID;

    //Current star
    [SerializeField] protected Star _currentStar;
    public Star currentStar { get => GetStar(); }

    protected virtual Star GetStar()
    {
        return _currentStar;
    }

    //The substar player is currently trying to reach
    public SubStar currentSubstar { get { return currentStar.subStars[currentSubStarIndex]; } }
    public int currentSubStarIndex; 

    //Star progression system info
    public int currentNumOfSubstars { get { return currentStar.numOfSubstars; } }
    public int currentStarScore = 0;
    public StarReward currentReward { get { return currentSubstar.reward; } }
    protected bool reward;

    public virtual bool StarReadyForReward() { return reward; }
    
    //Progression up to reach the current star in a range from 0 to 1
    public float currentStarProgression { get { return Mathf.Clamp01((float) currentStarScore / currentStar.pointsRequired); } }

    public virtual void AddStarScore(int score)
    {
        if (reward) return;
       
        int _substar = currentStar.pointsRequired / currentStar.numOfSubstars;
        int _max = (currentSubStarIndex+1) * _substar;

        currentStarScore += score;

        //reached a new substar
        if (currentStarScore >= _max)
        {
            //caps score and sets reward to true
            currentStarScore = _max;
            reward = true;
        }

        progressionBar.UpdateBar(currentStarProgression);
    }

    protected virtual void ResolveReward(bool rightAnswer)
    {
        //sets reward to false
        reward = false;

        //reached the end of the star
        if (currentSubStarIndex >= currentStar.numOfSubstars-1) 
        {
            OnStar();
        }
        else
        {
            //Gets to the next substar
            OnSubstar();
            currentSubStarIndex++;
        }
    }

    protected virtual void OnSubstar()
    {
        playerController.OnSubstar();
    }

    protected virtual void OnStar()
    {
        playerController.OnStar();

        progressionBar.UpdateSubstars(currentStar);
        progressionBar.UpdateBar(currentStarProgression);
    }

    #endregion

    [Header("Spawning")]
    [SerializeField] protected float floorWidth;
    [SerializeField] protected float zLimit;
    [SerializeField] protected Vector3 spawnPosition;
    [SerializeField] public float objectsSpeed;

    protected List<EnemyController> aliveEnemies = new List<EnemyController>();

    [Header("Upgrades")]
    [SerializeField] protected SignQuizEventManager signQuizEventManager;

    [SerializeField] private UpgradeSelection upgradeSelection;
    [SerializeField] private Panel upgradeSelectionPanel;

    private PlayerUpgrade[] currentUpgradeSelection;
    private PlayerUpgradeId selectedUpgrade;

    protected bool upgraded = false;

    //Upgrade event handling
    [SerializeField] protected SignSelector signSelector;

    //Instantiate an enemy prefab
    protected virtual EnemyController SpawnEnemy(GameObject prefab, float xOffsetRange)
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
        enemy_controller.difficultyValue = 0f;

        return enemy_controller;
    }

    protected virtual BossController SpawnBoss(GameObject prefab)
    {
        if (debug) Debug.Log(debugTag + "Spawning boss [" + prefab.name + "]");

        GameObject boss = Instantiate(prefab, spawnPosition, Quaternion.identity * Quaternion.Euler(0, 180, 0));
        BossController boss_controller = boss.GetComponent<BossController>();
        currentBoss = boss_controller;

        boss_controller.levelManager = this;
        boss_controller.particleManager = particleManager;
        boss_controller.zLimit = zLimit;
        boss_controller.speed = objectsSpeed;
        boss_controller.spawnPosition = spawnPosition;
        boss_controller.floorWidth = floorWidth;
        boss_controller.difficultyValue = 0f;

        return boss_controller;
    }

    public virtual void InsertEnemy(EnemyController enemy)
    {
        aliveEnemies.Add(enemy);
    }

    public virtual void RemoveEnemy(EnemyController enemy)
    {
        aliveEnemies.Remove(enemy);
    }
    
    public virtual List<EnemyController> GetAliveEnemies()
    {
        return aliveEnemies;
    }

    public virtual SignQuizEventManager GetSignQuizEventManager()
    {
        return signQuizEventManager;
    }
    
    public virtual void GameOver()
    {

    }

    public virtual void Revive()
    {

    }

    protected void StartUpgradeSelection(UpgradeReward upgradeReward)
    {
        //Rewards player with a upgrade
        currentUpgradeSelection = playerController.upgradeManager.UpgradeSelection(upgradeReward.numberOfUpgradeOptions, upgradeReward.upgradeTier);
        selectedUpgrade = PlayerUpgradeId.Count;

        upgradeSelectionPanel.SetActive(true);
        upgradeSelection.SetButtons(currentUpgradeSelection);
        upgradeSelection.levelManager = this;
    }
    
    public void SelectUpgrade(PlayerUpgradeId status)
    {
        selectedUpgrade = status;

        playerController.Upgrade(selectedUpgrade);
        upgradeSelectionPanel.SetActive(false);

        selectedUpgrade = PlayerUpgradeId.Count;

        upgraded = true;
    }

    protected virtual void Start()
    {
        if (debug) Debug.Log(debugTag + "Started");

        //Initialize Upgrade Event Manager
        signQuizEventManager.Initialize(this, signSelector, playerController, spawnPosition, zLimit, floorWidth);

        progressionBar.UpdateBar(currentStarProgression);
        progressionBar.UpdateSubstars(currentStar);
    }

    protected virtual void Update()
    {
        if (paused) return;
    }
}
