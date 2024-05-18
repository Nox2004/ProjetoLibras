using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GasparController : CardboardEnemyController
{
    private enum States 
    {
        Patrol,
        TurningAround,
        TakingDamage,
        Charging
    }

    private States chargingState = States.Patrol;

    [Header("Charger Enemy Textures")]
    [SerializeField] private Texture patrolTexture;
    [SerializeField] private Texture turningAround;
    [SerializeField] private Texture takingDamageTexture;
    [SerializeField] private Texture chargingTexture;
    [SerializeField] private Texture deadTexture;

    private Texture lastTexture;

    [SerializeField] private float takeDamageTextureDuration = 0.5f;
    private float takeDamageTimer = 0f;

    [Header("Charging Properties")]
    private float runSpeed;
    [SerializeField] private float startRunSpeed;
    [SerializeField] private float endRunSpeed;
    private float zTurnPoint;
    [SerializeField] private float minZTurnPoint;
    [SerializeField] private float maxZTurnPoint;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float chargeDelayInUnits;
    [SerializeField] private Vector3 direction = Vector3.back;
    [SerializeField] private Vector3 targetDirection;
    [SerializeField] private Transform wheels;
    [SerializeField] private float wheelsRotationMultiplier;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] turningSounds;
    [SerializeField] private AudioClip[] chasingSounds;

    override protected void Start()
    {
        base.Start();

        runSpeed += speed;

        ChangePaperTexture(patrolTexture);
        lastTexture = patrolTexture;

        zTurnPoint = Random.Range(minZTurnPoint, maxZTurnPoint);
        runSpeed = Mathf.LerpUnclamped(startRunSpeed, endRunSpeed, difficultyValue);
    }
    
    override protected void Update()
    {
        if (paused) return;
        
        base.Update();

        if (state == EnemyState.Moving)
        {
            wheels.Rotate(Vector3.right, -speed * wheelsRotationMultiplier * Time.deltaTime);    

            switch (chargingState)
            {
                case States.Patrol:
                {
                    if (transform.position.z < zTurnPoint)
                    {
                        chargingState = States.TurningAround;

                        ChangePaperTexture(turningAround);
                        lastTexture = turningAround;

                        //facing the player
                        direction = Vector3.back;
                        targetDirection = (GameObject.Find("Player").transform.position - transform.position).normalized;

                        //play sound
                        audioManager.PlayRandomSound(turningSounds);
                    }
                }
                break;
                case States.TurningAround:
                {
                    direction = Vector3.RotateTowards(direction, targetDirection, turnSpeed * Time.deltaTime, 0.0f);
                    
                    Vector3 tmp = direction;
                    //tmp.z -= -1;
                    transform.rotation = Quaternion.LookRotation(tmp);
                    tmp = transform.rotation.eulerAngles;
                    tmp.y -= 180;
                    transform.rotation = Quaternion.Euler(tmp);

                    if (Vector3.Angle(direction, targetDirection) < 5)
                    {
                        chargeDelayInUnits -= speed * Time.deltaTime;
                        if (chargeDelayInUnits <= 0)
                        {
                            chargingState = States.Charging;
                            ChangePaperTexture(chargingTexture);
                            lastTexture = chargingTexture;

                            //play sound
                            audioManager.PlayRandomSound(chasingSounds);
                        }
                    }
                }
                break;
                case States.Charging:
                {
                    transform.position += direction * runSpeed * Time.deltaTime;

                    wheels.Rotate(Vector3.right, -runSpeed * wheelsRotationMultiplier * Time.deltaTime);
                }
                break;
            }
        }

        if (takeDamageTimer > 0)
        {
            takeDamageTimer -= Time.deltaTime;
            if (takeDamageTimer <= 0 && state != EnemyState.Dead)
            {
                ChangePaperTexture(lastTexture);
            }
        }
    }

    override public void TakeDamage(float damage, int pierce)
    {
        ChangePaperTexture(takingDamageTexture);
        takeDamageTimer = takeDamageTextureDuration;

        base.TakeDamage(damage, pierce);
    }

    override public void Die()
    {
        ChangePaperTexture(deadTexture);

        base.Die();
    }
}
