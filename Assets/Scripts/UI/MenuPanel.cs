using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    private bool active = false;
    [SerializeField] private Canvas canvas;
    [SerializeField, Range(1,2), Tooltip("Multiplies panel size so it has a border offset")] private float sizeMultiplier;
    
    private Vector3 unactivePosition, activePosition, initialPosition, targetPosition;
    [SerializeField] private CurveValueInterpolator moveAnimation;
    private Image image; private RectTransform rect;

    [SerializeField] private AudioClip moveSound;
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        
        float xRatio = rect.sizeDelta.x; float yRatio = rect.sizeDelta.y;

        //Set the image size to cover the whole canvas
        rect.sizeDelta = new Vector2(canvas.pixelRect.width / canvas.scaleFactor, canvas.pixelRect.height / canvas.scaleFactor);

        //readjusts children position
        xRatio /= rect.sizeDelta.x; yRatio /= rect.sizeDelta.y;

        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            childRect.sizeDelta = new Vector2(childRect.sizeDelta.y / xRatio, childRect.sizeDelta.y / xRatio); //scales the size of the child with height ratio
            childRect.localPosition = new Vector3(childRect.localPosition.x / xRatio, childRect.localPosition.y / yRatio, childRect.localPosition.z);
        }
        
        //applies a border offset using a multiplier and sends the panel to the bottom of the screen
        rect.sizeDelta *= sizeMultiplier;
        rect.localPosition = new Vector3(0f, -rect.sizeDelta.y, 0);

        //set the active and unactive positions up
        unactivePosition = rect.localPosition;
        activePosition = new Vector3(0f, 0f, 0);
        initialPosition = unactivePosition; targetPosition = unactivePosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (initialPosition != targetPosition)
        {
            rect.localPosition = Vector3.LerpUnclamped(initialPosition, targetPosition, moveAnimation.Update(Time.deltaTime));
            if (moveAnimation.Finished())
            {
                initialPosition = targetPosition;
            }
        }
    }

    public void SetActive(bool active)
    {
        audioSource.PlayOneShot(moveSound);
        this.active = active;

        initialPosition = rect.localPosition;
        targetPosition = active ? activePosition : unactivePosition;

        moveAnimation.Reset();
    }
}
