using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

//private enum Status { ShootRate, Damage, Speed, Range, BulletsPerShoot, Knockback, Penetration, Count }
public enum PlayerStatus { ShootRate, BulletPower, BulletRange, BulletsPerShoot, Penetration, Count }

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
    [SerializeField] private ShowStatusUpgrade upgradeStatusObject;
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] Camera mainCamera; 
    [SerializeField] private LayerMask floorLayerMask; // Layer mask for the floor
    private AudioManager audioManager;
    
    [Header("Horizontal Movement")]
    [SerializeField] private float xStart, xRange; //Starting X in game world, and range of horizontal movement in game world (floor width)
    [SerializeField, Range(0, 1), Tooltip("Ratio of smooth moving: Player will move [Distance to touch / This] every second")] 
    private float smoothMoveRatio;
    private bool touchBeganAtFloor = false;

    [Header("Rigging")]
    [SerializeField] private Transform feetRig;
    [SerializeField] private float feetRotationMultiplier;
    [SerializeField] private float feetRigRotationSmoothRatio;

    [SerializeField] private Transform headRig;
    [SerializeField] private float headRotationMultiplier;
    [SerializeField] private float headRigRotationSmoothRatio;
    private float xLookingAtDistance;

    [SerializeField] private WaveValueInterpolator headWaveyInterpolator;
    private Vector3 headRigInitialPos;

    [Header("Wheels movement")]
    [SerializeField] private SkinnedMeshRenderer wheelRenderer;
    [SerializeField] private int wheelMaterialIndex;
    [SerializeField] private float wheelUVSpeed;
    private float wheelUVOffset;

    [Header("Bullet Instantiation")]
    [SerializeField] private GameObject bulletPrefab;
    private ObjectPooler bulletPooler;
    private IEnumerator shootingCoroutine; private bool isShooting;
    [SerializeField] private Vector3 bulletSpawnOffset;
    [SerializeField] private float bulletZLimit;
    [SerializeField, Tooltip("The arc (in degrees) in which bullets will be instantiated if player shoots more than one.")] private float bulletsAngleArc;


    [Header("Shooting")]
    [SerializeField, Tooltip("Shoots per second")] private float ShootRate; private float initialShootRate;
    private float shootCooldown => 1f / ShootRate; //Cooldown between shoots
    [SerializeField] private int bulletsPerShoot = 1; private int initialBulletsPerShoot;
    [SerializeField] private float bulletSpeed; private float initialBulletSpeed;
    [SerializeField] private float bulletRange; private float initialBulletRange;
    [SerializeField] private float bulletDamage; private float initialBulletDamage;
    [SerializeField] private float bulletKnockback; private float initialBulletKnockback;
    [SerializeField] private int bulletPenetration; private int initialBulletPenetration;
    
    //State machine properties
    public IPlayerState currentState;
    public ShootingState shootingState; public UpgradeState upgradeState;

    //Debuffs
    private float _stuckInPlace = 0f;
    [HideInInspector] public float stuckInPlace 
    {
        get => _stuckInPlace;
        set
        {
            _stuckInPlace = value;
            if (value > 0f)
            {
                stuckInPlacePos = transform.position;
            }
        }
    }
    private Vector3 stuckInPlacePos;

    #region //Shooting upgrade

    private int[] statusLevels = new int[(int) PlayerStatus.Count];

    public void Upgrade(PlayerStatus status, int levelUp = 1)
    {
        statusLevels[(int) status]+= levelUp;
        if (debug) Debug.Log(debugTag + "Upgraded " + status + " to level " + statusLevels[(int) status]);
        //upgradeStatusObject.ShowStatus((Status)random);

        //getDiminishingSum => f(I,L,M,C) = I+Sum(M * C^n,n,0,L-1)
        //initial + 0
        //initial + 0.5
        //initial + 0.5 * 0.8
        //initial + 0.5 * 0.8 + 0.5 * 0.64

        //apply upgrades
        bulletDamage = initialBulletDamage + getDiminishingSum(0.3f, 0.9f, 0, statusLevels[(int)PlayerStatus.BulletPower]-1); //0 - 0.3 - 0.57 - 0.81 - 1.02
        bulletKnockback = initialBulletKnockback + getDiminishingSum(2f, 0.75f, 0, statusLevels[(int)PlayerStatus.BulletPower]-1); //0 - 2 - 3.5 - 4.6

        bulletSpeed = initialBulletSpeed + getDiminishingSum(10, 0.7f, 0, statusLevels[(int)PlayerStatus.BulletRange]-1); //0 - 10 - 17 - 23
        bulletRange = initialBulletRange + getDiminishingSum(5, 0.75f, 0, statusLevels[(int)PlayerStatus.BulletRange]-1); //0 - 5 - 8.75 - 11.5
        
        bulletsPerShoot = initialBulletsPerShoot + statusLevels[(int)PlayerStatus.BulletsPerShoot]; //1 - 2 - 3 - 4
        if (bulletsPerShoot > 1) bulletDamage = bulletDamage / (bulletsPerShoot * 0.75f); //Compensate for multiple bullets

        ShootRate = initialShootRate + getDiminishingSum(0.5F, 0.75f, 0, statusLevels[(int)PlayerStatus.ShootRate]-1); //0 - 0.5 - 0.875 - 1.15

        bulletPenetration = initialBulletPenetration + statusLevels[(int)PlayerStatus.Penetration]; //0 - 1 - 2 - 3

        // bulletDamage = initialBulletDamage + Mathf.Sqrt(statusLevels[(int)Status.BulletPower]*1) * 0.5f; //0 - 0.5 - 0.7 - 0.9
        // bulletKnockback = initialBulletKnockback + Mathf.Sqrt(statusLevels[(int)Status.BulletPower] * 9f); //0 - 3 - 4.2 - 5.2

        // bulletSpeed = initialBulletSpeed + Mathf.Sqrt(statusLevels[(int)Status.BulletRange] * 200f); //0 - 14 - 20
        // bulletRange = initialBulletRange + Mathf.Sqrt(statusLevels[(int)Status.BulletRange] * 25f); //0 - 5 - 7 - 9

        // bulletsPerShoot = initialBulletsPerShoot + statusLevels[(int)Status.BulletsPerShoot]; //1 - 2 - 3 - 4
        // if (bulletsPerShoot > 1) bulletDamage = bulletDamage / (bulletsPerShoot * 0.75f); //Compensate for multiple bullets

        // ShootRate = initialShootRate + Mathf.Sqrt(statusLevels[(int)Status.ShootRate] * 2) * 0.5f; //0 - 0.7 - 1 - 1.2
        
        // bulletPenetration = initialBulletPenetration + statusLevels[(int)Status.Penetration]; //0 - 1 - 2 - 3
    }

    public float getDiminishingSum(float value, float mult, int start, int end)
    {
        if (end < start) return 0f;

        float sum = 0;
        for (int i = start; i <= end; i++)
        {
            sum += value * Mathf.Pow(mult,i);
        }
        return sum;
    }
    

    #endregion

    #region //State machine methods
    public IPlayerState GetState() { return currentState; }
    public void SetState(IPlayerState state)
    {
        if (debug) Debug.Log(debugTag + "SetState [" + "" + "]");

        currentState?.ExitState(this);
        currentState = state;
        currentState.EnterState(this);
    }

    #endregion

    #region //Shooting methods
    public IEnumerator ShootCoroutine()
    {
        while (true)
        {
            Shoot();

            yield return new WaitForSeconds(shootCooldown);
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

        audioManager.PlaySound(bullet_controller.shootSound, true);
    }

    public void Shoot()
    {
        bool playedSound = false;

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

            if (!playedSound)
            {
                audioManager.PlaySound(bullet_controller.shootSound, true);
                playedSound = true;
            }
        }
    }

    #endregion
    
    #region //Input methods

    public float GetTouchX(Touch t)
    {
        if (t.phase == TouchPhase.Ended) touchBeganAtFloor = false;
        
        Ray ray = mainCamera.ScreenPointToRay(t.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayerMask))
        {
            touchBeganAtFloor = true;

            if (t.phase == TouchPhase.Ended) touchBeganAtFloor = false;

            //Return floor position
            return hit.point.x;
        }
        else if (touchBeganAtFloor) 
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

        return transform.position.x;
    }

    #endregion

    #region //Smooth movement and animation

    public void SmoothHorizontalMovement(float x_target)
    {
        float step = Mathf.Abs(transform.position.x - x_target) / (smoothMoveRatio / Time.deltaTime);
        
        xLookingAtDistance = -(transform.position.x - x_target);

        Vector3 target_pos = transform.position;
        target_pos.x = x_target;
        transform.position = Vector3.MoveTowards(transform.position, target_pos, step);
    }

    public void RiggingMovementAnimation(float target_x)
    {
        Vector3 tmp = feetRig.localRotation.eulerAngles; //Get the wheels local rotation
        float target_y = target_x * feetRotationMultiplier; //Calculate target rotation
        if (tmp.y >= 180) tmp.y -= 360; //Normalize rotation if it is to the left
        tmp.y += (target_y - tmp.y) / (feetRigRotationSmoothRatio / Time.deltaTime); //Smooth interpolation
        feetRig.localRotation = Quaternion.Euler(tmp); //Set local rotation

        //Repeat for head
        tmp = headRig.localRotation.eulerAngles;
        target_y = target_x * headRotationMultiplier;
        if (tmp.y >= 180) tmp.y -= 360;
        tmp.y += (target_y - tmp.y) / (headRigRotationSmoothRatio / Time.deltaTime);
        headRig.localRotation = Quaternion.Euler(tmp);

        //head wavey animation
        headRig.localPosition = headRigInitialPos + headWaveyInterpolator.Update(Time.deltaTime) * Vector3.up;
    }

    #endregion

    void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);

        shootingState = new ShootingState();
        upgradeState = new UpgradeState(levelManager);

        bulletPooler = new ObjectPooler(bulletPrefab);

        headRigInitialPos = headRig.localPosition; //Initial head position 

        //Sets the initial state
        shootingCoroutine = ShootCoroutine();
        SetState(shootingState);

        //Initializes the upgrade levels
        for (int i = 0; i < (int)PlayerStatus.Count; i++) statusLevels[i] = 0;
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

        if (stuckInPlace > 0)
        {
            transform.position = stuckInPlacePos;
            stuckInPlace -= Time.deltaTime;
        }

        xLookingAtDistance = 0;

        currentState.UpdateState(this);

        //UV Animation
        wheelUVOffset += wheelUVSpeed * Time.deltaTime;
        //wheelRenderer.materials[wheelMaterialIndex].SetTextureOffset("_MainTex", new Vector2(0,wheelUVOffset));
        wheelRenderer.materials[wheelMaterialIndex].SetFloat("_UVOffsetY", wheelUVOffset);

        RiggingMovementAnimation(xLookingAtDistance);
    }

    //on colliding
    void OnTriggerEnter(Collider other)
    {
        //checks if object implements ITakesDamage
        

        if (other.gameObject.tag == "Damages Player")
        {
            levelManager.GameOver();
        }
    }
}