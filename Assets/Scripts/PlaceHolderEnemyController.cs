using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlaceHolderEnemyController : EnemyController
{
    [SerializeField] private float speed;
    
    override protected void Start()
    {
        base.Start();
    }
    
    override protected void Update()
    {
        base.Update();

        transform.Translate(Vector3.back * speed * Time.deltaTime);    
    }
}
