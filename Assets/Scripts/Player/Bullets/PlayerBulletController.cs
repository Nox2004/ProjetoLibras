using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    public float speed, zLimit; //speed of the bullet, and the limit of the bullet in the z axis
    [HideInInspector] public ParticleManager particleManager;
    [SerializeField] private GameObject hitParticlePrefab;

    public float range = 10f; private float distanceTraveled;
    public int damage = 1;
    public float knockback = 0f;
    public int pierce = 0;
    private GameObject[] ignoreList;

    [SerializeField] private AudioClip shootSound, collideSound;
    private AudioSource audioSource;


    //[SerializeField] private GameObject emissionParticlePrefab;
    //[SerializeField] private float min_part_cooldown, max_part_cooldown;
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

    private void HitObject (Vector3 hit_pos, ITakesDamage obj)
    {
        obj.currentHealth-=damage;

        audioSource.PlayOneShot(collideSound);

        //if it is an enemy
        if (obj is EnemyController)
        {
            EnemyController enemy = obj as EnemyController;
            enemy.KnockbackForce = transform.forward * knockback;

            if (pierce <= 0)
            {
                DestroyBullet(hit_pos);
            }
            else
            {
                //Emit some particles
                EmitParticleBurst(hit_pos);

                pierce--;
                //Add this to the ignore list
                for (int i = 0; i < ignoreList.Length; i++)
                {
                    if (ignoreList[i] == null)
                    {
                        ignoreList[i] = enemy.gameObject;
                        break;
                    }
                }
            }
        }

        //If is a sign cardboard
        if (obj is SignObjectController)
        {
            //Debug.Log("Im a sign");
            SignObjectController sign = obj as SignObjectController; 
            sign.ChooseMe();

            DestroyBullet(hit_pos);
        }
    }

    private void DestroyBullet(Vector3 position)
    {
        EmitParticleBurst(position);

        Destroy(gameObject);
    }

    private void EmitParticleBurst(Vector3 position)
    {
        //Emit particles
        particleManager.EmitRadiusBurst(position, 
                                        Random.Range(4, 7),
                                        hitParticlePrefab, 
                                        transform.rotation.eulerAngles,// + new Vector3(0, 180f, 0),
                                        Vector3.up * 180f);//Vector3.up * 10f);
    }

    void Start()
    {
        //StartCoroutine(EmitParticles());

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(shootSound);

        //Initializes ignore list
        ignoreList = new GameObject[pierce];

        //Emit particles
        particleManager.EmitRadiusBurst(transform.position, 
                                        6, 
                                        hitParticlePrefab, 
                                        transform.rotation.eulerAngles,// + new Vector3(0, 180f, 0),
                                        Vector3.up * 60f);//Vector3.up * 10f);
    }

    void LateUpdate()
    {
        //Handles Hits
        // RaycastHit hit;

        // if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime))
        // {
        //     Debug.Log("i was hit");

        //     bool ignore_this = false;
        //     for (int i = 0; i < ignoreList.Length; i++)
        //     {
        //         if (hit.transform.gameObject == ignoreList[i]) ignore_this = true;
        //     }
            
        //     //checks if object implements ITakesDamage
        //     ITakesDamage takesDamage = hit.transform.GetComponent<ITakesDamage>();

        //     if (takesDamage != null && !ignore_this)
        //     {
        //         //Debug.Log("i can take damage");

        //         if (takesDamage.alive)
        //         {
        //             //Debug.Log("im alive");

        //             HitObject(hit, takesDamage);
        //         }
        //     }
        // }

        transform.Translate(Vector3.forward * speed * Time.deltaTime); //moves towards direction
        
        distanceTraveled += speed * Time.deltaTime;

        //if the bullet is out of the screen, destroy it
        if (transform.position.z > zLimit || distanceTraveled > range)
        {
            DestroyBullet(transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //pause game on edtor
        #if UNITY_EDITOR
        //UnityEditor.EditorApplication.isPaused = true;
        #endif

        bool ignore_this = false;
        for (int i = 0; i < ignoreList.Length; i++)
        {
            if (other.transform.gameObject == ignoreList[i]) ignore_this = true;
        }
        
        //checks if object implements ITakesDamage
        ITakesDamage takesDamage = other.transform.GetComponent<ITakesDamage>();

        if (takesDamage != null && !ignore_this)
        {
            //Debug.Log("i can take damage");

            if (takesDamage.alive)
            {
                //Debug.Log("im alive");

                HitObject(transform.position, takesDamage);
            }
        }
    }
}
