using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    public float max_hp, hp;
    public float zLimit;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }

        //if the enemy is out of the screen, destroy it
        if (transform.position.z < zLimit)
        {
            Destroy(gameObject);
        }
    }
}
