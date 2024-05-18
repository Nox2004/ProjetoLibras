using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ConfettiLauncher : MonoBehaviour
{
    [SerializeField] private GameObject confettiPrefab;
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] private Canvas canvas3D;
    [SerializeField] private Material[] confettiMaterials;

    [SerializeField] private int confetti_num = 60;
    [SerializeField] private float hStrength = 11, vStrength = 16;

    public void LaunchConfetti()
    {
        float c_width = canvas3D.pixelRect.width / canvas3D.scaleFactor;
        float c_height = canvas3D.pixelRect.height / canvas3D.scaleFactor;
        float x_pos_left = -c_width/2;
        float x_pos_right = c_width/2;
        float y_pos = -c_height/2;
        
        for (int i = 0; i < confetti_num; i++)
        {
            float xx = x_pos_left;
            if (i > (int) (confetti_num/2))
            {
                xx = x_pos_right;
            }
            GameObject part = particleManager.EmitSingleParticle(Vector3.zero, confettiPrefab, Vector3.zero,-1,canvas3D.transform);

            //part.transform.SetParent(canvas3D.transform);
            part.GetComponent<RectTransform>().localPosition = new Vector3(xx, y_pos, 0);

            ConfettiParticle confetti = part.GetComponent<ConfettiParticle>();

            confetti.horizonalSpeed = Random.Range(hStrength/2, hStrength);
            confetti.verticalSpeed = Random.Range(hStrength/2, vStrength);

            if (i > (int) (confetti_num/2)) confetti.horizonalSpeed = -confetti.horizonalSpeed;

            confetti.meshRenderer.material = confettiMaterials[Random.Range(0, confettiMaterials.Length)];
        }
        
        //particleManager
    }

    
}


