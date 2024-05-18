using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    private bool on = false;
    private float onScale;
    private float offScale = 0f;
    private float currentScale;
    private float targetScale;

    [SerializeField] private float scaleSmoothnessRatio;
    [SerializeField] private float textureMoveSpeed;

    [Header("Audio")]
    [SerializeField] private AudioClip shieldActivateSound;
    [SerializeField] private AudioClip shieldDeactivateSound;
    private AudioManager audioManager;

    private Material material;

    private void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);
        material = GetComponent<MeshRenderer>().material;

        onScale = transform.localScale.x;

        currentScale = on ? onScale : offScale;
        targetScale = currentScale;
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    private void Update()
    {
        targetScale = on ? onScale : offScale;
        
        currentScale += (targetScale-currentScale) / (scaleSmoothnessRatio / Time.deltaTime);

        transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        material.mainTextureOffset += Vector2.up * textureMoveSpeed * Time.deltaTime;
    }

    public void Activate (bool on)
    {
        audioManager.PlaySound(on ? shieldActivateSound : shieldDeactivateSound);
        
        this.on = on;
    }
}