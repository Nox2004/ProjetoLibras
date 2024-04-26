using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorUV : MonoBehaviour, IPausable
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

    [SerializeField] private MeshRenderer rend;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private float UVVelocityPerUnit = 0.04f;
    private float UVOffset = 0;

    void Update()
    {
        if (paused) return;
        
        UVOffset += levelManager.objectsSpeed * UVVelocityPerUnit * Time.deltaTime;
        rend.material.SetFloat("_UVOffsetY", UVOffset);
    }
}
