using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private float enemySpawnCooldownMin, enemySpawnCooldownMax;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemySpeed, xRange, zLimit;

    [SerializeField] private Vector3 spawnPosition;

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(4);

        while (true)
        {
            
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition + new Vector3(Random.Range(-xRange/2f, xRange/2f), 0, 0), Quaternion.identity);
            EnemyController enemy_controller = enemy.GetComponent<EnemyController>();
            enemy_controller.zLimit = zLimit;

            yield return new WaitForSeconds(Random.Range(enemySpawnCooldownMin, enemySpawnCooldownMax));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
