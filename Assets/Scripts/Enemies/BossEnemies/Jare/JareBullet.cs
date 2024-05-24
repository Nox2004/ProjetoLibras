using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JareBullet : MonoBehaviour, IPausable
{
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

    [SerializeField] private float speed;
    [HideInInspector] public ParticleManager particleManager;
    [HideInInspector] public GameObject particlePrefab;

    [HideInInspector] public AudioManager audioManager;
    [Serialize] private AudioClip hitSound;

    void Start()
    {
        
    }

    void Update()
    {
        if (paused) return;
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
