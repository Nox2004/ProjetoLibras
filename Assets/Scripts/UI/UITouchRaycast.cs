using UnityEngine;

public class UITouchRaycast : MonoBehaviour
{
    [SerializeField] private bool debug; private string debugTag = "UITouchCast: ";

    [SerializeField] private LayerMask uiLayerMask; // Layer mask for your UI elements
    [SerializeField] private Camera uiCamera;    // Reference to the main camera (optional, if not attached)

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = uiCamera.ScreenPointToRay(touch.position); // Get ray from touch position
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, uiLayerMask))
            {
                var touched_ui = hit.collider.gameObject.GetComponent<Button3D>();
                if (touched_ui != null) touched_ui.HandleTouch(touch.phase == TouchPhase.Ended);
            }
        }
    }
}