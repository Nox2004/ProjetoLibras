using UnityEngine;
using UnityEngine.Events;

public class Button2D : MonoBehaviour
{
    protected bool control, beingTouched;

    protected RectTransform rectTransform;

    //OnTouch event
    [SerializeField] protected UnityEvent onTouchEvent;
    
    virtual protected void Start()
    {
        control = true;
        rectTransform = GetComponent<RectTransform>();
    }

    virtual protected void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(Input.touchCount-1);
            Vector2 touchPosition = touch.position;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPosition))
            {
                HandleTouch(touch.phase == TouchPhase.Ended);
            }
        }
    }

    protected void HandleTouch(bool ended)
    {
        if (!control) return;
        
        if (ended)
        {
            OnTouchEnd();
        }

        DuringTouch();
    }
    
    virtual protected void DuringTouch()
    {
        if (!beingTouched) beingTouched = true;
    }

    virtual protected void OnTouchEnd()
    {
        onTouchEvent.Invoke();
    }
}
