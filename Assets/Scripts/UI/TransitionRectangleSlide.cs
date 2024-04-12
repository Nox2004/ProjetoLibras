using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionRectangleSlide : Transition
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Canvas canvas;

    private Vector3 initialPosition, targetPosition;

    [SerializeField] private AudioClip soundAtEnter, soundAtExit;
    private AudioManager audioManager;

    override protected void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);
        
        base.Start();

        float canvas_w = canvas.pixelRect.width / canvas.scaleFactor;
        float canvas_h = canvas.pixelRect.height / canvas.scaleFactor;

        rect.sizeDelta = new Vector2(canvas_w, canvas_h);

        //Sets rect position to the right of the screen
        rect.localPosition = new Vector3(-canvas_w, 0, 0);
        initialPosition = rect.localPosition;
        targetPosition = new Vector3(0, 0, 0);
    }

    override protected void Update()
    {
        base.Update();

        rect.localPosition = Vector3.Lerp(initialPosition, targetPosition, animationCompletion);
    }

    override protected void AtEnter()
    {
        audioManager.PlaySound(soundAtEnter);
    }

    override protected void AtExit()
    {
        audioManager.PlaySound(soundAtExit);
    }
}
