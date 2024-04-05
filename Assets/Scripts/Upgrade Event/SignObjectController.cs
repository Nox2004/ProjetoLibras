using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class SignObjectController : MonoBehaviour, ITakesDamage
{
    #region ITakesDamage implementation

    public bool alive { get => !destroy; set {  } }

    private float _maxHealth = 1f; // Private backing field for max health
    public float maxHealth { get => _maxHealth ; set { _maxHealth = value; } }

    private float _health; // Private backing field for current health
    public float currentHealth
    {
        get => _health;
        set
        {
            Die();
        }
    }

    virtual public void TakeDamage(float damage) { }

    virtual public void Die() {}
    
    #endregion

    private Vector3 velocity = new Vector3(0, 0, -1);
    public float speed;

    public bool chosen = false;
    public bool destroy = false;
    private float fallSpeed = 0f;
    [SerializeField] private float fallAcceleration = 250f;

    public float zLimit;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += velocity * speed * Time.deltaTime;

        if (destroy)
        {
            fallSpeed += fallAcceleration * Time.deltaTime;

            if (transform.rotation.eulerAngles.x < 90)
            {
                var tmp = transform.rotation.eulerAngles;
                tmp.x += fallSpeed * Time.deltaTime;
                tmp.x = Mathf.Min(tmp.x, 90);

                transform.rotation = Quaternion.Euler(tmp);
            }
        }
        
        if (transform.position.z < zLimit)
        {
            Destroy(gameObject);
        }
    }

    public void StartDestruction()
    {
        destroy = true;
    }

    public void ChooseMe()
    {
        chosen = true;
    }
}
