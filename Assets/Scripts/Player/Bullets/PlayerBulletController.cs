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
    public int damage = 1;
    public float knockback = 0f;
    public int pierce = 0;
    private GameObject[] ignoreList;

    [SerializeField] private AudioClip shootSound, collideSound;

    private void HitObject (Vector3 hit_pos, ITakesDamage obj)
    {
        obj.currentHealth-=damage;

        playerAudioManager.PlaySound(collideSound);

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
            SignObjectController sign = obj as SignObjectController; 
            sign.ChooseMe();

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

        distanceTraveled = 0f;

        playerAudioManager.PlaySound(shootSound);
        
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
