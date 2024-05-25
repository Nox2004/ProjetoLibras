using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//private enum Status { shootRate, Damage, Speed, Range, BulletsPerShoot, Knockback, Piercing, Count }
public enum PlayerUpgradeId { shootRate, BulletPower, BulletRange, BulletsPerShoot, Piercing, Drone, GatlingGun, Shootgun, Shield, AllUp, BetterDrones, ExplosiveBullets, Count }

//Store possible upgrades, their levels and probability
[System.Serializable]
public class PlayerUpgrade
{
    public PlayerUpgradeId id;
    public UpgradeTier tier;
    public int rarity;
    public int currentLevel;
    public int maxLevel;

    public TranslatableText description;

    public PlayerUpgradeId[] exclusiveWith;

    public Sprite icon;

    public int weight;

    public enum UpgradeTier
    {
        Any = -1,
        Common = 1,
        Rare = 2,
    }
}

[System.Serializable]
public class PlayerUpgradeManager
{
    [SerializeField] private bool debug; private string debugTag = "PlayerUpgradeManager: ";
    [SerializeField] private PlayerUpgrade[] currentUpgrades = new PlayerUpgrade[(int)PlayerUpgradeId.Count];

    [SerializeField] private int initialUpgradeWeight;
    [SerializeField] private int upgradeWeightIncrease;
    [SerializeField] private int upgradeWeightAfterShowed;
    [SerializeField] private int upgradeWeightAfterSelected;

    public int GetUpgradeLevel(PlayerUpgradeId id) => currentUpgrades[(int)id].currentLevel;

    public void InitializeUpgradeWeights()
    {
        if (debug) Debug.Log(debugTag + "InitializeUpgradeWeights");

        for (int i = 0; i < currentUpgrades.Length; i++)
        {
            if (debug) Debug.Log(debugTag + "Setting weight of " + currentUpgrades[i].id + " to " + initialUpgradeWeight);
            currentUpgrades[i].weight = initialUpgradeWeight;
        }
    }

    public PlayerUpgrade GetRandom(PlayerUpgrade[] ignore = null, PlayerUpgrade.UpgradeTier targetTier = PlayerUpgrade.UpgradeTier.Common)
    {
        string debug_str = "";

        if (debug) Debug.Log(debugTag + "Get Random Upgrade method started");

        if (debug) debug_str = debugTag + "Removing ignore upgrades from the list of options: [";

        //Creates a copy and remove the ignore
        List<PlayerUpgrade> listCopy = new List<PlayerUpgrade>(currentUpgrades);
        if (ignore != null)
        {
            foreach (PlayerUpgrade i in ignore)
            {
                if (debug) debug_str += "Removed " + i.id + ", ";
                listCopy.Remove(i);
            }
        }
        
        if (debug) { debug_str += "]"; Debug.Log(debug_str); }
        
        if (debug) debug_str = debugTag + "Calculating total weight - [";

        int totalWeight = 0;
        foreach (PlayerUpgrade upgrade in listCopy)
        {
            if (upgrade.tier != targetTier && targetTier != PlayerUpgrade.UpgradeTier.Any) continue; //Ignore upgrades from other tiers
            if (upgrade.currentLevel >= upgrade.maxLevel && upgrade.maxLevel != -1) continue; //Ignore upgrades that are at max level
            
            //ignore upgrades that are exclusive with the current ones
            bool jump = false;
            for (int i = 0; i < currentUpgrades.Length; i++)
            {
                if (currentUpgrades[i].currentLevel == 0) continue;

                for (int j = 0; j < currentUpgrades[i].exclusiveWith.Length; j++)
                {
                    if (currentUpgrades[i].exclusiveWith[j] == upgrade.id)
                    {
                        jump = true;
                        break;
                    }
                }

                if (jump) break;
            }
            if (jump) continue;

            if (debug) debug_str += upgrade.id + ": " + upgrade.weight/upgrade.rarity + " + ";
            
            totalWeight += upgrade.weight/upgrade.rarity;
        } 

        int random = Random.Range(0, totalWeight);
        int count = 0;

        if (debug) 
        { 
            debug_str += "]"; 
            Debug.Log(debug_str); 
            Debug.Log(debugTag + "Total weight: " + totalWeight); 
            Debug.Log(debugTag + "Radom number selected: " + random);
        }

        //Debug.Log("Total weight: " + totalWeight);
        //Debug.Log("Radom: " + random);

        foreach (PlayerUpgrade upgrade in listCopy)
        {
            if (upgrade.tier != targetTier && targetTier != PlayerUpgrade.UpgradeTier.Any) continue; //Ignore upgrades from other tiers
            if (upgrade.currentLevel >= upgrade.maxLevel && upgrade.maxLevel != -1) continue; //Ignore upgrades that are at max level

            //ignore upgrades that are exclusive with the current ones
            bool jump = false;
            for (int i = 0; i < currentUpgrades.Length; i++)
            {
                if (currentUpgrades[i].currentLevel == 0) continue;

                for (int j = 0; j < currentUpgrades[i].exclusiveWith.Length; j++)
                {
                    if (currentUpgrades[i].exclusiveWith[j] == upgrade.id)
                    {
                        jump = true;
                        break;
                    }
                }

                if (jump) break;
            }

            if (jump) continue;

            count += upgrade.weight/upgrade.rarity;

            //Debug.Log("Count of " + upgrade.id + ": " + count);

            if (random <= count) 
            {
                return upgrade;
            }
        }
        
        Debug.LogError("Not able to choose a upgrade, returning the first one from the list.");
        return currentUpgrades[0];
    }

