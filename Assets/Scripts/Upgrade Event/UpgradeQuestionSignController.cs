using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeQuestionSignController : MonoBehaviour
{
    public enum Animation
    {
        None,
        Entering,
        Idle,
        ShowAnswer, 
        Exiting
    }

    private Animation currentAnimation = Animation.None;
    
    [SerializeField] private MeshRenderer questionRenderer;
    [SerializeField] private MeshRenderer answerRenderer;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    //Question object procedural animation
    [SerializeField] private CurveValueInterpolator EnterInterpolator;
    private float xAngle = 0f, yAngle = 0f;
    [SerializeField] private CurveValueInterpolator ExitInterpolator;
    private float yPosition = 0f;
    private WaveValueInterpolator yAngleInterpolator, xAngleInterpolator, zAngleInterpolator;

    [SerializeField] private float showAnswerRotationRatio, showAnswerDuration;
    private float showAnswerTimer;
    
    //Set the renderer textures
    public void SetTextures(Texture questionTexture, Texture answerTexture)
    {
        questionRenderer.material.SetInt("_DrawSecondTex",1);
        questionRenderer.material.SetTexture("_SecondTex",questionTexture);

        answerRenderer.material.SetInt("_DrawSecondTex",1);
        answerRenderer.material.SetTexture("_SecondTex",answerTexture);
    }

    void Start()
    {
        //Set the initial position and rotation of the question object
        initialPosition = transform.localPosition;
        initialRotation = transform.rotation;

        //Set the wavey interpolators
        xAngleInterpolator = new WaveValueInterpolator(-2f, 2f, 2f);
        yAngleInterpolator = new WaveValueInterpolator(-3f, 3f, 1.7f);
        zAngleInterpolator = new WaveValueInterpolator(-4f, 4f, 3f);

        //currentAnimation = Animation.None;
    }

    void Update()
    {
        switch (currentAnimation)
        {
            case Animation.Entering:
            {
                EnterInterpolator.Update(Time.deltaTime);

                xAngle = EnterInterpolator.GetValue();
                
                if (EnterInterpolator.Finished())
                {
                    currentAnimation = Animation.Idle;
                }
            }
            break;
            case Animation.Idle:
            {
                xAngleInterpolator.Update(Time.deltaTime);
                yAngleInterpolator.Update(Time.deltaTime);
                zAngleInterpolator.Update(Time.deltaTime);
            }
            break;
            case Animation.ShowAnswer:
            {
                xAngleInterpolator.Update(Time.deltaTime);
                yAngleInterpolator.Update(Time.deltaTime);
                zAngleInterpolator.Update(Time.deltaTime);
                
                float target = 180f;
                yAngle += (target-yAngle) / (showAnswerRotationRatio / Time.deltaTime);

                showAnswerTimer -= Time.deltaTime;
                if (showAnswerTimer <= 0f)
                {
                    SetAnimation(Animation.Exiting);
                }
            }
            break;
            case Animation.Exiting:
            {
                ExitInterpolator.Update(Time.deltaTime);
                yPosition = ExitInterpolator.GetValue();

                if (ExitInterpolator.Finished())
                {
                    currentAnimation = Animation.None;
                    xAngle = 0f;
                    yAngle = 0f;
                    yPosition = 0f;
                    xAngleInterpolator.Reset(); yAngleInterpolator.Reset(); zAngleInterpolator.Reset();
                }
            }
            break;
        }
        
        //Updates sign rotation
        Vector3 tmp = initialRotation.eulerAngles;
        tmp += new Vector3(xAngle + xAngleInterpolator.GetValue(),
        yAngle + yAngleInterpolator.GetValue(),
        zAngleInterpolator.GetValue());
        
        transform.rotation = Quaternion.Euler(tmp);
        
        //Updates sign position
        tmp = initialPosition; tmp.y += yPosition;
        transform.localPosition = tmp;
    }

    public void SetAnimation (Animation value)
    {
        currentAnimation = value;
        Debug.Log("SetAnimation: " + value);

        switch (value)
        {
            case Animation.Entering:
            {
                EnterInterpolator.Reset();
            }
            break;
            case Animation.Exiting:
            {
                ExitInterpolator.Reset();
            }
            break;
            case Animation.ShowAnswer:
            {
                showAnswerTimer = showAnswerDuration;
            }
            break;
        }
    }
}
