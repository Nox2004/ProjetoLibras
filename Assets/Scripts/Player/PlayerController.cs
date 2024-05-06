using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

//private enum Status { shootRate, Damage, Speed, Range, BulletsPerShoot, Knockback, Piercing, Count }
public enum PlayerUpgradeId { shootRate, BulletPower, BulletRange, BulletsPerShoot, Piercing, Aim, LotsOfDamage, LotsOfBullets, LotsOfshootRate, Drone, Count }

//Store possible upgrades, their levels and probability
[System.Serializable]
public class PlayerUpgrade
{
    public PlayerUpgradeId id;
    public int rarity;
    public int currentLevel;
    public int maxLevel;

    public Sprite icon;

    public int weight;

    public static PlayerUpgrade GetRandom(PlayerUpgrade[] list, PlayerUpgrade[] ignore = null)
    {
        //Creates a copy and remove the ignore
        List<PlayerUpgrade> listCopy = new List<PlayerUpgrade>(list);
        if (ignore != null)
        {
            foreach (PlayerUpgrade i in ignore)
            {
                listCopy.Remove(i);
            }
        }
        
        int totalWeight = 0;
        foreach (PlayerUpgrade upgrade in listCopy)
        {
            if (upgrade.currentLevel >= upgrade.maxLevel && upgrade.maxLevel != -1) continue; //Ignore upgrades that are at max level
            
            totalWeight += upgrade.weight/upgrade.rarity;
        } 

        int random = Random.Range(0, totalWeight);
        int count = 0;

        //Debug.Log("Total weight: " + totalWeight);
        //Debug.Log("Radom: " + random);

        foreach (PlayerUpgrade upgrade in listCopy)
        {
            if (upgrade.currentLevel >= upgrade.maxLevel && upgrade.maxLevel != -1) continue; //Ignore upgrades that are at max level
            
            if (upgrade.weight/upgrade.rarity == 0) continue;

            count += upgrade.weight/upgrade.rarity;

            //Debug.Log("Count of " + upgrade.id + ": " + count);

            if (random <= count) return upgrade;
        }
        
        Debug.LogError("Not able to choose a upgrade, returning the first one from the list.");
        return list[0];
    }
}

[System.Serializable]
public class PlayerStatus
{
    [Header("Shoot Rate")]
    public float initialshootRate;
    public float shootRateIncreasePerLevel;
    public float shootRateDiminishMultiplier;

    [Header("Bullet Power")]
    public float initialBulletDamage;
    public float bulletDamageIncreasePerLevel;
    public float bulletDamageDiminishMultiplier;

    public float initialBulletKnockback;
    public float bulletKnockbackIncreasePerLevel;
    public float bulletKnockbackDiminishMultiplier;

    [Header("Bullet Range")]
    public float initialBulletRange;
    public float bulletRangeIncreasePerLevel;
    public float bulletRangeDiminishMultiplier;

    public float initialBulletSpeed;
    public float bulletSpeedIncreasePerLevel;
    public float bulletSpeedDiminishMultiplier;

    [Header("Bullets Per Shoot")]
    public int initialBulletsPerShoot;
    public int bulletsPerShootIncreasePerLevel;
    [Tooltip("Lerp value to calculate fina damage 0 - 1 -> (damage / bullets) - damage")] [Range(0,1)] public float bulletQuantityDamageLerp;

    [Header("Piercing")]
    public int initialPiercing;
    public int piercingIncreasePerLevel;

    [Header("Aim")]
    public float initialAimImprecision;

    [Header("More Damage Less shot rate")]
    public float lotsOfDamageDamageMultiplier;
    public float lotsOfDamageshootRateMultiplier;

    [Header("More Bullets Less range")]
    public int lotsOfBulletsQuantityOfBulletsIncrease;
    public float lotsOfBulletsRangeMultiplier;

    [Header("More Shoot Rate Less knockback")]
    public float lotsOfshootRateshootRateMultiplier;
    public float lotsOfshootRateKnockbackMultiplier;

