using System.Collections;
using UnityEngine;

public class ComponentPooler<T> where T : Behaviour
{
    protected T[] pool;
    protected int poolSize;
    protected float poolSizeExpandFactor;
    protected GameObject targetObject;

    public ComponentPooler(GameObject targetObject, int poolSize = 1, float poolSizeExpandFactor = 2)
    {
        this.targetObject = targetObject;
        this.poolSize = poolSize;
        this.poolSizeExpandFactor = poolSizeExpandFactor;
        pool = new T[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = targetObject.AddComponent<T>();
            pool[i].enabled = false;
        }
    }

    protected void CheckInUse()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!IsActive(pool[i]))
            {
                ReturnComponent(pool[i]);
            }
        }
    }

    virtual protected bool IsActive(T component)
    {
        return component.enabled;
    }

    public T GetComponent()
    {
        CheckInUse();

        for (int i = 0; i < poolSize; i++)
        {
            if (!pool[i].enabled)
            {
                pool[i].enabled = true;
                return pool[i];
            }
        }

        int oldSize = poolSize;
        poolSize = (int) (poolSize * poolSizeExpandFactor);
        T[] newPool = new T[poolSize];

        for (int i = 0; i < oldSize; i++)
        {
            newPool[i] = pool[i];
        }

        for (int i = oldSize; i < poolSize; i++)
        {
            newPool[i] = targetObject.AddComponent<T>();

            newPool[i].enabled = false;
        }

        pool = newPool;

        T output = pool[oldSize];
        output.enabled = true;
        
        return output;
    }

    public T[] GetActiveComponents()
    {
        CheckInUse();

        int activeCount = 0;
        for (int i = 0; i < poolSize; i++)
        {
            if (IsActive(pool[i]))
            {
                activeCount++;
            }
        }

        T[] activeComponents = new T[activeCount];
        int j = 0;
        for (int i = 0; i < poolSize; i++)
        {
            if (IsActive(pool[i]))
            {
                activeComponents[j] = pool[i];
                j++;
            }
        }

        return activeComponents;
    }

    public void ReturnComponent(T component)
    {
        component.enabled = false;
    }
}


public class AudioSourcePooler : ComponentPooler<AudioSource>
{
    public AudioSourcePooler(GameObject targetObject, int poolSize = 1, float poolSizeExpandFactor = 2) : base(targetObject, poolSize, poolSizeExpandFactor)
    {
    }

    override protected bool IsActive(AudioSource component)
    {
        return component.isPlaying;
    }
}