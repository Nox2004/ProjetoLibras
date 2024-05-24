using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RobobertoController : CardboardEnemyController
{
    [Header("Robot Textures")]
    [SerializeField] private Texture patrolTexture;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture defendingTexture;
    [SerializeField] private Texture deadTexture;

    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Robot Properties")]
    [SerializeField] private float defenseAgainstNonPierce = 0.7f;
    private float defense = 0f;
    [SerializeField] private float startDefense = 0f;
    [SerializeField] private float endDefense = 0.2f;
    [SerializeField] private float defenseCap = 0.5f;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] defendSounds;

    override protected void Start()
    {
        base.Start();

        ChangePaperTexture(patrolTexture);

        defense = Mathf.Lerp(startDefense, endDefense, difficultyValue);
        defense = Mathf.Max(defense, defenseCap);
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

            base.TakeDamage(damage*(1-defenseAgainstNonPierce), pierce);

            return;
        }

        ChangePaperTexture(takingDamageTexture);
        takeDamageTimer = takeDamageTextureDuration;

        base.TakeDamage(damage*(1-defense), pierce);
    }

    override public void Die()
    {
        ChangePaperTexture(deadTexture);

        base.Die();
    }
}
