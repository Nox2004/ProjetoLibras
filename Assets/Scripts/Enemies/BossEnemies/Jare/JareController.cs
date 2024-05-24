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

    [Header("Idle behaviour")]
    [SerializeField] private float horizontalMovementWaveySpeed = 2f;
    [SerializeField] private float horizontalMovementAngleIncrease= 10f;

    [SerializeField] private float attackTimerMax, attackTimerMin;
    private float attackTimer;
    private bool attacking;

    [Header("Random Charge")]
    [SerializeField] private float randomChargingPrepareTime;
    [SerializeField] private float randomChargingPrepareSpeed;
    [SerializeField] private float randomChargeSpeed;
    [SerializeField] private float returnFromChargeSpeed;

    [Header("Guided Charge")]
    [SerializeField] private float guidedChargePrepareTime;
    [SerializeField] private float guidedChargeSpeed;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;

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
        //Change later
        zLimit = -100;
        difficultyValue = 1;
        spawnPosition = new Vector3(0,-1,32);
        floorWidth = 6;

        attackTimer = Random.Range(attackTimerMin, attackTimerMax);

        base.Start();

        player = FindObjectOfType<PlayerController>();
        initialRotation = transform.eulerAngles;

        mouthInitialAngle = mouth.localRotation.x;
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

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;

                waveyTimer += Time.deltaTime * horizontalMovementWaveySpeed;

                float lastX = transform.position.x;
                transform.position = startPos + Vector3.right * ((Mathf.Sin(waveyTimer+timerOffset) * moveRange/2f) - xOffset);

                targetRotation = Vector3.up * (lastX-transform.position.x) * horizontalMovementAngleIncrease;

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

    private IEnumerator RandomCharge()
    {
        targetRotation = Vector3.zero;

        Vector3 startPos = transform.position;

        float timer = 0;
        while (timer < randomChargingPrepareTime)
        {
            while (paused) yield return null;

            transform.Translate(Vector3.back * randomChargingPrepareSpeed * Time.deltaTime);
            timer += Time.deltaTime;

            //opens mouth
            OpenMouth();

            yield return null;
        }

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
                GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);

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
                    GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
                }
                else
                {
                    GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(transform.eulerAngles + Vector3.up * shooting2Angle/2));
                    projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(transform.eulerAngles - Vector3.up * shooting2Angle/2));
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


    #endregion
}
