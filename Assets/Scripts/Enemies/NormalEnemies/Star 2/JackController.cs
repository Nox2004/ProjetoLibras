using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class JackController : CardboardEnemyController
{
    
    [Header("Jack Textures")]
    [SerializeField] private Texture hidingTexture;
    [SerializeField] private Texture showingUpTexture;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture deadTexture;

    [Header("Damage")]
    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Hiding timing")]
    [SerializeField] private float minHidingDistance;
    [SerializeField] private float maxHidingDistance;
    [SerializeField] private float minShowingDistance;
    [SerializeField] private float maxShowingDistance;

    private float hidingCount;
    private bool hiding = true;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] showUpSounds;
    [SerializeField] private AudioClip[] hideSounds;

    override protected void Start()
    {
        base.Start();

        hiding = Random.Range(0, 2) == 0;

        ChangePaperTexture(hiding ? hidingTexture : showingUpTexture);

        hidingCount = hiding ? Random.Range(minHidingDistance, maxHidingDistance) : Random.Range(minShowingDistance, maxShowingDistance);
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
                ChangePaperTexture(hiding ? hidingTexture : showingUpTexture);
            }
        }
        if (state != EnemyState.Dead)
        {
            hidingCount -= speed * Time.deltaTime;
            if (hidingCount <= 0)
            {
                hiding = !hiding;
                hidingCount = hiding ? Random.Range(minHidingDistance, maxHidingDistance) : Random.Range(minShowingDistance, maxShowingDistance);
                ChangePaperTexture(hiding ? hidingTexture : showingUpTexture);

                audioManager.PlayRandomSound(hiding ? hideSounds : showUpSounds);
            }
        }
    }

    override public void TakeDamage(float damage, int pierce)
    {
        if (hiding) return;

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
