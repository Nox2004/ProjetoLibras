using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ZombiaController : CardboardEnemyController
{
    [Header("Racing Zombie Textures")]
    [SerializeField] private Texture patrol1Texture;
    [SerializeField] private Texture patrol2Texture;
    private Texture currentPatrol;
    [SerializeField] private float patrolTextureChangeTime = 0.5f;
    private float patrolTimer = 0f;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture deadTexture;

    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Racing Zombie Properties")]
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    private float run_speed;
    [SerializeField] private Transform wheels;
    [SerializeField] private float wheelsRotationMultiplier;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] rollerSounds;

    override protected void Start()
    {
        base.Start();

        currentPatrol = patrol1Texture;
        ChangePaperTexture(patrol1Texture);

        run_speed = Random.Range(minSpeed, maxSpeed);
    }
    
    override protected void Update()
    {
        if (paused) return;
        
        base.Update();

        if (takeDamageTimer > 0)
        {
            takeDamageTimer -= Time.deltaTime;
            if (takeDamageTimer <= 0 && state != EnemyState.Dead)
            {
                ChangePaperTexture(currentPatrol);
            }
        }
        else 
        {
            if (state != EnemyState.Dead)
            {
                patrolTimer -= Time.deltaTime;
                if (patrolTimer <= 0)
                {
                    currentPatrol = currentPatrol == patrol1Texture ? patrol2Texture : patrol1Texture;
                    patrolTimer = patrolTextureChangeTime;
                    ChangePaperTexture(currentPatrol);
                    audioManager.PlayRandomSound(rollerSounds);
                }
            }
        }

        if (state == EnemyState.Moving)
        {
            transform.position += Vector3.back * run_speed * Time.deltaTime;

            
                wheels.Rotate(Vector3.right, -run_speed * wheelsRotationMultiplier * Time.deltaTime);
            
        }
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
