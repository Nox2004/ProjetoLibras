using UnityEngine;
using UnityEngine.UI;

public class RemoveAdsToUnlockButton : Button2D
{
    //RemoveAdsToUnlockButton
    private float currentScale;
    [SerializeField] private float normalScale = 1f, touchScale = 1f, scaleSmoothRatio;

    [SerializeField] private AudioClip touchSound;
    private AudioManager audioManager;

    [SerializeField] private Button2D lockedButton;
    [SerializeField] private Panel activePanel;
    [SerializeField] private RemovePanel removeAdsPanel;

    public Button lockAdButton;

    [SerializeField] private float destroyScaleMultiply;
    [SerializeField] private float destroyScaleTreshold;

    private bool destroy = false;

    override protected void Start()
    {
        base.Start();
        audioManager = Injector.GetAudioManager(gameObject);
        lockedButton.control = false;
        //lockAdButton = GameObject.Find("SeeAd").GetComponent<Button>();
        lockAdButton.interactable = false;
    }

    override protected void LateUpdate()
    {
        base.LateUpdate();

        if (destroy)
        {
            //Change Later
            normalScale *= Mathf.Pow(destroyScaleMultiply, Time.deltaTime);
            if (normalScale < destroyScaleTreshold)
            {
                activePanel.RemoveButton(this);
                Destroy(gameObject);
            }

            control = false;
        }
        else
        {
            lockedButton.control = false;
        }

        float target_scale = beingTouched ? touchScale : normalScale;
        currentScale += (target_scale - currentScale) / (scaleSmoothRatio / Time.deltaTime);

        beingTouched = false;

        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    override protected void OnTouchEnd()
    {
        base.OnTouchEnd();

        activePanel.SetButtonsActive(false);
        removeAdsPanel.SetPanelActive();
        removeAdsPanel.lockButton = this;
        lockAdButton.interactable = true;

        audioManager.PlaySound(touchSound);
    }

    public void BoughtAdRemove()
    {
        lockedButton.control = true;
        lockAdButton.interactable = false;
        destroy = true;
    }

    public void ReturnFromShopScreen()
    {
        activePanel.SetButtonsActive(true);
    }

}
