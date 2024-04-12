using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour, IPausable
{
    #region //IPausable implementation

    private bool paused = false;
    private bool wasShootingBeforePause = false;
    public void Pause()
    {
        paused = true;
        //stops coroutine if player is shooting
        if (isShooting) 
        {
            SetShooting(false);
            wasShootingBeforePause = true;
        }
    }

    public void Resume()
    {
        paused = false;
        //resumes coroutine if player was shooting before pause
        if (wasShootingBeforePause) 
        {
            SetShooting(true);
            wasShootingBeforePause = false;
        }
        
    }

    #endregion

    [Header("Debug")]
    [SerializeField] private bool debug; private string debugTag = "PlayerController: ";

    [Header("Important references")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private ParticleManager particleManager;
    private AudioManager audioManager;
    
    [Header("Horizontal Movement")]
    [SerializeField] [Range(0, 1)] [Tooltip("Ratio of smooth moving: Player will move [Distance to touch / This] every second")] 
    private float smoothMoveRatio;

    [SerializeField] Camera mainCamera; 
    [SerializeField] private LayerMask floorLayerMask; // Layer mask for the floor

    [SerializeField] private float xStart, xRange; //Starting X in game world, and range of horizontal movement in game world (floor width)

    [Header("Bullet Instantiation")]
    private IEnumerator shootingCoroutine; private bool isShooting;
    [SerializeField] private GameObject bulletPrefab;
    private ObjectPooler bulletPooler;
    [SerializeField] private Vector3 bulletSpawnOffset;
    [SerializeField] private float bulletZLimit;
    [SerializeField, Tooltip("The arc (in degrees) in which bullets will be instantiated if player shoots more than one.")] private float bulletsAngleArc;


    [Header("Shooting")]
    [SerializeField, Tooltip("Shoots per second")] private float ShootRate; private float initialShootRate;
    private float shootCooldown => 1f / ShootRate; //Cooldown between shoots
    [SerializeField] private int bulletsPerShoot = 1; private int initialBulletsPerShoot;
    [SerializeField] private float bulletSpeed; private float initialBulletSpeed;
    [SerializeField] private float bulletRange; private float initialBulletRange;
    [SerializeField] private int bulletDamage; private int initialBulletDamage;
    [SerializeField] private float bulletKnockback; private float initialBulletKnockback;
    [SerializeField] private int bulletPenetration; private int initialBulletPenetration;

    //State machine properties
    public IPlayerState currentState;
    public ShootingState shootingState; public UpgradeState upgradeState;

    //Shooting upgrade properties
    private enum Status { ShootRate, Damage, Speed, Range, BulletsPerShoot, Knockback, Penetration, Count }
    private int[] statusLevels = new int[(int)Status.Count];

    public void Upgrade()
    {
        //get lowest level status
        int min = int.MaxValue;
        for (int i = 0; i < (int)Status.Count; i++) {
            if (statusLevels[i] < min) min = statusLevels[i];
        }

        //upgrade a random status of the lowest level
        while (true)
        {
            int random = Random.Range(0, (int)Status.Count);
            if (statusLevels[random] == min)
            {
                statusLevels[random]++;
                Debug.Log(debugTag + "Upgraded " + (Status)random + " to level " + statusLevels[random]);
                break;
            }
        }

        //apply upgrades
        ShootRate = initialShootRate + statusLevels[(int)Status.ShootRate]; //0 - 1 - 2 - 3
        bulletDamage = initialBulletDamage + statusLevels[(int)Status.Damage]; //0 - 1 - 2 - 3
        bulletSpeed = initialBulletSpeed + Mathf.Sqrt(statusLevels[(int)Status.Speed] * 200f); //0 -14 - 20
        bulletRange = initialBulletRange + Mathf.Sqrt(statusLevels[(int)Status.Range] * 36f); //0 - 6 - 8 - 10
        bulletsPerShoot = initialBulletsPerShoot + statusLevels[(int)Status.BulletsPerShoot]; //1 - 2 - 3 - 4
        bulletKnockback = initialBulletKnockback + Mathf.Sqrt(statusLevels[(int)Status.Knockback] * 25f); //0 - 5 - 7 - 9
        bulletPenetration = initialBulletPenetration + statusLevels[(int)Status.Penetration]; //0 - 1 - 2 - 3
    }

    //State machine methods
    public IPlayerState GetState() { return currentState; }
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
            Shoot();

            yield return new WaitForSeconds(shootCooldown);
        }
    }

    public void SimpleShoot()
    {
        GameObject bullet = bulletPooler.GetObject(transform.position + bulletSpawnOffset, Quaternion.identity);
        PlayerBulletController bullet_controller = bullet.GetComponent<PlayerBulletController>();

        bullet_controller.zLimit = bulletZLimit;
        bullet_controller.particleManager = particleManager;
        bullet_controller.playerAudioManager = audioManager;
        bullet_controller.speed = 35f;
        bullet_controller.damage = 1;
        bullet_controller.range = 30f;
        bullet_controller.knockback = 0f;
        bullet_controller.pierce = 0;
    }

    public void Shoot()
    {
        float angle = -bulletsAngleArc / 2;
        for (int i = 0; i < bulletsPerShoot; i++)
        {
            angle += bulletsAngleArc / (bulletsPerShoot + 1); 
            //Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            GameObject bullet = bulletPooler.GetObject(transform.position + bulletSpawnOffset, Quaternion.Euler(0,angle,0));
            PlayerBulletController bullet_controller = bullet.GetComponent<PlayerBulletController>();

            //Populate bullet properties
            bullet_controller.zLimit = bulletZLimit;
            bullet_controller.particleManager = particleManager;
            bullet_controller.playerAudioManager = audioManager;
            bullet_controller.speed = bulletSpeed;
            bullet_controller.damage = bulletDamage;
            bullet_controller.range = bulletRange;
            bullet_controller.knockback = bulletKnockback;
            bullet_controller.pierce = bulletPenetration;
        }
    }

    public float GetTouchX(Touch t)
    {
        Ray ray = mainCamera.ScreenPointToRay(t.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayerMask))
        {
            //Return floor position
            return hit.point.x;
        }
        else 
        {
            //Return the edges X position
            if (t.position.x < Screen.width / 2)
            {
                return xStart - xRange/2;
            }
            else
            {
                return xStart + xRange/2;
            }
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
        audioManager = Injector.GetAudioManager(gameObject);

        shootingState = new ShootingState();
        upgradeState = new UpgradeState(levelManager);

        bulletPooler = new ObjectPooler(bulletPrefab);

        //Sets the initial state
        shootingCoroutine = ShootCoroutine();
        SetState(shootingState);

        //Initializes the upgrade levels
        for (int i = 0; i < (int)Status.Count; i++) statusLevels[i] = 0;
        initialBulletDamage = bulletDamage;
        initialBulletKnockback = bulletKnockback;
        initialBulletRange = bulletRange;
        initialBulletSpeed = bulletSpeed;
        initialBulletsPerShoot = bulletsPerShoot;
        initialShootRate = ShootRate;
    }

    void Update()
    {
        if (paused) return;

        currentState.UpdateState(this);
    }

    //on colliding
    void OnTriggerEnter(Collider other)
    {
        //checks if object implements ITakesDamage
        ITakesDamage takesDamage = other.gameObject.GetComponent<ITakesDamage>();

        if (takesDamage != null)
        {
            if (takesDamage.alive)
            {
                //Restarts the game
                levelManager.RestartLevel();
            }
        }
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





