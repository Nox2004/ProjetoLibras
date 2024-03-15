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
    private float xTarget; //Target X position of the player

    [Header("Shooting")]
    private IEnumerator shootingCoroutine; private bool isShooting;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Vector3 bulletSpawnOffset;
    [SerializeField] private float shootCooldown, bulletSpeed, bulletZLimit;

    //State machine properties
    private IPlayerState currentState;
    private ShootingState shootingState; private AnsweringState answeringState;

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

    void Start()
    {
        shootingState = new ShootingState(xStart, xRange, touchXTreshold, smoothMoveRatio);
        answeringState = new AnsweringState();

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





public class AnsweringState : IPlayerState
{
    public void EnterState(PlayerController me)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState(PlayerController me)
    {
        throw new System.NotImplementedException();
    }

    public void ExitState(PlayerController me)
    {
        throw new System.NotImplementedException();
    }
}