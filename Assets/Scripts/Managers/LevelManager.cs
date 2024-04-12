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
    [SerializeField] private PlayerController playerController;

    [Header("Spawning")]
    [SerializeField] private float floorWidth;
    [SerializeField] private float zLimit;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private float objectsSpeed;

    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private float enemySpawnCooldownMin, enemySpawnCooldownMax;
    private IEnumerator enemySpawningCoroutine;

    [Header("Upgrades")]
    [SerializeField] private UpgradeEventManager upgradeEventManager;
    [SerializeField] private float upgradeCooldown;

    //Available signs and upgrade event handling
    private List<SignCode> availableSigns; 
    private float upgradeTimer = 0f;

    private bool upgradeEventOnGoing = false;

    //Instantiate an enemy prefab
    private void SpawnEnemy(GameObject prefab, Vector3 pos_offset)
    {
        if (debug) Debug.Log(debugTag + "Spawning enemy [" + prefab.name + "]");

        GameObject enemy = Instantiate(prefab, spawnPosition + pos_offset, Quaternion.identity);
        EnemyController enemy_controller = enemy.GetComponent<EnemyController>();
        enemy_controller.zLimit = zLimit;
        enemy_controller.speed = objectsSpeed;
    }
    
    //Initialize a new upgrade event
    private bool StartUpgradeEvent(SignCode[] signs, int correct_answer)
    {
        if (debug) Debug.Log(debugTag + "Populating QuestonSignController");
        
        GameObject[] answer_prefabs = new GameObject[signs.Length];
        for (int i = 0; i < signs.Length; i++)
        {
            answer_prefabs[i] = SignSetManager.GetTargetSign(signs[i]).signObjectPrefab;
        }

        upgradeEventManager.StartUpgradeEvent(  signs,
                                                SignSetManager.GetSoureSign(signs[correct_answer]).signTexture,
                                                SignSetManager.GetTargetSign(signs[correct_answer]).signTexture,
                                                answer_prefabs,
                                                correct_answer,
                                                objectsSpeed);

        if (debug) Debug.Log(debugTag + "Creating upgrade event manager");

        return true;
    }

    //Coroutine for spawning enemies
    private IEnumerator SpawnEnemyCoroutine()
    {
        while (true)
        {
            //!CHANGE LATER
            if (paused) yield return new WaitForSeconds(Random.Range(enemySpawnCooldownMin, enemySpawnCooldownMax));
            if (debug) Debug.Log(debugTag + "Trying to spawn enemy");

            if (upgradeEventOnGoing) 
            {
                if (debug) Debug.Log(debugTag + "Upgrade event on going, waiting to spawn enemy");
                yield return new WaitForSeconds(1); 
                continue;
            }

            SpawnEnemy(enemyPrefab, new Vector3(Random.Range(-floorWidth/2f, floorWidth/2f), 0, 0));

            yield return new WaitForSeconds(Random.Range(enemySpawnCooldownMin, enemySpawnCooldownMax));
        }
    }

    public UpgradeEventManager GetUpgradeEventManager()
    {
        return upgradeEventManager;
    }
    
    public void RestartLevel()
    {
        //!!!change later - Restart scene
        Debug.Log("Restarting scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        if (debug) Debug.Log(debugTag + "Started");

        enemySpawningCoroutine = SpawnEnemyCoroutine(); 
        StartCoroutine(enemySpawningCoroutine); //Start enemy spawning coroutine

        //copy sign codes to available signs
        availableSigns = new List<SignCode>(SignSetManager.signCodes);

        if (debug) 
        {
            string debug_str = debugTag + "Available signs - [";
            foreach (SignCode sign in availableSigns) debug_str += sign + ", ";
            debug_str += "]";

            Debug.Log(debug_str);
        }
        
        //Initialize Upgrade Event Manager
        upgradeEventManager.Initialize(objectsSpeed, spawnPosition, zLimit, floorWidth);
    }

    void Update()
    {
        if (paused) return;

        //Upgrade event cooldown
        if (!upgradeEventOnGoing) upgradeTimer += Time.deltaTime;

        //When upgrade event cooldown is over
        if (upgradeTimer > upgradeCooldown)
        {
            //If there is no upgrade event on going, start a new upgrade event
            if (!upgradeEventOnGoing)
            {
                if (debug) Debug.Log("LevelManager: Starting upgrade event");

                int num = upgradeEventManager.GetCurrentInfo().numOfOptions;

                //select 3 signs from sign code list
                SignCode[] selectedSigns = new SignCode[num];
                for (int i = 0; i < num; i++)
                {
                    int randomIndex = Random.Range(0, availableSigns.Count);

                    selectedSigns[i] = availableSigns[randomIndex];
                    availableSigns.RemoveAt(randomIndex);
                }

                //Select a correct answer
                int correct_answer = Random.Range(0, selectedSigns.Length);
                
                if (debug) 
                {
                    string debug_str = "LevelManager: Selected signs: [";
                    foreach (SignCode sign in selectedSigns) debug_str += sign + ", ";
                    debug_str += "]";
                    Debug.Log(debug_str);
                    Debug.Log("Correct answer: " + selectedSigns[correct_answer]);
                }

                upgradeEventOnGoing = StartUpgradeEvent(selectedSigns, correct_answer);
            }
            else //If there is an upgrade event on going, check if it is finished
            {
                if (upgradeEventManager.Finished())
                {
                    if (debug) Debug.Log(debugTag + "Upgrade event finished");

                    // enemySpawnCooldownMin*=0.85f; //!!!Change this later
                    // enemySpawnCooldownMax*=0.85f;
                    // objectsSpeed += 0.4f;
                    // score += upgradeEventManager.Finished(); //!!!Change this later
                    // scoreText.text = score.ToString(); //!!!Change this later

                    upgradeEventOnGoing = false;
                    upgradeTimer = 0;

                    if (availableSigns.Count < upgradeEventManager.GetCurrentInfo().numOfOptions) //if there are not enough available signs, refresh the list
                    {
                        //copy sign codes to available signs
                        availableSigns = new List<SignCode>(SignSetManager.signCodes);

                        if (debug) 
                        {
                            Debug.Log(debugTag + "Not enough available signs to create next update event, refreshing list");

                            string debug_str = debugTag + "Available signs - [";
                            foreach (SignCode sign in availableSigns) debug_str += sign + ", ";
                            debug_str += "]";

                            Debug.Log(debug_str);
                        }
                    }
                }
            }
        }
    }
}
