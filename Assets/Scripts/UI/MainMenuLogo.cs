using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLogo : MonoBehaviour
{
    private Vector3 initialPosition, initialRotation;
    [SerializeField] private CurveValueInterpolator dropInterpolator;
    private float yy = 0f;
    [SerializeField] private WaveValueInterpolator angleWave;
    private float zzAngle = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        yy = dropInterpolator.Update();
        zzAngle = angleWave.Update();
        
        transform.position = initialPosition + Vector3.up * yy;
        transform.rotation = Quaternion.Euler(initialRotation + Vector3.forward * zzAngle);
    }
}
