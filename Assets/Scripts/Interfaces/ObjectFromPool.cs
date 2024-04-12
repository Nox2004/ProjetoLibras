using System.Collections;
using UnityEngine;

public abstract class ObjectFromPool : MonoBehaviour
{
    public ObjectPooler pooler;
    protected bool onActivated = false;

    protected void OnEnable()
    {
        onActivated = true;
    }

    virtual protected void Update()
    {
        if (onActivated)
        {
            onActivated = false;
            AfterEnable();
        }
    }

    virtual protected void AfterEnable() { }
}