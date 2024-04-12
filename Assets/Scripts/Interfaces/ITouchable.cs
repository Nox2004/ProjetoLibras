using UnityEngine;
using UnityEngine.Events;

public interface ITouchable
{
    public bool control { get; set; } 
    public bool beingTouched { get; set; }
    //[SerializeField] public UnityEvent onTouchEvent { get; set; }

    public void HandleTouch(bool ended) {}
    
    virtual protected void DuringTouch() {}

    virtual protected void OnTouchEnd() {}
}