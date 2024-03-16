using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debug; private string debugTag = "PlayerController: ";

    [Header("Important references")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private ParticleManager particleManager;
    
    [Header("Horizontal Movement")]
    [SerializeField] [Range(0, 1)] [Tooltip("Ratio of smooth moving: Player will move [Distance to touch / This] every second")] 
    private float smoothMoveRatio;
    [SerializeField] private float xStart, xRange; //Starting X in game world, and range of horizontal movement in game world
    [SerializeField] [Range(0, 0.5f)] private float touchXTreshold; //Treshold for touch position to be considered as a movement

    [Header("Shooting")]
    private IEnumerator shootingCoroutine; private bool isShooting;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Vector3 bulletSpawnOffset;
    [SerializeField] private float shootCooldown, bulletSpeed, bulletZLimit;

    //State machine properties
    public IPlayerState currentState;
    public ShootingState shootingState; public UpgradeState upgradeState;

    //State machine methods
    public void SetState(IPlayerState state)
    {
        if (debug) Debug.Log(debugTag + "SetState [" + "" + "]");

        currentState?.ExitState(this);
        currentState = state;
        currentState.EnterState(this);
    }

    //Shooting coroutine
    public IEnumerator ShootCoroutine()
    {
        while (true)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position + bulletSpawnOffset, Quaternion.identity);
            PlayerBulletController bullet_controller = bullet.GetComponent<PlayerBulletController>();

            bullet_controller.speed = bulletSpeed;
            bullet_controller.zLimit = bulletZLimit;
            bullet_controller.particleManager = particleManager;

            yield return new WaitForSeconds(shootCooldown);
        }
    }

    public void SmoothHorizontalMovement(float x_target)
    {
        float step = Mathf.Abs(transform.position.x - x_target) / (smoothMoveRatio / Time.deltaTime);

        Vector3 target_pos = transform.position;
        target_pos.x = x_target;
        transform.position = Vector3.MoveTowards(transform.position, target_pos, step);
    }

    void Start()
    {
        shootingState = new ShootingState(xStart, xRange, touchXTreshold, smoothMoveRatio);
        upgradeState = new UpgradeState(levelManager, touchXTreshold);

        //Sets the initial state
        shootingCoroutine = ShootCoroutine();
        SetState(shootingState);
    }

    void Update()
    {
        currentState.UpdateState(this);
    }

    //Stop/Resumes shooting
    public void SetShooting(bool value)
    {
        if (debug) Debug.Log(debugTag + "SetShooting(" + value + ")");

        if (value)
        {
            if (isShooting) 
            {
                if (debug) Debug.LogError(debugTag + "Error - Player is already shooting");
                return;
            }

            StartCoroutine(shootingCoroutine); isShooting = true;
        }
        else
        {
            if (!isShooting) 
            {
                if (debug) Debug.LogError(debugTag + "Error - Player is not shooting");
                return;
            }

            StopCoroutine(shootingCoroutine); isShooting = false;
        }
    }
}





