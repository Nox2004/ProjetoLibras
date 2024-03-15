using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletParticle : MonoBehaviour, IParticle
{
    public Vector3 direction { get; set; }
    public float speed { get; set; }

    public float lifeTime { get; set; }
    public float lifeSpan { get; set; }

    [SerializeField] private Vector2 lifeTimeRange, speedRange;
    
    private Vector3 initialScale;

    void Start()
    {
        lifeTime = Random.Range(lifeTimeRange.x, lifeTimeRange.y);
        lifeSpan = lifeTime;

        speed = Random.Range(speedRange.x, speedRange.y);
        transform.rotation = Quaternion.Euler(direction);

        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        lifeSpan -= Time.deltaTime;

        float _lifespan = lifeSpan / lifeTime;

        transform.localScale = initialScale * _lifespan;
        speed *= Mathf.Pow(0.1f, Time.deltaTime);

        //transform.Translate(direction * speed * Time.deltaTime);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
        if (lifeSpan <= 0)
        {
            Destroy(gameObject);
        }
    }
}