    public PlayerUpgrade[] UpgradeSelection(int numOfSelectedUpgrades, PlayerUpgrade.UpgradeTier targetTier = PlayerUpgrade.UpgradeTier.Common)
    {
        PlayerUpgrade[] possibleUpgrades = new PlayerUpgrade[numOfSelectedUpgrades];

        if (debug) Debug.Log(debugTag + "Start upgrade selection");

        //Choose the upgrades
        for (int i = 0; i < numOfSelectedUpgrades; i++)
        {
            possibleUpgrades[i] = GetRandom(possibleUpgrades, targetTier);
            possibleUpgrades[i].weight = upgradeWeightAfterShowed;
        }

        if (debug) 
        {
            Debug.Log(debugTag + "Selection Finished:");
            for (int i = 0; i < possibleUpgrades.Length; i++)
            {
                Debug.Log(debugTag + "Upgrade " + i + ": " + possibleUpgrades[i].id);
            }
        }

        //Increase the weight of the upgrades that were not shown
        for (int i = 0; i < currentUpgrades.Length; i++)
        {
            bool shown = false;
            for (int j = 0; j < numOfSelectedUpgrades; j++)
            {
                if (currentUpgrades[i].id == possibleUpgrades[j].id) 
                {
                    if (debug) Debug.Log(debugTag + "Upgrade " + currentUpgrades[i].id + " was just shown in selection, so its weight will not be increased.");
                    shown = true; break;
                }
            }
            if (shown) continue;

            if (debug) Debug.Log(debugTag + "Increasing weight of " + currentUpgrades[i].id + " to " + currentUpgrades[i].weight + " + " + upgradeWeightIncrease);

            currentUpgrades[i].weight += upgradeWeightIncrease;
        }
        
        return possibleUpgrades;
    }

    public void SelectUpgrade(PlayerUpgradeId id, int levelUp)
    {
        currentUpgrades[(int)id].weight = upgradeWeightAfterSelected;
        currentUpgrades[(int)id].currentLevel += levelUp;
    }

