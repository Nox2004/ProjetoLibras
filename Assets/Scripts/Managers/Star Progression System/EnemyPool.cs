using UnityEngine;

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