    [Header("Drone")]
    public GameObject dronePrefab;
    public List<PlayerDrone> drones;
    public Vector3 droneSpawnPosition;
}

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
    [SerializeField] public ParticleManager particleManager;
    [SerializeField] Camera mainCamera; 
    [SerializeField] private LayerMask floorLayerMask; // Layer mask for the floor
    private AudioManager audioManager;

    //Time
    public float time;
    
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
    private IEnumerator shootingCoroutine; public bool isShooting;
    [SerializeField] private Vector3 bulletSpawnOffset;
    [SerializeField] private float bulletZLimit;
    [SerializeField, Tooltip("The arc (in degrees) in which bullets will be instantiated if player shoots more than one.")] private float bulletsAngleArc;


    [Header("Shooting")]
    [SerializeField, Tooltip("Shoots per second")] public float shootRate;
    private float shootCooldown => 1f / shootRate; //Cooldown between shoots
    [SerializeField] public int bulletsPerShoot = 1;
    [SerializeField] public float bulletSpeed;
    [SerializeField] public float bulletRange;
    [SerializeField] public float bulletDamage;
    [SerializeField] public float bulletKnockback;
    [SerializeField] public int bulletPiercing;
    [SerializeField] public float aimImprecision;
    [SerializeField] public int quantityOfDrones;
    
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

    #region //Upgrades

    [SerializeField] public PlayerUpgrade[] upgrades = new PlayerUpgrade[(int)PlayerUpgradeId.Count];
    [SerializeField] private PlayerStatus playerStatus;

    public void Upgrade(PlayerUpgradeId upgradeId, int levelUp = 1)
    {
        upgrades[(int) upgradeId].currentLevel += levelUp;
        if (debug) Debug.Log(debugTag + "Upgraded " + upgradeId + " to level " + upgrades[(int) upgradeId].currentLevel);
        //upgradeStatusObject.ShowStatus((Status)random);

        UpdateStatus();

        //getDiminishingSum => f(I,L,M,C) = I+Sum(M * C^n,n,0,L-1)
        //initial + 0
        //initial + 0.5
        //initial + 0.5 * 0.8
        //initial + 0.5 * 0.8 + 0.5 * 0.64
    }

    public void UpdateStatus()
    {
        PlayerStatus _ps = playerStatus;

        //apply upgrades
        bulletDamage = _ps.initialBulletDamage + getDiminishingSum(_ps.bulletDamageIncreasePerLevel, _ps.bulletDamageDiminishMultiplier, 0, upgrades[(int)PlayerUpgradeId.BulletPower].currentLevel-1);
        bulletKnockback = _ps.initialBulletKnockback + getDiminishingSum(_ps.bulletKnockbackIncreasePerLevel, _ps.bulletKnockbackDiminishMultiplier, 0, upgrades[(int)PlayerUpgradeId.BulletPower].currentLevel-1);

        bulletSpeed = _ps.initialBulletSpeed + getDiminishingSum(_ps.bulletSpeedIncreasePerLevel, _ps.bulletSpeedDiminishMultiplier, 0, upgrades[(int)PlayerUpgradeId.BulletRange].currentLevel-1);
        bulletRange = _ps.initialBulletRange + getDiminishingSum(_ps.bulletRangeIncreasePerLevel, _ps.bulletRangeDiminishMultiplier, 0, upgrades[(int)PlayerUpgradeId.BulletRange].currentLevel-1);
        
        bulletsPerShoot = _ps.initialBulletsPerShoot + upgrades[(int)PlayerUpgradeId.BulletsPerShoot].currentLevel * _ps.bulletsPerShootIncreasePerLevel;
        
        if (upgrades[(int)PlayerUpgradeId.LotsOfBullets].currentLevel > 0)
        {
            bulletsPerShoot += _ps.lotsOfBulletsQuantityOfBulletsIncrease;
            bulletRange *= _ps.lotsOfBulletsRangeMultiplier;
        }
        
        if (bulletsPerShoot > 1) bulletDamage = Mathf.Lerp(bulletDamage / bulletsPerShoot,bulletDamage,_ps.bulletQuantityDamageLerp); //Compensate for multiple bullets

        shootRate = _ps.initialshootRate + getDiminishingSum(_ps.shootRateIncreasePerLevel, _ps.shootRateDiminishMultiplier, 0, upgrades[(int)PlayerUpgradeId.shootRate].currentLevel-1);

        bulletPiercing = _ps.initialPiercing + upgrades[(int)PlayerUpgradeId.Piercing].currentLevel * _ps.piercingIncreasePerLevel;

        if (upgrades[(int)PlayerUpgradeId.Aim].currentLevel == 0) aimImprecision = _ps.initialAimImprecision;
        else aimImprecision = 0f;

        if (upgrades[(int)PlayerUpgradeId.LotsOfDamage].currentLevel > 0)
        {
            bulletDamage *= _ps.lotsOfDamageDamageMultiplier;
            shootRate *= _ps.lotsOfDamageshootRateMultiplier;
        }

        if (upgrades[(int)PlayerUpgradeId.LotsOfshootRate].currentLevel > 0)
        {
            shootRate *= _ps.lotsOfshootRateshootRateMultiplier;
            bulletKnockback *= _ps.lotsOfshootRateKnockbackMultiplier;
        }

        //Drones
        if (upgrades[(int)PlayerUpgradeId.Drone].currentLevel > 0)
        {
            //Instantiate drone
            for (int i = quantityOfDrones; i < upgrades[(int)PlayerUpgradeId.Drone].currentLevel; i++)
            {
                GameObject droneObj = Instantiate(_ps.dronePrefab, _ps.droneSpawnPosition, Quaternion.identity);
                PlayerDrone drone = droneObj.GetComponent<PlayerDrone>();
                
                drone.player = this;
                drone.droneIndex = i;
                
                _ps.drones.Add(drone);
                quantityOfDrones++;
            }
        }
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

        float angle = -bulletsAngleArc / 2 + Random.Range(-aimImprecision/2, aimImprecision/2);
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
            bullet_controller.pierce = bulletPiercing;

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

        UpdateStatus();

        //Sets the initial state
        shootingCoroutine = ShootCoroutine();
        SetState(shootingState);
    }

    void Update()
    {
        if (paused) return;

        time += Time.deltaTime;

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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Damages Player")
        {
            levelManager.GameOver();
        }
    }
}