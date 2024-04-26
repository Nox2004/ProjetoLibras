using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITakesDamage {
    bool alive { get; set; }
    
    float maxHealth { get; set; }
    float currentHealth { get; set; }

    void TakeDamage(float damage, int pierce);

    void Die();
}   
