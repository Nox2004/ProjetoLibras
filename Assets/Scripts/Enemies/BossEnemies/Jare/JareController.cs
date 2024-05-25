using System.Collections;
using UnityEngine;

public class JareController : BossController
{
    private enum JareAttacks
    {
        RandomCharge,
        GuidedCharge,
        Shooting1,
        Shooting2
        //Spawning
    }

    [SerializeField] private JareAttacks attackState;

    [SerializeField] float targetZ = 11f;
    private bool intro = true;

    private Vector3 initialRotation;
    private Vector3 targetRotation;
    [SerializeField] private float rotationSmoothRatio;

    [Header("Procedural Animation")]
    [SerializeField] private Transform mouth;
    [SerializeField] private float maxOpenMouthAngle = 50;
    private float mouthAngle;
    private float mouthInitialAngle;
    private Vector3 mouthInitialPosition;

    [Header("Idle behaviour")]
    [SerializeField] private float horizontalMovementWaveySpeed = 2f;
    [SerializeField] private float horizontalMovementAngleIncrease= 10f;

    [Header("Dead behaviour")]
    [SerializeField] private float deadSpeed = 5f;
    [SerializeField] private float deadMouthShaking = 0.3f;
    [SerializeField] private GameObject deadParticlesPrefab;
    [SerializeField] private Vector3 deadParticlesOffset;
    [SerializeField] private float deadParticlesSpawnTime;

    [Header("Attacking")]
    [SerializeField] private float attackTimerMax, attackTimerMin;
    private float attackTimer;
    private bool attacking;

    [Header("Random Charge")]
    [SerializeField] private float randomChargingPrepareTime;
    [SerializeField] private float randomChargingPrepareSpeed;
    [SerializeField] private float randomChargeSpeed;
    [SerializeField] private float returnFromChargeSpeed;
    [SerializeField] private GameObject chargeTarget;

    [Header("Guided Charge")]
    [SerializeField] private float guidedChargePrepareTime;
    [SerializeField] private float guidedChargeSpeed;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject projectileParticlesPrefab;
    [SerializeField] private Vector3 projectileOffset;

    [Header("Shooting attack one")]
    [SerializeField] private float shooting1PrepareTime;
    [SerializeField] private float shooting1Delay;
    [SerializeField] private int shooting1MinShoots;
    [SerializeField] private int shooting1MaxShoots;
    [SerializeField] private int shooting1Angle;

    [Header("Shooting attack two")]
    [SerializeField] private float shooting2HorizontalMovementSmoothRatio;
    [SerializeField] private float shooting2PrepareTime;
    [SerializeField] private float shooting2Delay;
    [SerializeField] private int shooting2MinShoots;    
    [SerializeField] private int shooting2MaxShoots;
    [SerializeField] private int shooting2Angle;


    private PlayerController player;

    override protected void Start()
    {
        attackTimer = Random.Range(attackTimerMin, attackTimerMax);

        base.Start();

        player = levelManager.PlayerController;
        initialRotation = transform.eulerAngles;

        mouthInitialAngle = mouth.localRotation.x;
        mouthInitialPosition = mouth.localPosition;

        targetZ = Mathf.Min(targetZ, player.transform.position.z + player.bulletRange + 2f);
        chargeTarget.SetActive(false);
    }
    
    override protected void Update()
    {
        if(paused) return;

        base.Update();
        
        if (intro)
        {
            if (transform.position.z > targetZ)
            {
                transform.position += Vector3.back * speed * Time.deltaTime;
                WaveMouth();
            }
            else
            {
                intro = false;
                StartCoroutine(IdleBehaviour());
            }
        }

        Vector3 realTargetRotation = (initialRotation + targetRotation);
        transform.eulerAngles += (realTargetRotation - transform.eulerAngles) / (rotationSmoothRatio / Time.deltaTime);

        //Vector3.Magnitude(realTargetRotation - transform.eulerAngles) / (rotationSmoothRatio / Time.deltaTime)
        //transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, realTargetRotation, 10f*Time.deltaTime, 0f);

        Vector3 tmp = mouth.localRotation.eulerAngles;
        tmp.x = mouthInitialAngle + mouthAngle;
        mouth.localRotation = Quaternion.Euler(tmp);
    }

    private void WaveMouth()
    {
        mouthAngle = (Mathf.Sin(Time.time*3f)+1f)/2f * 7.5f;
    }

    private void OpenMouth()
    {
        mouthAngle += (maxOpenMouthAngle-mouthAngle) / (0.25f / Time.deltaTime);
    }