    public void ResetUpgradeLevel(PlayerUpgradeId id, int levelDown = -1)
    {
        //currentUpgrades[(int)id].weight = upgradeWeightAfterSelected;
        if (levelDown == -1)currentUpgrades[(int)id].currentLevel = 0;
        else currentUpgrades[(int)id].currentLevel -= levelDown;
    }
}

[System.Serializable]
public class PlayerStatusManager
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

    [Header("Drone")]
    public GameObject dronePrefab;
    public List<PlayerDrone> drones;
    public Vector3 droneSpawnPosition;

    [Header("Gatling Gun")]
    public float gatlingGunShootRateMultiplier;
    public float gatlingGunBulletDamageMultiplier;
    public float gatlingGunBulletKnockbackMultiplier;
    public float gatlingGunImprecisionLevel;

    [Header("ShootGun")]
    public int shootgunExtraBullets;
    public float shootgunRangeMultiplier;
    public float shootgunShootRateMultiplier;
    public float ShootgunBulletKnockbackMultiplier;
    public float ShootgunBulletDamageMultiplier;
    public float shootgunExtraBulletsAngleArc;
    public float shootgunImprecisionLevel;
    
    [Header("AllUp")]
    public float allStatusUpMultiplier;

    [Header("Shield")]
    public int defense;
    public int iFramesAfterUsingShield;
    public PlayerShield shield;
    
    [Header("Better Drones")]
    public float betterDronesOutputDamageRatio = 2f;
    public float betterDronesShootRate = 1.5f;
    public float betterDronesHoverSpeedMultiplier = 2f;
    public int betterDronesPiercing = 1;


    // [Header("Explosive Bullets")]
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
    private Renderer[] myRenderers;

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
    private float shootTimer = 0;
    [SerializeField, Tooltip("Shoots per second")] public float shootRate;
    private float shootCooldown => 1f / shootRate; //Cooldown between shoots
    [SerializeField] private int bulletsPerShoot = 1;
    [SerializeField] private float bulletSpeed;
    [SerializeField] public float bulletRange;
    [SerializeField] private float bulletDamage;
    [SerializeField] private float bulletKnockback;
    [SerializeField] private int bulletPiercing;
    [SerializeField] private float aimImprecision;
    [SerializeField] private int quantityOfDrones;
    [SerializeField] private bool hasShield;
    [SerializeField] private int defense;
    
    //State machine properties
    public IPlayerState currentState;
    public ShootingState shootingState; public UpgradeState upgradeState; public DeadState deadState;

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

    private float _unableToShootTimer = 0f;
    [HideInInspector] public float unableToShootTimer
    {
        get => _unableToShootTimer;
        set
        {
            _unableToShootTimer = value;
            if (value > 0f)
            {
                unableToShootParticle.SetActive(true);
            }
            else
            {
                unableToShootParticle.SetActive(false);
            }
        }
    }
    [SerializeField] private GameObject unableToShootParticle;

    [Header("Death")]
    [SerializeField] private GameObject aliveObject;
    [SerializeField] private GameObject deadObject;
    [SerializeField] private GameObject deathSmokeParticle;
    private float iFrames = 0;
    [SerializeField] private float iFramesInvisibilityDuration; 
    [SerializeField] private float iFramesWhenRevived;
    private IPlayerState stateBeforeDeath;

    #region //Upgrades

    [SerializeField] public PlayerUpgradeManager upgradeManager;
    [SerializeField] private PlayerStatusManager playerStatusManager;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] normalShootSounds;
    [SerializeField] private AudioClip[] gatlingGunSounds;
    [SerializeField] private AudioClip[] shootgunSounds;
    [SerializeField] private AudioClip deathSound;

    public void Upgrade(PlayerUpgradeId upgradeId, int levelUp = 1)
    {
        upgradeManager.SelectUpgrade(upgradeId, levelUp);
        
        if (debug) Debug.Log(debugTag + "Upgraded " + upgradeId + " to level " + upgradeManager.GetUpgradeLevel(upgradeId));

        UpdateStatus(upgradeId);
    }

    public void UpdateStatus(PlayerUpgradeId updatedUpgrade = default)
    {
        PlayerStatusManager _ps = playerStatusManager;

        //apply upgrades
        bulletDamage = _ps.initialBulletDamage + getDiminishingSum(_ps.bulletDamageIncreasePerLevel, _ps.bulletDamageDiminishMultiplier, 0, upgradeManager.GetUpgradeLevel(PlayerUpgradeId.BulletPower)-1);
        bulletKnockback = _ps.initialBulletKnockback + getDiminishingSum(_ps.bulletKnockbackIncreasePerLevel, _ps.bulletKnockbackDiminishMultiplier, 0, upgradeManager.GetUpgradeLevel(PlayerUpgradeId.BulletPower)-1);

        bulletSpeed = _ps.initialBulletSpeed + getDiminishingSum(_ps.bulletSpeedIncreasePerLevel, _ps.bulletSpeedDiminishMultiplier, 0, upgradeManager.GetUpgradeLevel(PlayerUpgradeId.BulletRange)-1);
        bulletRange = _ps.initialBulletRange + getDiminishingSum(_ps.bulletRangeIncreasePerLevel, _ps.bulletRangeDiminishMultiplier, 0, upgradeManager.GetUpgradeLevel(PlayerUpgradeId.BulletRange)-1);
        
        bulletsPerShoot = _ps.initialBulletsPerShoot + upgradeManager.GetUpgradeLevel(PlayerUpgradeId.BulletsPerShoot) * _ps.bulletsPerShootIncreasePerLevel;
        if (bulletsPerShoot > 1) 
        {
            bulletDamage = Mathf.Lerp(bulletDamage / bulletsPerShoot,bulletDamage,_ps.bulletQuantityDamageLerp); //Compensate for multiple bullets
            bulletKnockback = Mathf.Lerp(bulletKnockback / bulletsPerShoot,bulletKnockback,_ps.bulletQuantityDamageLerp); //Compensate for multiple bullets
        }

        shootRate = _ps.initialshootRate + getDiminishingSum(_ps.shootRateIncreasePerLevel, _ps.shootRateDiminishMultiplier, 0, upgradeManager.GetUpgradeLevel(PlayerUpgradeId.shootRate)-1);

        bulletPiercing = _ps.initialPiercing + upgradeManager.GetUpgradeLevel(PlayerUpgradeId.Piercing) * _ps.piercingIncreasePerLevel;
        
        //All Up
        if (upgradeManager.GetUpgradeLevel(PlayerUpgradeId.AllUp) > 0)
        {
            shootRate += upgradeManager.GetUpgradeLevel(PlayerUpgradeId.AllUp) * _ps.allStatusUpMultiplier * _ps.shootRateIncreasePerLevel;
            bulletDamage += upgradeManager.GetUpgradeLevel(PlayerUpgradeId.AllUp) * _ps.allStatusUpMultiplier * _ps.bulletDamageIncreasePerLevel;
            bulletKnockback += upgradeManager.GetUpgradeLevel(PlayerUpgradeId.AllUp) * _ps.allStatusUpMultiplier * _ps.bulletKnockbackIncreasePerLevel;
            bulletSpeed += upgradeManager.GetUpgradeLevel(PlayerUpgradeId.AllUp) * _ps.allStatusUpMultiplier * _ps.bulletSpeedIncreasePerLevel;
            bulletRange += upgradeManager.GetUpgradeLevel(PlayerUpgradeId.AllUp) * _ps.allStatusUpMultiplier * _ps.bulletRangeIncreasePerLevel;
            bulletPiercing += upgradeManager.GetUpgradeLevel(PlayerUpgradeId.AllUp) * _ps.piercingIncreasePerLevel;
        }

        //Drones
        if (upgradeManager.GetUpgradeLevel(PlayerUpgradeId.Drone) > 0)
        {
            //Instantiate new drones
            for (int i = quantityOfDrones; i < upgradeManager.GetUpgradeLevel(PlayerUpgradeId.Drone); i++)
            {
                GameObject droneObj = Instantiate(_ps.dronePrefab, _ps.droneSpawnPosition, Quaternion.identity);
                PlayerDrone drone = droneObj.GetComponent<PlayerDrone>();
                
                drone.player = this;
                drone.droneIndex = i;
                
                _ps.drones.Add(drone);
                quantityOfDrones++;
            }

            //Update drones
            for (int i = 0; i < quantityOfDrones; i++)
            {
                PlayerDrone drone = _ps.drones[i];

                drone.quantityOfDrones = quantityOfDrones;
                drone.playerDamage = bulletDamage;
                drone.playerNumOfBullets = bulletsPerShoot;
                drone.playerShootRate = shootRate;

                if (upgradeManager.GetUpgradeLevel(PlayerUpgradeId.BetterDrones) > 0)
                {
                    drone.outputDamageRatio = _ps.betterDronesOutputDamageRatio;
                    drone.shootRate = _ps.betterDronesShootRate;
                    drone.piercing = _ps.betterDronesPiercing;
                    drone.hoverSpeedMultiplier = _ps.betterDronesHoverSpeedMultiplier;
                }
            }
        }

        //Gatling Gun
        if (upgradeManager.GetUpgradeLevel(PlayerUpgradeId.GatlingGun) > 0)
        {
            shootRate *= bulletsPerShoot;
            bulletsPerShoot = 1;

            shootRate *= _ps.gatlingGunShootRateMultiplier;
            bulletDamage *= _ps.gatlingGunBulletDamageMultiplier;
            bulletKnockback *= _ps.gatlingGunBulletKnockbackMultiplier;
            aimImprecision = _ps.gatlingGunImprecisionLevel;
        }

        //Shootgun
        if (upgradeManager.GetUpgradeLevel(PlayerUpgradeId.Shootgun) > 0)
        {
            bulletsPerShoot += _ps.shootgunExtraBullets;
            bulletDamage = Mathf.Lerp(bulletDamage / bulletsPerShoot,bulletDamage,_ps.bulletQuantityDamageLerp); //Compensate for multiple bullets
            bulletKnockback = Mathf.Lerp(bulletKnockback / bulletsPerShoot,bulletKnockback,_ps.bulletQuantityDamageLerp); //Compensate for multiple bullets
            bulletsAngleArc = _ps.shootgunExtraBulletsAngleArc;
            bulletRange *= _ps.shootgunRangeMultiplier;
            shootRate *= _ps.shootgunShootRateMultiplier;
            bulletKnockback *= _ps.ShootgunBulletKnockbackMultiplier;
            bulletDamage *= _ps.ShootgunBulletDamageMultiplier;
            aimImprecision = _ps.shootgunImprecisionLevel;
        }

        if (updatedUpgrade == PlayerUpgradeId.Shield)
        {
            hasShield = true;
            defense = upgradeManager.GetUpgradeLevel(PlayerUpgradeId.Shield)*_ps.defense;
            _ps.shield.Activate(true);
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
        if (debug) Debug.Log(debugTag + "SetState [" + state.ToString() + "]");

        currentState?.ExitState(this);
        currentState = state;
        currentState.EnterState(this);
    }

    #endregion

    #region //Shooting methods

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

            isShooting = true;
        }
        else
        {
            if (!isShooting) 
            {
                if (debug) Debug.LogError(debugTag + "Error - Player is not shooting");
                return;
            }

            isShooting = false;
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
        float angle = -bulletsAngleArc / 2;
        for (int i = 0; i < bulletsPerShoot; i++)
        {
            angle += bulletsAngleArc / (bulletsPerShoot + 1) + Random.Range(-aimImprecision/2, aimImprecision/2);
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
        }

        if (upgradeManager.GetUpgradeLevel(PlayerUpgradeId.GatlingGun) > 0)
        {
            audioManager.PlayRandomSound(gatlingGunSounds);
        }
        else if (upgradeManager.GetUpgradeLevel(PlayerUpgradeId.Shootgun) > 0)
        {
            audioManager.PlayRandomSound(shootgunSounds);
        }
        else
        {
            audioManager.PlayRandomSound(normalShootSounds, true);
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

    public void OnSubstar()
    {

    }

    public void OnStar()
    {
        if (hasShield)
        {
            defense = playerStatusManager.defense;
            playerStatusManager.shield.Activate(true);
        }
    }
    
    void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);

        shootingState = new ShootingState();
        upgradeState = new UpgradeState(levelManager);
        deadState = new DeadState();

        bulletPooler = new ObjectPooler(bulletPrefab);

        headRigInitialPos = headRig.localPosition; //Initial head position 

        upgradeManager.InitializeUpgradeWeights();
        UpdateStatus();

        //Sets the initial state
        SetState(shootingState);

        myRenderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (paused) return;
        
        //invencibility frames
        if (iFrames > 0)
        {
            iFrames -= Time.deltaTime;
            
            for (int i = 0; i < myRenderers.Length; i++)
            {
                myRenderers[i].enabled = (iFrames % iFramesInvisibilityDuration * 2) < iFramesInvisibilityDuration;
            }

            if (iFrames <= 0)
            {
                for (int i = 0; i < myRenderers.Length; i++)
                {
                    myRenderers[i].enabled = true;
                }
            }
        }

        if (unableToShootTimer > 0)
        {
            unableToShootTimer -= Time.deltaTime;
        }

        time += Time.deltaTime;

        if (stuckInPlace > 0)
        {
            transform.position = stuckInPlacePos;
            stuckInPlace -= Time.deltaTime;
        }

        xLookingAtDistance = 0;

        currentState.UpdateState(this);

        if (isShooting && unableToShootTimer <= 0)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                Shoot();
                shootTimer = shootCooldown;
            }
        }

        //UV Animation
        wheelUVOffset += wheelUVSpeed * levelManager.objectsSpeed * Time.deltaTime;
        //wheelRenderer.materials[wheelMaterialIndex].SetTextureOffset("_MainTex", new Vector2(0,wheelUVOffset));
        wheelRenderer.materials[wheelMaterialIndex].SetFloat("_UVOffsetY", wheelUVOffset);

        RiggingMovementAnimation(xLookingAtDistance);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Damages Player")
        {
            Die(other.gameObject);
        }
    }

    public void Die(GameObject killer)
    {
        if (iFrames > 0 || currentState == deadState) return;

        particleManager.EmitExplosion(transform.position, 16, deathSmokeParticle);

        if (defense > 0)
        {
            defense--;
            iFrames = playerStatusManager.iFramesAfterUsingShield;
            
            if (defense == 0)
            {
                playerStatusManager.shield.Activate(false);
            }

            if (killer.GetComponent<EnemyController>() != null)
            {
                killer.GetComponent<EnemyController>().Die();
            }
            else
            {
                killer.SetActive(false);
            }
            return;
        }

        if (killer.GetComponent<EnemyController>() != null)
        {
            //apply knockback so it seems like the objects collided
            killer.GetComponent<EnemyController>().KnockbackForce = Vector3.forward * 5;
        }

        aliveObject.SetActive(false);
        deadObject.SetActive(true);

        stateBeforeDeath = currentState;
        currentState = deadState;
        levelManager.GameOver();

        audioManager.PlaySound(deathSound);
    }

    public void Revive()
    {
        particleManager.EmitExplosion(transform.position, 16, deathSmokeParticle);
        
        aliveObject.SetActive(true);
        deadObject.SetActive(false);

        SetState(stateBeforeDeath);

        transform.position = new Vector3(xStart, transform.position.y, transform.position.z);
        iFrames = iFramesWhenRevived;
    }
}