using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BossController : EnemyController
{
    [SerializeField] protected bool invincible = false;
    
    [Header("Sounds")]
    [SerializeField] AudioClip[] deathSounds;
    [SerializeField] AudioClip[] hitSounds;    

    override protected void Start()
    {
        base.Start();
    }
    
    override protected void Update()
    {
        base.Update();
    }

    public override void TakeDamage(float damage, int pierce)
    {
        if (invincible) return;
        
        base.TakeDamage(damage, pierce);

        audioManager.PlayRandomSound(hitSounds);
    }

    override public void Die()
    {
        base.Die();

        audioManager.PlayRandomSound(deathSounds);
    }
}
