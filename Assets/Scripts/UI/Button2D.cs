using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Button2D : MonoBehaviour, ITouchable
{
    [HideInInspector] public bool control { get; set; } 
    [HideInInspector] public bool beingTouched { get; set; }
    protected bool startedTouchWhenActive = false;

    //Touch event
    [SerializeField] protected UnityEvent onTouchEvent;

    protected RectTransform rectTransform;

    virtual protected void Start()
    {
        control = true;
        rectTransform = GetComponent<RectTransform>();
    }

    virtual protected void LateUpdate()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(Input.touchCount-1);

            if (touch.phase == TouchPhase.Began && control)
            {
                startedTouchWhenActive = true;
            }

            Vector2 touchPosition = touch.position;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPosition) && startedTouchWhenActive)
            {
                HandleTouch(touch.phase == TouchPhase.Ended);
            }
        }
        else
        {
            startedTouchWhenActive = false;
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
