using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShootingSpiderController : CardboardEnemyController
{
    [Header("Shooting Spider Textures")]
    [SerializeField] private Texture patrolTexture;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture deadTexture;
    [SerializeField] private Texture shootingTexture;


    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Speed")]
    [SerializeField] private float minHorizonalSpeed;
    [SerializeField] private float maxHorizonalSpeed;
    private float horizonalSpeed;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private AudioClip shootingSound;
    [SerializeField] private float shootingCooldown;
    [SerializeField] private Vector3 projectileOffset;
    private float shootingTimer = 0f, shootingTextureTimer = 0f, shootingTextureTimer1 = 0f;


    override protected void Start()
    {
        base.Start();

        ChangePaperTexture(patrolTexture);

        horizonalSpeed = Random.Range(minHorizonalSpeed, maxHorizonalSpeed) * (Random.Range(0, 2) == 0 ? 1 : -1);
        shootingTimer = shootingCooldown;
    }
    
    override protected void Update()
    {
        if (paused) return;
        
        base.Update();

        if (state == EnemyState.Moving)
        {
            transform.position += Vector3.right * horizonalSpeed * Time.deltaTime;
            if (transform.position.x >= spawnPosition.x + floorWidth/2f - width/2f || transform.position.x <= spawnPosition.x - floorWidth/2f + width/2f)
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

        if (shootingTimer > 0)
        {
            shootingTimer -= Time.deltaTime;
        }
        else if (state != EnemyState.Dead)
        {
            ChangePaperTexture(shootingTexture);
            shootingTextureTimer = takeDamageTextureDuration;
            shootingTextureTimer1 = takeDamageTextureDuration/2;

            shootingTimer = shootingCooldown;
        }

        if (shootingTextureTimer > 0 && state != EnemyState.Dead)
        {
            shootingTextureTimer -= Time.deltaTime;

            if (shootingTextureTimer1 > 0)
            {
                shootingTextureTimer1 -= Time.deltaTime;
                if (shootingTextureTimer1 <= 0)
                {
                    Shoot();
                }    
            }

            if (shootingTextureTimer <= 0)
            {
                ChangePaperTexture(patrolTexture);
            }
        }
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position+projectileOffset, Quaternion.identity);
        projectile.GetComponent<SpiderProjectile>().baseSpeed = speed * speedMultiplier;
        audioManager.PlaySound(shootingSound);
    }
    
    override public void TakeDamage(float damage, int pierce)
    {
        ChangePaperTexture(takingDamageTexture);
        takeDamageTimer = takeDamageTextureDuration;

        if (shootingTextureTimer > 0 && shootingTextureTimer1 > 0)
        {
            shootingTimer = 0;
            shootingTextureTimer = 0;
            shootingTextureTimer1 = 0;
        } 

        base.TakeDamage(damage, pierce);
    }

    override public void Die()
    {
        ChangePaperTexture(deadTexture);

        base.Die();
    }
}
