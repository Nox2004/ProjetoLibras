using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BasicSpiderController : CardboardEnemyController
{
    [Header("Spider Textures")]
    [SerializeField] private Texture patrolTexture;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture deadTexture;

    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Spider Properties")]
    [SerializeField] private float minHorizonalSpeed;
    [SerializeField] private float maxHorizonalSpeed;
    private float horizonalSpeed;


    override protected void Start()
    {
        base.Start();

        ChangePaperTexture(patrolTexture);

        horizonalSpeed = Random.Range(minHorizonalSpeed, maxHorizonalSpeed) * (Random.Range(0, 2) == 0 ? 1 : -1);
    }
    
    override protected void Update()
    {
        if (paused) return;

        if (state == EnemyState.Moving)
        {
            transform.position += Vector3.right * horizonalSpeed * Time.deltaTime;
            if (transform.position.x <= minX || transform.position.x >= maxX)
            {
                horizonalSpeed = -horizonalSpeed;
            }
        }

        if (takeDamageTimer > 0)
        {
            takeDamageTimer -= Time.deltaTime;
            if (takeDamageTimer <= 0 && state != EnemyState.Dead)
            {
                ChangePaperTexture(patrolTexture);
            }
        }

        base.Update();
    }
    
    override public void TakeDamage(float damage, int pierce)
    {
        ChangePaperTexture(takingDamageTexture);
        takeDamageTimer = takeDamageTextureDuration;

        base.TakeDamage(damage, pierce);
    }

    override public void Die()
    {
        ChangePaperTexture(deadTexture);

        base.Die();
    }
}
