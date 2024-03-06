using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class EnemyController : MonoBehaviour
{
    public bool debug; private string debugTag;
    
    public float max_hp, hp;
    public float zLimit;
    
    virtual protected void Start()
    {
        debugTag = "EnemyController - [" + gameObject.name + "]: ";
        if (debug) Debug.Log(debugTag + "Start");
    }

    virtual protected void Update()
    {
        if (hp <= 0)
        {
            if (debug) Debug.Log("EnemyController [" + gameObject.name + "]: HP reached zero - Destroying enemy");
            Destroy(gameObject);
        }

        //if the enemy is out of the screen, destroy it
        if (transform.position.z < zLimit)
        {
            if (debug) Debug.Log("EnemyController [" + gameObject.name + "]: Out of screen - Destroying enemy");
            Destroy(gameObject);
        }
    }
}
