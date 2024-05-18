using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerDrone : MonoBehaviour, IPausable
{
    #region //IPausable implementation

    private bool paused = false;
    private bool wasShootingBeforePause = false;
    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion
    
    private AudioManager audioManager;

    public PlayerController player;
    public int droneIndex;

    [SerializeField] private float distanceFromPlayer;
    [SerializeField] private float hoverAngularSpeed;
    public float hoverSpeedMultiplier = 1f;
    [SerializeField] private Vector3 offsetPos;

    [SerializeField] float movementSmoothnessRatio;

    [SerializeField] private Transform propeller;
    [SerializeField] private float propellerSpeed;

    private float currentDirection = 0;
    private Vector3 currentPosition;

    [SerializeField] private WaveValueInterpolator yWave;

    private ObjectPooler bulletPooler;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Vector3 bulletOffset;
    [SerializeField] public float outputDamageRatio = 3;
    [SerializeField] public float shootRate = 1;
    private float shootingCooldown => 1 / shootRate;
    private float shootTimer = 0;

    [SerializeField] public int piercing = 0;
    
    [HideInInspector] public int quantityOfDrones;
    [HideInInspector] public float playerDamage, playerNumOfBullets, playerShootRate;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    
    void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);
        bulletPooler = new ObjectPooler(bulletPrefab);
        currentPosition = transform.position;   
    }

    void LateUpdate()
    {
        if (paused) return;

        if (player.isShooting)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootingCooldown)
            {
                Shoot();
                shootTimer = 0;
            }
        }

        //Gets the angle of the drone in relation to the player
        float targetAngle = 360 / quantityOfDrones * droneIndex;
        
        //Smoothly rotates the drone to the target angle
        currentDirection += (targetAngle - currentDirection) / (movementSmoothnessRatio / Time.deltaTime);

        //Calculates the target position of the drone
        Vector3 targetPosition = player.transform.position + offsetPos;
        targetPosition += Quaternion.Euler(0, currentDirection + (player.time * hoverAngularSpeed * hoverSpeedMultiplier), 0) * Vector3.forward * distanceFromPlayer;

        //Smoothly moves the drone to the target position
        currentPosition += (targetPosition - currentPosition) / (movementSmoothnessRatio / Time.deltaTime);

        //Updates wavey movement offset
        yWave.Update(Time.deltaTime);

        //Calculates the final position of the drone
        transform.position = currentPosition + yWave.GetValue() * Vector3.up;

        //Rotates the propeller
        propeller.Rotate(Vector3.forward, propellerSpeed * Time.deltaTime);
    }

    private void Shoot()
    {
        //player damage output per shoot
        float dpsTarget = (playerDamage * playerNumOfBullets * playerShootRate) / outputDamageRatio;
        float dmg = dpsTarget / shootRate;

        GameObject bullet = bulletPooler.GetObject(transform.position + bulletOffset, Quaternion.Euler(0,0,0));
        PlayerBulletController bullet_controller = bullet.GetComponent<PlayerBulletController>();

        //Populate bullet properties
        bullet_controller.zLimit = 1000;
        bullet_controller.particleManager = player.particleManager;
        bullet_controller.playerAudioManager = audioManager;
        bullet_controller.speed = 10f;
        bullet_controller.damage = dmg;
        bullet_controller.range = 10f;
        bullet_controller.knockback = 0;
        bullet_controller.pierce = piercing;

        audioManager.PlaySound(shootSound, true);
    }
}
