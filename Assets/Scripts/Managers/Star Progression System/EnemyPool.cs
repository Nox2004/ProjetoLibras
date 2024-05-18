using UnityEngine;

[System.Serializable]
public class EnemyPool
{
    public EnemyInPool[] pool;
    private int _totalWeight = 0;
    private int totalWeight {
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

    public GameObject GetRandomEnemyPrefab()
    {
        int random = Random.Range(0, totalWeight);
        int count = 0;

        foreach (EnemyInPool enemy in pool)
        {
            count += enemy.chanceWeight;

            if (random <= count) return enemy.enemyPrefab;
        }

        Debug.LogError("Error in GetRandomEnemyPrefab: Random  - " + random + " Count - " + count);
        
        return null;
    }
}

[System.Serializable]
public class EnemyInPool
{
    public GameObject enemyPrefab;
    public int chanceWeight;
}