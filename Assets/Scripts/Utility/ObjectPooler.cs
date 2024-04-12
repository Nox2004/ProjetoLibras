using System.Collections;
using UnityEngine;

public class ObjectPooler
{
    private GameObject[] pool;
    private int poolSize;
    private float poolSizeExpandFactor;
    public GameObject prefab { get; private set;}
    private Transform parent;

    public ObjectPooler(GameObject prefab, Transform parent = null, int poolSize = 10, float poolSizeExpandFactor = 2)
    {
        this.prefab = prefab;
        this.poolSize = poolSize;
        this.parent = parent;
        this.poolSizeExpandFactor = poolSizeExpandFactor;
        pool = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = GameObject.Instantiate(prefab, parent);

            ObjectFromPool obj = pool[i].GetComponent<ObjectFromPool>();
            if (obj != null) obj.pooler = this;

            pool[i].SetActive(false);
        }
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetObject();

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        return obj;
    }

    public GameObject GetObject()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                pool[i].SetActive(true);
                return pool[i];
            }
        }

        int oldSize = poolSize;
        poolSize = (int) (poolSize * poolSizeExpandFactor);
        GameObject[] newPool = new GameObject[poolSize];

        for (int i = 0; i < oldSize; i++)
        {
            newPool[i] = pool[i];
        }

        for (int i = oldSize; i < poolSize; i++)
        {
            newPool[i] = GameObject.Instantiate(prefab, parent);

            ObjectFromPool obj = newPool[i].GetComponent<ObjectFromPool>();
            if (obj != null) obj.pooler = this;

            newPool[i].SetActive(false);
        }

        pool = newPool;

        GameObject output = pool[oldSize];
        output.SetActive(true);
        return output;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}