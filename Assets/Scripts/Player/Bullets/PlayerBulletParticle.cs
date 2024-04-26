using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletParticle : ObjectFromPool, IParticle, IPausable
{
    #region //IPausable implementation

    private bool paused = false;

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion

    public Vector3 direction { get; set; }
    public float speed { get; set; }

    public float lifeTime { get; set; }
    public float lifeSpan { get; set; }

    [SerializeField] private Vector2 lifeTimeRange, speedRange;
    [SerializeField] private float speedMultiplierOverTime = 0.1f;
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    override protected void AfterEnable()
    {
        base.AfterEnable();

        lifeTime = Random.Range(lifeTimeRange.x, lifeTimeRange.y);
        lifeSpan = lifeTime;

        speed = Random.Range(speedRange.x, speedRange.y);
        transform.rotation = Quaternion.Euler(direction);

        transform.localScale = initialScale;
    }

    override protected void Update()
    {
        if (paused) return;

        base.Update();

        lifeSpan -= Time.deltaTime;

        float _lifespan = lifeSpan / lifeTime;

        transform.localScale = initialScale * _lifespan;
        speed *= Mathf.Pow(speedMultiplierOverTime, Time.deltaTime);

        //transform.Translate(direction * speed * Time.deltaTime);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
        if (lifeSpan <= 0)
        {
            pooler.ReturnObject(gameObject);
        }
    }
}
