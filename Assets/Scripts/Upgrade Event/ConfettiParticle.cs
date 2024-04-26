using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ConfettiParticle : ObjectFromPool, IParticle, IPausable
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
    
    public Vector3 direction { get; set; }
    public float speed { get; set; }

    public float lifeTime { get; set; }
    public float lifeSpan { get; set; }

    
    public float horizonalSpeed;
    public float verticalSpeed;

    [SerializeField] private float gravity;

    [SerializeField] private float speedMultiplierOverTime = 0.1f;

    [SerializeField] private float minScaleMult, maxScaleMult;
    private Vector3 initialScale;

    [SerializeField] private float minRotationSpeed, maxRotationSpeed;
    private float xRotationSpeed, yRotationSpeed, zRotationSpeed;

    private MeshRenderer rend; 
    public MeshRenderer meshRenderer { 
        get 
        { 
            if (rend==null) rend = GetComponent<MeshRenderer>(); 
            return rend;
        } 
    }

    void Start()
    {
        initialScale = transform.localScale;

        xRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        yRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        zRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
    }

    override protected void AfterEnable()
    {
        base.AfterEnable();

        transform.localScale = initialScale * Random.Range(minScaleMult, maxScaleMult);
    }

    override protected void Update()
    {
        if (paused) return;

        base.Update();

        horizonalSpeed *= Mathf.Pow(speedMultiplierOverTime, Time.deltaTime);
        verticalSpeed -= gravity * Time.deltaTime;

        Vector3 tmp = transform.position;
        tmp.x += horizonalSpeed * Time.deltaTime;
        tmp.y += verticalSpeed * Time.deltaTime;
        transform.position = tmp;
        
        transform.Rotate(new Vector3(xRotationSpeed, yRotationSpeed, zRotationSpeed) * Time.deltaTime);

        if (tmp.y < -transform.localScale.y)
        {
            pooler.ReturnObject(gameObject);
        }
    }
}
