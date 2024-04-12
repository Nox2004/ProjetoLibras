using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    [SerializeField] protected bool active = false;
    [SerializeField] protected Canvas canvas;
    [SerializeField, Range(1,2), Tooltip("Multiplies panel size so it has a border offset")] private float sizeMultiplier;
    
    protected Image image; protected RectTransform rect;
    protected AudioManager audioManager;

    private List<ITouchable> childButtons;
    private bool needsToUpdateButtons = false;
    virtual protected void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);

        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        
        float xRatio = rect.sizeDelta.x; float yRatio = rect.sizeDelta.y;

        //Set the image size to cover the whole canvas
        rect.sizeDelta = new Vector2(canvas.pixelRect.width / canvas.scaleFactor, canvas.pixelRect.height / canvas.scaleFactor);

        //Get the ratio of the image size to the canvas size
        xRatio /= rect.sizeDelta.x; yRatio /= rect.sizeDelta.y;

        //readjusts children position
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.gameObject.GetComponent<RectTransform>();
            childRect.localPosition = new Vector3(childRect.localPosition.x / xRatio, childRect.localPosition.y / yRatio, childRect.localPosition.z);
        }

        //readjusts children size
        ScaleWithCanvas[] childs = GetComponentsInChildren<ScaleWithCanvas>();
        foreach (ScaleWithCanvas child in childs)
        {
            RectTransform childRect = child.gameObject.GetComponent<RectTransform>();
            if (child.scaleUniformly)
            {
                childRect.sizeDelta = new Vector2(childRect.sizeDelta.y / xRatio, childRect.sizeDelta.y / xRatio); 
                //scales the size of the child with height ratio
            }   
            else
            {
                childRect.sizeDelta = new Vector2(childRect.sizeDelta.x / xRatio, childRect.sizeDelta.y / yRatio);
                //scales the size of the child with width and height ratio
            }
        }
        
        //applies a border offset using a multiplier and sends the panel to the bottom of the screen
        rect.sizeDelta *= sizeMultiplier;

        childButtons = new List<ITouchable>();
        foreach (Transform child in transform)
        {
            ITouchable button = child.GetComponent<ITouchable>();
            if (button != null)
            {
                childButtons.Add(button);
            }
        }

        SetActive(active);
    }

    virtual protected void Update()
    {
        if (needsToUpdateButtons)
        {
            foreach (ITouchable button in childButtons)
            {
                button.control = active;
            }
            needsToUpdateButtons = false;
        }
    }

    virtual public void SetActive(bool active)
    {
        this.active = active;
        
        needsToUpdateButtons = true;
    }
}
