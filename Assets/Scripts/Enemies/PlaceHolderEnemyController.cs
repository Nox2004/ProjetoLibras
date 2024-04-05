using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlaceHolderEnemyController : EnemyController
{
    [SerializeField] private float speedMultiplier;
    
    //Falling when dead animation
    private float fallSpeed = 0f;
    [SerializeField] private float fallAcceleration = 0.1f;

    AudioSource audioSource;
    [SerializeField] AudioClip deathSound;

    override protected void Start()
    {
        base.Start();

        audioSource = gameObject.AddComponent<AudioSource>();

        speed *= speedMultiplier;
    }
    
    override protected void Update()
    {
        base.Update();

        transform.position += Vector3.back * speed * Time.deltaTime; //Not using transform.Translate because its affected by the object's rotation

        if (state == EnemyState.Dead)
        {
            fallSpeed += fallAcceleration * Time.deltaTime;
            if (transform.rotation.eulerAngles.x < 60)
            {
                var tmp = transform.rotation.eulerAngles;
                tmp.x += fallSpeed * Time.deltaTime;
                tmp.x = Mathf.Min(tmp.x, 60);
                
                transform.rotation = Quaternion.Euler(tmp);
            }
        }
    }

    override public void Die()
    {
        base.Die();

        audioSource.PlayOneShot(deathSound);

        //destroy collider for performance
        Destroy(GetComponent<Collider>());
    }
}
