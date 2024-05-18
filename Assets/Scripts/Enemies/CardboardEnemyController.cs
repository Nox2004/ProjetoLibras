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

    [SerializeField] protected MeshRenderer cardboardRenderer;
    [SerializeField] protected int cardboardMaterialIndex;

    [SerializeField] protected MeshRenderer baseRenderer;
    [SerializeField] protected int baseMaterialIndex;

    private Color originalCardboardColor;
    [SerializeField] private Color deadColor;
    private float deadColorLerpValue = 1f;
    [SerializeField] private float deadColorLerpMultiplier;
    
    [Header("Sounds")]
    [SerializeField] AudioClip[] deathSounds;
    [SerializeField] AudioClip[] hitSounds;    

    override protected void Start()
    {
        base.Start();

        speed *= speedMultiplier;

        originalCardboardColor = cardboardRenderer.material.GetColor("_Color");
    }
    
    override protected void Update()
    {
        if (paused) return;
        
        base.Update();

        speed = levelManager.objectsSpeed * speedMultiplier;

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

            deadColorLerpValue *= Mathf.Pow(deadColorLerpMultiplier, Time.deltaTime);
            cardboardRenderer.materials[cardboardMaterialIndex].SetColor("_Color", Color.Lerp(deadColor, originalCardboardColor, deadColorLerpValue));
            baseRenderer.materials[baseMaterialIndex].SetColor("_Color", Color.Lerp(deadColor, originalCardboardColor, deadColorLerpValue));
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