    private void CloseMouth()
    {
        mouthAngle *= Mathf.Pow(0.005f, Time.deltaTime);
    }

    public override void TakeDamage(float damage, int pierce)
    {
        base.TakeDamage(damage, pierce);   
    }

    override public void Die()
    {
        base.Die();
    }

    #region Attack States
    private IEnumerator IdleBehaviour()
    {
        //prepare wavey movement
        float moveRange = maxX - minX;
        Vector3 startPos = transform.position;
        float xOffset = startPos.x - (maxX + minX)/2f;

        float waveyTimer = 0f;
        float timerOffset = (xOffset / (moveRange)) * Mathf.PI;

        while (true)
        {
            while (paused) yield return null;

            WaveMouth();

            if (state == EnemyState.Dead)
            {
                StartCoroutine(DeadBehaviour());
                break;
            }

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;

                waveyTimer += Time.deltaTime * horizontalMovementWaveySpeed;

                float lastX = transform.position.x;
                transform.position = startPos + Vector3.right * ((Mathf.Sin(waveyTimer+timerOffset) * moveRange/2f) - xOffset);

                targetRotation = Vector3.up * (lastX-transform.position.x)/Time.deltaTime * horizontalMovementAngleIncrease;

                yield return null;
            }
            else
            {
                //chooses an attack
                int randomAttack = Random.Range(0, JareAttacks.GetValues(typeof(JareAttacks)).Length);
                attackState = (JareAttacks) randomAttack;

                IEnumerator coroutine = null;

                switch (attackState)
                {
                    case JareAttacks.RandomCharge:
                        coroutine = RandomCharge();
                        break;
                    case JareAttacks.GuidedCharge:
                        coroutine = GuidedCharge();
                        break;
                    case JareAttacks.Shooting1:
                        coroutine = Shooting1();
                        break;
                    case JareAttacks.Shooting2:
                        coroutine = Shooting2();
                        break;
                }

                if (coroutine != null) 
                {
                    StartCoroutine(coroutine);
                    attackTimer = Random.Range(attackTimerMin, attackTimerMax);
                    attacking = true;

                    break;
                }
            }

            yield return null;
        }
    } 

    private IEnumerator DeadBehaviour()
    {
        targetRotation = Vector3.zero;

        float particleTimer = 0f;

        while (true)
        {
            while (paused) yield return null;
            
            particleTimer -= Time.deltaTime;
            if (particleTimer < 0f)
            {
                particleManager.EmitExplosion(transform.position+deadParticlesOffset, 10, deadParticlesPrefab);
                particleTimer = deadParticlesSpawnTime;
            }

            mouth.transform.localPosition = mouthInitialPosition + new Vector3(Random.Range(-deadMouthShaking, deadMouthShaking),
            Random.Range(-deadMouthShaking, deadMouthShaking),
            Random.Range(-deadMouthShaking, deadMouthShaking));

            transform.Translate(Vector3.back * deadSpeed * Time.deltaTime);

            if (transform.position.z > spawnPosition.z)
            {
                Destroy(gameObject);
            }

            yield return null;
        }
    }

    private IEnumerator RandomCharge()
    {
        targetRotation = Vector3.zero;

        Vector3 startPos = transform.position;

        float timer = 0;

        chargeTarget.SetActive(true);

        while (timer < randomChargingPrepareTime)
        {
            while (paused) yield return null;

            transform.Translate(Vector3.back * randomChargingPrepareSpeed * Time.deltaTime);
            timer += Time.deltaTime;

            //opens mouth
            OpenMouth();

            yield return null;
        }

        chargeTarget.SetActive(false);

        Vector3 targetPos = new Vector3(transform.position.x,transform.position.y,player.transform.position.z);

        //charging towards player
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            while (paused) yield return null;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, randomChargeSpeed * Time.deltaTime);
            CloseMouth();

            yield return null;
        }

        //going back
        while (Vector3.Distance(transform.position, startPos) > 0.1f)
        {
            while(paused) yield return null;

            transform.position = Vector3.MoveTowards(transform.position, startPos, returnFromChargeSpeed * Time.deltaTime);

            WaveMouth();

            yield return null;
        }

        StartCoroutine(IdleBehaviour());
    }

    private IEnumerator GuidedCharge()
    {   
        Vector3 startPos = transform.position;
        Vector3 targetPos = player.transform.position;
        
        float timer = guidedChargePrepareTime/2;

        chargeTarget.SetActive(true);

        while (timer > 0f)
        {
            while (paused) yield return null;
            
            targetPos = player.transform.position;
            targetPos.x = Mathf.Clamp(targetPos.x,minX,maxX);
            
            Vector3 posDiff = targetPos - transform.position;
            targetRotation = Quaternion.LookRotation(new Vector3(posDiff.x,0f,posDiff.z)).eulerAngles - initialRotation;

            timer -= Time.deltaTime;

            OpenMouth();

            yield return null;
        }

        timer = guidedChargePrepareTime/2;
        while (timer > 0f)
        {
            while (paused) yield return null;
            
            timer -= Time.deltaTime;

            yield return null;
        }

        chargeTarget.SetActive(false);

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            while(paused) yield return null;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, guidedChargeSpeed * Time.deltaTime);

            CloseMouth();

            yield return null;
        }

        while (Vector3.Distance(transform.position, startPos) > 0.1f)
        {
            while(paused) yield return null;

            transform.position = Vector3.MoveTowards(transform.position, startPos, returnFromChargeSpeed * Time.deltaTime);

            WaveMouth();

            yield return null;
        }
        
        transform.eulerAngles = new Vector3(0f,180f,0f);

        StartCoroutine(IdleBehaviour());
    }

    private IEnumerator Shooting1()
    {
        targetRotation = Vector3.zero;

        float timer = shooting1PrepareTime;
        while (timer > 0)
        {
            while (paused) yield return null;

            timer -= Time.deltaTime;

            //opens mouth
            OpenMouth();

            yield return null;
        }

        int shoots = Random.Range(shooting1MinShoots, shooting1MaxShoots);
        timer = shooting1Delay;

        if (shoots % 2 == 0)
        {
            targetRotation = Vector3.up * shooting1Angle;
        }
        else
        {
            targetRotation = Vector3.up * -shooting1Angle;
        }

        while (shoots > 0)
        {
            while (paused) yield return null;

            timer -= Time.deltaTime;

            if (timer < 0)
            {
                Shoot();

                if (shoots % 2 == 0)
                {
                    targetRotation = Vector3.up * shooting1Angle/2;
                }
                else
                {
                    targetRotation = Vector3.up * -shooting1Angle/2;
                }

                timer = shooting1Delay;
                shoots--;
            }

            yield return null;
        }

        while (mouthAngle > 0.1f)
        {
            while (paused) yield return null;

            CloseMouth();

            yield return null;
        }

        StartCoroutine(IdleBehaviour());
    }

    private IEnumerator Shooting2()
    {
        //goes to the middle
        float targetX = (maxX + minX) / 2f;

        while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
        {
            while (paused) yield return null;

            float lastX = transform.position.x;
            Vector3 tmp = transform.position;
            tmp.x += (targetX - tmp.x) / (shooting2HorizontalMovementSmoothRatio / Time.deltaTime);
            transform.position = tmp;
            
            targetRotation = Vector3.up * (lastX - transform.position.x) * horizontalMovementAngleIncrease;

            yield return null;
        }

        targetRotation = Vector3.zero;

        float timer = shooting2PrepareTime;

        while (timer > 0)
        {
            while (paused) yield return null;

            timer -= Time.deltaTime;

            //opens mouth
            OpenMouth();

            yield return null;
        }

        int shoots = Random.Range(shooting2MinShoots, shooting2MaxShoots);
        timer = shooting2Delay;

        while (shoots > 0)
        {
            while (paused) yield return null;

            timer -= Time.deltaTime;

            if (timer < 0)
            {
                if (shoots % 2 == 0)
                {
                    Shoot();
                }
                else
                {
                    Shoot(2,shooting2Angle);
                }

                timer = shooting2Delay;
                shoots--;
            }

            yield return null;
        }

        while (mouthAngle > 0.1f)
        {
            while (paused) yield return null;

            CloseMouth();

            yield return null;
        }

        StartCoroutine(IdleBehaviour());
    }

    private IEnumerator Spawning()
    {
        StartCoroutine(IdleBehaviour());
        yield break;
        //Spawn some enemies
    }

    private void Shoot(int quant = 1, float angle = 0f)
    {
        float a = -angle/2;
        for (int i = 0; i < quant; i++)
        {
            JareBullet bullet = Instantiate(projectilePrefab, transform.position + transform.rotation * projectileOffset, Quaternion.Euler(transform.eulerAngles + Vector3.up * a)).GetComponent<JareBullet>();

            a += angle / (quant-1);
        }

        particleManager.EmitRadiusBurst(transform.position + transform.rotation * projectileOffset, 
                                        Random.Range(8, 12),
                                        projectileParticlesPrefab, 
                                        transform.eulerAngles,
                                        Vector3.up * 90f);//Vector3.up * 10f);
    }

    #endregion
}
