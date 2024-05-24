using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MariaFifiController : CardboardEnemyController
{
    [Header("Shooting Spider Textures")]
    [SerializeField] private Texture patrolTexture;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture deadTexture;
    [SerializeField] private Texture shootingTexture;

    [Header("Wheels")]
    [SerializeField] private Transform[] wheels;
    [SerializeField] private float wheelsRotationMultiplier;

    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Speed")]
    [SerializeField] private float minHorizonalSpeed;
    [SerializeField] private float maxHorizonalSpeed;
    private float horizonalSpeed;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject particlesPrefab;
    private float shootingDistance;
    [SerializeField] private float minShootingDistance;
    [SerializeField] private float maxShootingDistance;
    [SerializeField] private Vector3 projectileOffset;
    private float shootingCount = 0f, shootingTextureTimer = 0f, shootingTextureTimer1 = 0f;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] shootingSounds;

    override protected void Start()
    {
        base.Start();

        shootingDistance = Mathf.LerpUnclamped(minShootingDistance, maxShootingDistance, difficultyValue);

        ChangePaperTexture(patrolTexture);

        horizonalSpeed = Random.Range(minHorizonalSpeed-difficultyValue, maxHorizonalSpeed+difficultyValue) * (Random.Range(0, 2) == 0 ? 1 : -1);
        shootingCount = minShootingDistance;
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

            foreach (Transform wheel in wheels)
            {
                wheel.Rotate(Vector3.forward, -horizonalSpeed * wheelsRotationMultiplier * Time.deltaTime);
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

        if (shootingCount > 0)
        {
            shootingCount -= speed * Time.deltaTime;
        }
        else if (state != EnemyState.Dead)
        {
            ChangePaperTexture(shootingTexture);
            shootingTextureTimer = takeDamageTextureDuration;
            shootingTextureTimer1 = takeDamageTextureDuration/2;

            shootingCount = minShootingDistance;
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
        particleManager.EmitRadiusBurst(transform.position+projectileOffset, 
                                        Random.Range(4, 7),
                                        particlesPrefab, 
                                        transform.rotation.eulerAngles + new Vector3(0, 180f, 0),
                                        Vector3.up * 180f);//Vector3.up * 10f);

        GameObject projectile = Instantiate(projectilePrefab, transform.position+projectileOffset, Quaternion.identity);
        projectile.GetComponent<SpiderWeb>().baseSpeed = speed * speedMultiplier;
        projectile.GetComponent<SpiderWeb>().particleManager = particleManager;
        projectile.GetComponent<SpiderWeb>().particlePrefab = particlesPrefab;
        projectile.GetComponent<SpiderWeb>().audioManager = audioManager;
        audioManager.PlayRandomSound(shootingSounds);
    }
    
    override public void TakeDamage(float damage, int pierce)
    {
        ChangePaperTexture(takingDamageTexture);
        takeDamageTimer = takeDamageTextureDuration;

        if (shootingTextureTimer > 0 && shootingTextureTimer1 > 0)
        {
            shootingCount = 0;
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
