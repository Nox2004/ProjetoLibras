using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderProjectile : MonoBehaviour, IPausable
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

    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject onFloor;

    private int state = 0;
    [SerializeField] private float speed;
    [SerializeField] private float debuffTime;
    [HideInInspector] public float baseSpeed;

    void Start()
    {

    }

    void Update()
    {
        if (paused) return;
        
        switch (state)
        {
            case 0:
            {
                transform.Translate(Vector3.back * (baseSpeed + speed) * Time.deltaTime);
            }
            break;
            case 1:
            {
                debuffTime -= Time.deltaTime;
                if (debuffTime <= 0)
                {
                    Destroy(gameObject);
                }
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
            player.stuckInPlace = debuffTime;
            transform.position = player.transform.position;

            projectile.SetActive(false);
            onFloor.SetActive(true);

            state = 1;
            Debug.Log("Player hit by spider projectile");
        }
    }
}
