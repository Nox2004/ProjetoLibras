using UnityEngine;

public class AdPanel : MonoBehaviour
{
    public SeeAdToUnlockButton lockButton;
    private Panel panel;

    void Start()
    {
        panel = GetComponent<Panel>();

        MonetizationManager.OnRewardedCompleted += OnRewardedCompleted;
    }

    private void OnDestroy()
    {
        MonetizationManager.OnRewardedCompleted -= OnRewardedCompleted;
    }

    public void OnRewardedCompleted()
    {
        lockButton.SawAdd();
    }

    public void SetPanelActive()
    {
        panel.SetActive(true);
    }

    public void SeeAd()
    {
        MonetizationManager.ShowRewarded();
        //lockButton.SawAdd();

        lockButton.ReturnFromAdScreen();
        panel.SetActive(false);
    }

    public void NotSeeAd()
    {
        lockButton.ReturnFromAdScreen();
        panel.SetActive(false);
    }
}