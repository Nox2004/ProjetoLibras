using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EvilRobotController : CardboardEnemyController
{
    [Header("Robot Textures")]
    [SerializeField] private Texture patrolTexture;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture defendingTexture;
    [SerializeField] private Texture deadTexture;

    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [SerializeField] private AudioClip[] defendSounds;

    override protected void Start()
    {
        base.Start();

        ChangePaperTexture(patrolTexture);
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
    }

    override public void TakeDamage(float damage, int pierce)
    {
        // Defends against non piercing attacks
        if (pierce <= 0)
        {
            ChangePaperTexture(defendingTexture);
            takeDamageTimer = takeDamageTextureDuration;

            audioManager.PlayRandomSound(defendSounds);

            return;
        }

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
