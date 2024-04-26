using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CardboardEnemyController : EnemyController
{
    [Header("Speed multiplier")]
    [SerializeField] protected float speedMultiplier;

    [Header("Cardboard properties")]
    [SerializeField] protected Transform cardboardTransform;
    [SerializeField] protected MeshRenderer paperRenderer;
    [SerializeField] protected int paperMaterialIndex;
    protected Texture currentTexture;
    
    //Falling when dead animation
    protected float fallSpeed = 0f;
    [SerializeField] protected float fallAcceleration = 0.1f;
    [SerializeField] protected float fallMaxAngle = 60;
    [SerializeField] protected Vector3 fallRotationVector;
    private float angleRotated;

    [Header("Sounds")]
    [SerializeField] AudioClip[] deathSounds;
    [SerializeField] AudioClip[] hitSounds;

    override protected void Start()
    {
        base.Start();

        speed *= speedMultiplier;
    }
    
    override protected void Update()
    {
        if (paused) return;
        
        base.Update();

        transform.position += Vector3.back * speed * Time.deltaTime; //Not using transform.Translate because its affected by the object's rotation

        //Falling animation
        if (state == EnemyState.Dead)
        {
            if (angleRotated < fallMaxAngle)
            {
                fallSpeed += fallAcceleration * Time.deltaTime;
                angleRotated += fallSpeed * Time.deltaTime;

                //Corrects the last rotation to be exactly the max angle
                if (angleRotated > fallMaxAngle)
                {
                    fallSpeed -= angleRotated - fallMaxAngle;
                    angleRotated = fallMaxAngle;
                }

                cardboardTransform.Rotate(fallRotationVector * fallSpeed * Time.deltaTime);
            }


            // float target_x = cardboardInitialXRotation + 60;
            // if (cardboardTransform.localRotation.eulerAngles.x < target_x)
            // {
            //     cardboardTransform.Rotate(Vector3.up, fallSpeed);
            //     // var tmp = cardboardTransform.localRotation.eulerAngles;
            //     // tmp.x += fallSpeed * Time.deltaTime;
            //     // tmp.x = Mathf.Min(tmp.x, target_x);
                
            //     //cardboardTransform.localRotation = Quaternion.Euler(tmp);
            // }
        }
    }

    public override void TakeDamage(float damage, int pierce)
    {
        base.TakeDamage(damage, pierce);

        audioManager.PlayRandomSound(hitSounds);
    }

    override public void Die()
    {
        base.Die();

        audioManager.PlayRandomSound(deathSounds);

        angleRotated = 0f;
    }

    protected void ChangePaperTexture(Texture newTexture)
    {
        currentTexture = newTexture;
        paperRenderer.materials[paperMaterialIndex].mainTexture = newTexture;
    }
}
