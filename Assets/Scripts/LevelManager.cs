using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
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

    private List<SignCode> availableSigns; 
    private float upgradeTimer = 0f;

    private UpgradeEventManager upgradeEventOnGoing = null;

    //Instantiate an enemy prefab
    private void SpawnEnemy(GameObject prefab, Vector3 pos_offset)
    {
        GameObject enemy = Instantiate(prefab, spawnPosition + pos_offset, Quaternion.identity);
        EnemyController enemy_controller = enemy.GetComponent<EnemyController>();
        enemy_controller.zLimit = zLimit;
    }

    //Initialize a new upgrade event
    private UpgradeEventManager StartUpgradeEvent(SignCode[] signs, int correct_answer)
    {
        //Instantiate upgrade event
        UpgradeEventManager uem = this.AddComponent<UpgradeEventManager>();

        //Populate upgrade event manager pro
        uem.speed = signObjectsSpeed;
        uem.correctAnswerIndex = correct_answer;
        uem.questionObject = upgradeQuestionObject;
        uem.questionSprite = signManager.GetSoureSign(signs[correct_answer]).signSprite;
        
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
            if (upgradeEventOnGoing != null) yield return new WaitForSeconds(1);

            SpawnEnemy(enemyPrefab, new Vector3(Random.Range(-floorWidth/2f, floorWidth/2f), 0, 0));

            yield return new WaitForSeconds(Random.Range(enemySpawnCooldownMin, enemySpawnCooldownMax));
        }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemyCoroutine()); //Start enemy spawning coroutine

        //copy sign codes to available signs
        availableSigns = new List<SignCode>(signManager.signCodes);
    }

    // Update is called once per frame
    void Update()
    {
        //Upgrade event cooldown
        if (upgradeEventOnGoing == null) upgradeTimer += Time.deltaTime;

        //When upgrade event cooldown is over
        if (upgradeTimer > upgradeCooldown)
        {
            //If there is no upgrade event on going, start a new upgrade event
            if (!upgradeEventOnGoing)
            {
                //select 3 signs from sign code list
                SignCode[] selectedSigns = new SignCode[3];
                for (int i = 0; i < 3; i++)
                {
                    int randomIndex = Random.Range(0, availableSigns.Count);

                    selectedSigns[i] = availableSigns[randomIndex];
                    availableSigns.RemoveAt(randomIndex);
                }

                //Select a correct answer
                int correctAnswer = Random.Range(0, selectedSigns.Length);
                
                upgradeEventOnGoing = StartUpgradeEvent(selectedSigns, correctAnswer);
            }
            else //If there is an upgrade event on going, check if it is finished
            {
                if (upgradeEventOnGoing.Finished())
                {
                    Destroy(upgradeEventOnGoing);
                    upgradeEventOnGoing = null;
                    upgradeTimer = 0;
                }
            }
            
            //upgradeTimer = 0;
            
        }
    }
}
