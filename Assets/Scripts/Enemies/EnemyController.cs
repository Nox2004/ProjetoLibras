using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class EnemyController : MonoBehaviour, ITakesDamage
{
    protected enum EnemyState
    {
        Moving,
        Dead
    }

    //Debug properties
    public bool debug; private string debugTag;

    protected EnemyState state;

    //Take damage properties
    public bool alive { get => state != EnemyState.Dead; set {  } }

    [SerializeField] private float _maxHealth; // Private backing field for max health
    public float maxHealth { get => _maxHealth ; set { _maxHealth = value; } }

    private float _health; // Private backing field for current health
    public float currentHealth
    {
        get => _health;
        set
        {
            if (value < _health)
            {
                if (value <= 0f)
                {
                    Die();
                }
                else
                {
                    TakeDamage(_health - value);
                }
            }

            _health = Mathf.Clamp(value, 0f, maxHealth); // Enforce health bounds
        }
    }
    
    //Rendereres in which effects are applied
    [SerializeField] private Renderer[] effectRenderers;

    //Z limit for the enemy to be destroyed
    public float zLimit;

    //Combat properties
    [HideInInspector] public float speed;
    public float damage;
    [Range(0,1)] public float KnockbackResistance;
    [HideInInspector] public Vector3 KnockbackForce;
    private float whiteEffectStrength;
    public float whiteEffectDuration;

    virtual protected void Start()
    {
        debugTag = "EnemyController - [" + gameObject.name + "]: ";
        if (debug) Debug.Log(debugTag + "Start");

        //Set the current health to the max health
        currentHealth = maxHealth;
    }

    virtual protected void Update()
    {
        //Apply knockback force
        if (KnockbackForce.sqrMagnitude > Mathf.Epsilon) // Avoid small force application
        {
            //Apply knockback resistance
            if (KnockbackResistance > 0) KnockbackForce *= Mathf.Pow((1-KnockbackResistance),Time.deltaTime);

            //Apply knockback force
            transform.position += (KnockbackForce * Time.deltaTime);
        }

        //Apply white effect strength
        if (whiteEffectStrength > 0) whiteEffectStrength -= Time.deltaTime / whiteEffectDuration;
        else whiteEffectStrength = 0;
        
        for (int i = 0; i < effectRenderers.Length; i++)
        {
            Renderer rend = effectRenderers[i];
            
            //Sets the white effect in the first material of that renderer
            rend.material.SetFloat("_WhiteEffectStrength", whiteEffectStrength);
        }

        //if the enemy is out of the screen, destroy it
        if (transform.position.z < zLimit)
        {
            if (debug) Debug.Log(debugTag + "Out of screen - Destroying enemy");
            Destroy(gameObject);
        }        
    }

    virtual public void TakeDamage(float damage)
    {
        if (debug) Debug.Log(debugTag + "Took " + damage + " damage");
        
        whiteEffectStrength = 1;
    }

    virtual public void Die()
    {
        if (debug) Debug.Log(debugTag + "HP reached zero - Destroying enemy");
        
        //Changes the state to dead
        state = EnemyState.Dead;
    }
}
