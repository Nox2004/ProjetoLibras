using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveyTransform : MonoBehaviour, IPausable
{
    #region //IPausable implementation

    protected bool paused = false;

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion

    [Header("Interpolator")]
    [SerializeField] private WaveValueInterpolator waveyInterpolator;

    [Header("Local Position")]
    [SerializeField] private bool posX = false;
    [SerializeField] private bool posY = false;
    [SerializeField] private bool posZ = false;

    [Header("Local Rotation")]
    [SerializeField] private bool rotX = false;
    [SerializeField] private bool rotY = false;
    [SerializeField] private bool rotZ = false;

    [Header("Local Scale")]
    [SerializeField] private bool scaleX = false;
    [SerializeField] private bool scaleY = false;
    [SerializeField] private bool scaleZ = false;

    private Vector3 initialPosition;
    private Vector3 initialRotation;
    private Vector3 initialScale;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localEulerAngles;
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (paused) return;
        
        float value = waveyInterpolator.Update();

        transform.localPosition = new Vector3(
            posX ? initialPosition.x + value : transform.localPosition.x,
            posY ? initialPosition.y + value : transform.localPosition.y,
            posZ ? initialPosition.z + value : transform.localPosition.z
        );

        transform.localEulerAngles = new Vector3(
            rotX ? initialRotation.x + value : transform.localEulerAngles.x,
            rotY ? initialRotation.y + value : transform.localEulerAngles.y,
            rotZ ? initialRotation.z + value : transform.localEulerAngles.z
        );

        transform.localScale = new Vector3(
            scaleX ? initialScale.x + value : transform.localScale.x,
            scaleY ? initialScale.y + value : transform.localScale.y,
            scaleZ ? initialScale.z + value : transform.localScale.z
        );

    }
}