using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private bool debug; private string debugTag = "LevelManager: ";
    [SerializeField] private SignSetManager signManager;

    [Header("Spawning")]
    [SerializeField] private float floorWidth;
    [SerializeField] private float zLimit;
    [SerializeField] private Vector3 spawnPosition;

    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private float enemySpawnCooldownMin, enemySpawnCooldownMax;
    [SerializeField] private float enemySpeedMultiplier;

    [Header("Upgrades")]
    [SerializeField] private float upgradeCooldown;
    [SerializeField] private GameObject upgradeQuestionObject;
    [SerializeField] private float signObjectsSpeed;
    [SerializeField] private int numberOfOptionsPerUpgrade;

    private List<SignCode> availableSigns; 
    private float upgradeTimer = 0f;

    private UpgradeEventManager upgradeEventOnGoing = null;

    //Instantiate an enemy prefab
    private void SpawnEnemy(GameObject prefab, Vector3 pos_offset)
    {
        if (debug) Debug.Log(debugTag + "Spawning enemy [" + prefab.name + "]");

        GameObject enemy = Instantiate(prefab, spawnPosition + pos_offset, Quaternion.identity);
        EnemyController enemy_controller = enemy.GetComponent<EnemyController>();
        enemy_controller.zLimit = zLimit;
    }

    //Initialize a new upgrade event
    private UpgradeEventManager StartUpgradeEvent(SignCode[] signs, int correct_answer)
    {
        if (debug) Debug.Log(debugTag + "Creating upgrade event manager");

        //Instantiate upgrade event
        UpgradeEventManager uem = this.AddComponent<UpgradeEventManager>();
        
        //Populate upgrade event manager pro
        uem.debug = debug;
        uem.spawnPosition = spawnPosition;
        uem.speed = signObjectsSpeed;
        uem.correctAnswerIndex = correct_answer;
        uem.questionObject = upgradeQuestionObject;
        uem.questionSprite = signManager.GetSoureSign(signs[correct_answer]).signSprite;
        
        uem.answerPrefabs = new GameObject[signs.Length];
        for (int i = 0; i < signs.Length; i++)
        {
            uem.answerPrefabs[i] = signManager.GetTargetSign(signs[i]).signObjectPrefab;
        }

        return uem;
    }

    //Coroutine for spawning enemies
    private IEnumerator SpawnEnemyCoroutine()
    {
        yield return new WaitForSeconds(4);

        while (true)
        {
            if (debug) Debug.Log(debugTag + "Trying to spawn enemy");

            if (upgradeEventOnGoing != null) 
            {
                if (debug) Debug.Log(debugTag + "Upgrade event on going, waiting to spawn enemy");
                yield return new WaitForSeconds(1); 
                continue;
            }

            SpawnEnemy(enemyPrefab, new Vector3(Random.Range(-floorWidth/2f, floorWidth/2f), 0, 0));

            yield return new WaitForSeconds(Random.Range(enemySpawnCooldownMin, enemySpawnCooldownMax));
        }
    }
    
    void Start()
    {
        if (debug) Debug.Log(debugTag + "Started");

        StartCoroutine(SpawnEnemyCoroutine()); //Start enemy spawning coroutine

        //copy sign codes to available signs
        availableSigns = new List<SignCode>(signManager.signCodes);

        if (debug) 
        {
            string debug_str = debugTag + "Available signs - [";
            foreach (SignCode sign in availableSigns) debug_str += sign + ", ";
            debug_str += "]";

            Debug.Log(debug_str);
        }
    }

    void Update()
    {
        //Upgrade event cooldown
        if (upgradeEventOnGoing == null) upgradeTimer += Time.deltaTime;

        //When upgrade event cooldown is over
        if (upgradeTimer > upgradeCooldown)
        {
            //If there is no upgrade event on going, start a new upgrade event
            if (upgradeEventOnGoing == null)
            {
                if (debug) Debug.Log("LevelManager: Starting upgrade event");

                //select 3 signs from sign code list
                SignCode[] selectedSigns = new SignCode[numberOfOptionsPerUpgrade];
                for (int i = 0; i < numberOfOptionsPerUpgrade; i++)
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
                if (upgradeEventOnGoing.Finished())
                {
                    if (debug) Debug.Log(debugTag + "Upgrade event finished, destroying it");
                    
                    Destroy(upgradeEventOnGoing);
                    upgradeEventOnGoing = null;
                    upgradeTimer = 0;

                    if (availableSigns.Count < numberOfOptionsPerUpgrade) //if there are not enough available signs, refresh the list
                    {
                        //copy sign codes to available signs
                        availableSigns = new List<SignCode>(signManager.signCodes);

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
