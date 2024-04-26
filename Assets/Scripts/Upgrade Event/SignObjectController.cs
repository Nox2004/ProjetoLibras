using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class SignObjectController : MonoBehaviour, ITakesDamage, IPausable
{
    #region //ITakesDamage implementation

    public bool alive { get => !destroy; set {  } }

    private float _maxHealth = 1f; // Private backing field for max health
    public float maxHealth { get => _maxHealth ; set { _maxHealth = value; } }

    private float _health; // Private backing field for current health
    public float currentHealth
    {
        get => _health;
        set
        {
            Die();
        }
    }

    virtual public void TakeDamage(float damage, int pierce) { ChooseMe(); }

    virtual public void Die() {}
    
    #endregion

    #region //IPausable implementation

    private bool paused = false;

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion

    //get a material from renderer
    [SerializeField] private int sourceMaterialIndexInRend;
    [SerializeField] private int targetMaterialIndexInRend;
    [SerializeField] private MeshRenderer mainRenderer;

    private Vector3 velocity = new Vector3(0, 0, -1);
    public float speed;

    public bool chosen = false;
    public bool destroy = false;
    
    //rotate when hit
    private bool stoppedRotating = false;
    [SerializeField] private float rotateSpeed = 3420f;
    [SerializeField] private float stopRotatingTime = 0.5f;
    [SerializeField] private GameObject rotateObject;
    [SerializeField] private AudioClip startRotatingSound, stopRotatingSound;
    private AudioSource rotatingSoundAudioSource;

    [HideInInspector] public ParticleManager particleManager;
    [SerializeField] private GameObject explosionParticlePrefab;

    private AudioManager myAudioManager;

    public float zLimit;

    //Paint the object
    [SerializeField] MeshRenderer legoRenderer;
    [SerializeField] int legoMaterialIndex;
    [SerializeField] MeshRenderer targetRenderer;
    [SerializeField] int targetMaterialIndex;


    //Set the renderer textures
    public void SetTextures(Texture sourceTexture, Texture targetTexture, Color color)
    {
        mainRenderer.materials[sourceMaterialIndexInRend].SetInt("_DrawSecondTex",1);
        mainRenderer.materials[sourceMaterialIndexInRend].SetColor("_SecondTexColor",Color.Lerp(color,Color.black,0.15f));
        mainRenderer.materials[sourceMaterialIndexInRend].SetTexture("_SecondTex",sourceTexture);

        mainRenderer.materials[targetMaterialIndexInRend].SetInt("_DrawSecondTex",1);
        mainRenderer.materials[targetMaterialIndexInRend].SetColor("_SecondTexColor",Color.Lerp(color,Color.black,0.15f));
        mainRenderer.materials[targetMaterialIndexInRend].SetTexture("_SecondTex",targetTexture);

        legoRenderer.materials[legoMaterialIndex].SetColor("_Color",color);
        targetRenderer.materials[targetMaterialIndex].SetColor("_Color",color);
    }
    
    void Start()
    {
        myAudioManager = Injector.GetAudioManager(gameObject);
    }

    void Update()
    {
        if (paused) return;

        transform.position += velocity * speed * Time.deltaTime;

        if (chosen && !stoppedRotating)
        {
            float zz = rotateObject.transform.rotation.eulerAngles.z;

            zz += rotateSpeed * Time.deltaTime;
            stopRotatingTime -= Time.deltaTime;

            if (stopRotatingTime <= 0)
            {
                stoppedRotating = true;

                zz = 180f;
                myAudioManager.StopSound(rotatingSoundAudioSource);
                rotatingSoundAudioSource = myAudioManager.PlaySound(stopRotatingSound);
            }

            rotateObject.transform.rotation = Quaternion.Euler(rotateObject.transform.rotation.eulerAngles.x, rotateObject.transform.rotation.eulerAngles.y, zz);
        }
        
        if (transform.position.z < zLimit)
        {
            Destroy(gameObject);
        }
    }

    public void DestroyMe()
    {
        particleManager.EmitExplosion(transform.position, 16, explosionParticlePrefab);
        Destroy(gameObject);
    }

    public void ChooseMe()
    {
        rotatingSoundAudioSource = myAudioManager.PlaySound(startRotatingSound);
        chosen = true;
    }
}
