using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobobertaProjectile : MonoBehaviour, IPausable
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

    //[SerializeField] private GameObject projectile;
    //[SerializeField] private GameObject onFloor;

    private int state = 0;
    [SerializeField] private float speed;
    [SerializeField] private float debuffTime;
    //private float initialDebuffTime;
    [HideInInspector] public float baseSpeed;
    [HideInInspector] public ParticleManager particleManager;
    [HideInInspector] public GameObject particlePrefab;
    [HideInInspector] public AudioManager audioManager;
    [SerializeField] private AudioClip hitSound;

    void Start()
    {
        //initialDebuffTime = debuffTime;
    }

    void Update()
    {
        if (paused) return;
        
        switch (state)
        {
            case 0:
            {
                //moves forward
                transform.Translate(Vector3.forward * (baseSpeed + speed) * Time.deltaTime);
            }
            break;
            case 1:
            {
                // debuffTime -= Time.deltaTime;
                // transform.localScale = Vector3.one * (debuffTime / initialDebuffTime);
                // if (debuffTime <= 0)
                // {
                //     Destroy(gameObject);
                // }
            }
            break;
        }
        
    }

    //on colliding with player
    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            particleManager.EmitExplosion(transform.position, 15, particlePrefab);

            player.unableToShootTimer = debuffTime;
            audioManager.PlaySound(hitSound);
            Destroy(gameObject);
        //     transform.position = player.transform.position;

        //     projectile.SetActive(false);
        //     onFloor.SetActive(true);

        //     state = 1;
        //     Debug.Log("Player hit by spider projectile");
        }
    }
}
