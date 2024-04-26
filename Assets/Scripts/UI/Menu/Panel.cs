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

    protected List<ITouchable> childButtons;
    protected bool needsToUpdateButtons = false;
    virtual protected void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);

        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        
        //Set the image size to cover the whole canvas
        rect.sizeDelta = new Vector2(canvas.pixelRect.width / canvas.scaleFactor, canvas.pixelRect.height / canvas.scaleFactor);
        
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
            SetButtonsActive(active);
            needsToUpdateButtons = false;
        }
    }

    virtual public void SetActive(bool active)
    {
        this.active = active;
        
        needsToUpdateButtons = true;
    }

    virtual public void SetButtonsActive(bool on)
    {
        foreach (ITouchable button in childButtons)
        {
            button.control = on;
        }
    }

    virtual public void AddButton(Button2D button)
    {
        childButtons.Add(button);
        needsToUpdateButtons = true;        
    }

    virtual public void RemoveButton(Button2D button)
    {
        childButtons.Remove(button);
    }
}
