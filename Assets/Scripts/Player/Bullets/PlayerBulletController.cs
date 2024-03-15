using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    public float speed, zLimit; //speed of the bullet, and the limit of the bullet in the z axis
    [HideInInspector] public ParticleManager particleManager;
    [SerializeField] private GameObject hitParticlePrefab;

    [SerializeField] private GameObject emissionParticlePrefab;
    [SerializeField] private float min_part_cooldown, max_part_cooldown;
    //private Vector3 direction = Vector3.forward;

    // IEnumerator EmitParticles()
    // {
    //     // float r = 0.2f;

    //     // while (true)
    //     // {
    //     //     GameObject part = particleManager.EmitSingleParticle(Vector3.zero,
    //     //     emissionParticlePrefab, transform.rotation.eulerAngles + new Vector3(0, 180f, 0));

    //     //     part.transform.parent = transform;
    //     //     part.transform.localPosition = Vector3.zero;
    //     //     part.transform.localPosition += new Vector3(Random.Range(-r, r),0,Random.Range(r, r));
    //     //     yield return new WaitForSeconds(Random.Range(min_part_cooldown, max_part_cooldown));
    //     // }
    // }

    void Start()
    {
        //StartCoroutine(EmitParticles());
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime); //moves towards direction
        
        //if the bullet is out of the screen, destroy it
        if (transform.position.z > zLimit)
        {
            Destroy(gameObject);
        }

        
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime))
        {
            //checks if object implements ITakesDamage
            ITakesDamage takesDamage = hit.transform.GetComponent<ITakesDamage>();

            if (takesDamage != null)
            {
                if (takesDamage.alive)
                {
                    takesDamage.currentHealth--;

                    //if it is an enemy
                    if (takesDamage is EnemyController)
                    {
                        //!!!Apply knockback and other stuff here later
                    }

                    //Emit particles
                    particleManager.EmitRadiusBurst(hit.point, 
                                                    10, 
                                                    hitParticlePrefab, 
                                                    transform.rotation.eulerAngles,// + new Vector3(0, 180f, 0),
                                                    Vector3.up * 180f);//Vector3.up * 10f);

                    Destroy(gameObject);
                }
            }
        }
    }
}
