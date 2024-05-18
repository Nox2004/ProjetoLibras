using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletController : ObjectFromPool, IPausable
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

    public float speed, zLimit; //speed of the bullet, and the limit of the bullet in the z axis
    [HideInInspector] public ParticleManager particleManager;
    [HideInInspector] public AudioManager playerAudioManager;
    [SerializeField] private GameObject hitParticlePrefab;

    public float range = 10f; private float distanceTraveled;
    public float damage = 1; private float initialDamage = 0;
    public float knockback = 0f;
    public int pierce = 0; private int initialPierce = 0;
    private GameObject[] ignoreList;

    [SerializeField] private GameObject coneObject, sphereObject;
    [SerializeField] public AudioClip shootSound, collideSound;

    private void HitObject (Vector3 hit_pos, ITakesDamage obj)
    {
        obj.TakeDamage(damage, pierce);

        playerAudioManager.PlaySound(collideSound, true);

        //if it is an enemy
        if (obj is EnemyController)
        {
            EnemyController enemy = obj as EnemyController;
            enemy.KnockbackForce += transform.forward * knockback * (1f-enemy.KnockbackResistance);
            pierce-= enemy.piercingResistance;

            if (pierce < 0)
            {
                DestroyBullet(hit_pos);
            }
            else
            {
                //Reduces damage for each enemy pierced 
                //e.g. current pierce is 0 and initial pierce is 3, damage = initialDamage * (1/4)
                damage = initialDamage * ((pierce+1) / (initialPierce+1));

                //Emit some particles
                EmitParticleBurst(hit_pos);

                //Change shape depending on pierce
                if (pierce > 0) 
                {
                    coneObject.SetActive(true);
                    sphereObject.SetActive(false);
                }
                else 
                {
                    coneObject.SetActive(false);
                    sphereObject.SetActive(true);
                }

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
            DestroyBullet(hit_pos);
        }
    }

    private void DestroyBullet(Vector3 position)
    {
        EmitParticleBurst(position);

        pooler.ReturnObject(gameObject);
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

    //Is called one frame after the object is enabled
    override protected void AfterEnable()
    {
        base.AfterEnable();

        if (pierce > 0) 
        {
            coneObject.SetActive(true);
            sphereObject.SetActive(false);
        }
        else 
        {
            coneObject.SetActive(false);
            sphereObject.SetActive(true);
        }

        initialPierce = pierce;
        initialDamage = damage;
        distanceTraveled = 0f;

        //!!!CHANGE LATER
        float scale = initialDamage;
        if (scale < 1)
        {
            scale = Mathf.Lerp(0.5f, 1f, scale);
        }
        else
        {
            //scale*=2;
        }
        //Set scale
        transform.localScale = new Vector3(scale, scale, scale);

        //playerAudioManager.PlaySound(shootSound);
        
        //Initializes ignore list
        ignoreList = new GameObject[pierce];

        //Emit particles
        particleManager.EmitRadiusBurst(transform.position, 
                                        6, 
                                        hitParticlePrefab, 
                                        transform.rotation.eulerAngles,// + new Vector3(0, 180f, 0),
                                        Vector3.up * 60f);//Vector3.up * 10f);
    }

    override protected void Update()
    {
        if (paused) return;
        
        base.Update();

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
        bool ignore_this = false;
        for (int i = 0; i < ignoreList.Length; i++)
        {
            if (other.transform.gameObject == ignoreList[i]) ignore_this = true;
        }
        
        //checks if object implements ITakesDamage
        ITakesDamage takesDamage = other.transform.GetComponent<ITakesDamage>();

        if (takesDamage != null && !ignore_this)
        {
            if (takesDamage.alive)
            {
                HitObject(transform.position, takesDamage);
            }
        }
    }
}
