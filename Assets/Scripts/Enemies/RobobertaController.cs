using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RobobertaController : CardboardEnemyController
{
    [Header("Shooting Spider Textures")]
    [SerializeField] private Texture patrolTexture;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture deadTexture;
    [SerializeField] private Texture shootingTexture;

    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject particlesPrefab;
    [SerializeField] private AudioClip shootingSound;
    [SerializeField] private float minShootingDistance;
    [SerializeField] private float maxShootingDistance;
    [SerializeField] private Vector3 projectileOffset;
    [SerializeField] private float shootingRightOffset = 0.4f;

    [SerializeField] private float maxDistanceToShoot = 3f;

    private float shootingCount = 0f, shootingTextureTimer = 0f, shootingTextureTimer1 = 0f;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] shootingSounds;

    private GameObject player;

    override protected void Start()
    {
        base.Start();

        ChangePaperTexture(patrolTexture);

        shootingCount = Random.Range(minShootingDistance, maxShootingDistance);
        player = GameObject.Find("Player");
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
                ChangePaperTexture(patrolTexture);
            }
        }

        if (shootingCount > 0)
        {
            if (transform.position.z > (player.transform.position.z + maxDistanceToShoot)) shootingCount -= speed * Time.deltaTime;
        }
        else if (state != EnemyState.Dead)
        {
            ChangePaperTexture(shootingTexture);
            shootingTextureTimer = takeDamageTextureDuration;
            shootingTextureTimer1 = takeDamageTextureDuration/2;

            shootingCount = Random.Range(minShootingDistance, maxShootingDistance);
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
                    audioManager.PlayRandomSound(shootingSounds);
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
        Vector3 bulletPos = transform.position+projectileOffset-Vector3.right*shootingRightOffset;
        Quaternion dir = Quaternion.LookRotation(player.transform.position - bulletPos);
        GameObject projectile = Instantiate(projectilePrefab, bulletPos, dir);
        projectile.GetComponent<RobobertaProjectile>().baseSpeed = speed * speedMultiplier;
        projectile.GetComponent<RobobertaProjectile>().particleManager = particleManager;
        projectile.GetComponent<RobobertaProjectile>().particlePrefab = particlesPrefab;
        projectile.GetComponent<RobobertaProjectile>().audioManager = audioManager;

        particleManager.EmitRadiusBurst(bulletPos, 
                                        Random.Range(4, 7),
                                        particlesPrefab, 
                                        dir.eulerAngles,
                                        Vector3.up * 60f);//Vector3.up * 10f);

        bulletPos = transform.position+projectileOffset+Vector3.right*shootingRightOffset;
        dir = Quaternion.LookRotation(player.transform.position - bulletPos);
        projectile = Instantiate(projectilePrefab, bulletPos, dir);
        projectile.GetComponent<RobobertaProjectile>().baseSpeed = speed * speedMultiplier;
        projectile.GetComponent<RobobertaProjectile>().particleManager = particleManager;
        projectile.GetComponent<RobobertaProjectile>().particlePrefab = particlesPrefab;
        projectile.GetComponent<RobobertaProjectile>().audioManager = audioManager;

        particleManager.EmitRadiusBurst(bulletPos, 
                                        Random.Range(4, 7),
                                        particlesPrefab, 
                                        dir.eulerAngles,
                                        Vector3.up * 60f);

        audioManager.PlaySound(shootingSound);
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
