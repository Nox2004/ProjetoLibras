using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class EnemyController : MonoBehaviour, ITakesDamage, IPausable
{
    #region //IPausable implementation

    protected bool paused = false;

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion

    [Header("Debug")]
    public bool debug; private string debugTag;

    [Header("Width in units")]
    public float width;

    [Header("Score")]
    [SerializeField] private int pointsOnDefeated;

    [Header("Important References")]
    [SerializeField] private Renderer[] effectRenderers; //Rendereres in which effects are applied
    [HideInInspector] public LevelManager levelManager;
    protected AudioManager audioManager;
    [HideInInspector] public ParticleManager particleManager;

    //Enemy states
    protected enum EnemyState
    {
        Moving,
        Dead
    }

    protected EnemyState state;
    
    #region //Implements take damage
    public bool alive { get => state != EnemyState.Dead; set {  } }

    [Header("Health")]
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
                // if (value <= 0f)
                // {
                //     Die();
                // }
            }

            _health = Mathf.Clamp(value, 0f, maxHealth); // Enforce health bounds
        }
    }
    
    #endregion

    //Z limit for the enemy to be destroyed
    [HideInInspector] public float zLimit;
    [HideInInspector] public float difficultyValue;
    [HideInInspector] public Vector3 spawnPosition;
    [HideInInspector] public float floorWidth;

    [Header("Combat and Movement")]
    [HideInInspector] public float speed;
    public float damage;
    public int piercingResistance = 1;
    [Range(0,1)] public float KnockbackResistance;
    [Range(0,1)] public float KnockbackMultiplier = 0.85f;
    //private Vector3 _knockbackForce;
    [HideInInspector] public Vector3 KnockbackForce;

    [Header("Effects")]
    private float whiteEffectStrength;
    public float whiteEffectDuration;

    protected float minX, maxX;

    virtual protected void Start()
    {
        debugTag = "EnemyController - [" + gameObject.name + "]: ";
        if (debug) Debug.Log(debugTag + "Start");

        //Set the current health to the max health
        currentHealth = maxHealth;

        //Injects the audio manager
        audioManager = Injector.GetAudioManager(gameObject);

        minX = spawnPosition.x - floorWidth/2f + width/2f;
        maxX = spawnPosition.x + floorWidth/2f - width/2f;

        Vector3 tmp = transform.position;
        tmp.x = Mathf.Clamp(tmp.x, minX, maxX);
        transform.position = tmp;
    }

    virtual protected void Update()
    {
        if (paused) return;

        //Apply knockback force
        if (KnockbackForce.sqrMagnitude > Mathf.Epsilon) // Avoid small force application
        {
            //Apply knockback resistance
            if (KnockbackResistance > 0) KnockbackForce *= Mathf.Pow((1-KnockbackMultiplier),Time.deltaTime);

            //Apply knockback force
            transform.position += KnockbackForce * Time.deltaTime;
        }

        //Apply white effect strength
        if (whiteEffectStrength > 0) whiteEffectStrength -= Time.deltaTime / whiteEffectDuration;
        else whiteEffectStrength = 0;
        
        for (int i = 0; i < effectRenderers.Length; i++)
        {
            Renderer rend = effectRenderers[i];
            
            //Sets the white effect in the first material of that renderer
            for (int j = 0; j < rend.materials.Length; j++)
            {
                Material rendMaterial = rend.materials[j];
                rendMaterial.SetFloat("_WhiteEffectStrength", whiteEffectStrength);
            }
            //rend.material.SetFloat("_WhiteEffectStrength", whiteEffectStrength);
        }

        //if the enemy is out of the screen, destroy it
        if (transform.position.z < zLimit)
        {
            if (debug) Debug.Log(debugTag + "Out of screen - Destroying enemy");

            //Removes the enemy from the level manager alive enemies list
            levelManager.RemoveEnemy(this);
        
            Destroy(gameObject);
        } 

        //Clamp the enemy position to the floor
        Vector3 tmp = transform.position;
        tmp.x = Mathf.Clamp(tmp.x, minX, maxX);
        transform.position = tmp;
    }

    virtual public void TakeDamage(float damage, int pierce)
    {
        if (debug) Debug.Log(debugTag + "Took " + damage + " damage" + " with " + pierce + " pierce");
        
        //Apply damage
        currentHealth -= damage;
        whiteEffectStrength = 1;

        //If the enemy is dead, Die()
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    virtual public void Die()
    {
        if (debug) Debug.Log(debugTag + "HP reached zero - Destroying enemy");
        
        //Changes the state to dead
        state = EnemyState.Dead;

        //Removes the enemy from the level manager alive enemies list
        levelManager.AddStarScore(pointsOnDefeated);
        levelManager.RemoveEnemy(this);

        //destroy collider for performance
        Destroy(GetComponent<Collider>());
    }
}
