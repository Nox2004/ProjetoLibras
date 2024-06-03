using UnityEngine;

public class AdPanel : MonoBehaviour
{
    public SeeAdToUnlockButton lockButton;
    private Panel panel;
    public static bool sawReward = false;

    void Start()
    {
        panel = GetComponent<Panel>();
    }

    private void Update()
    {
        if (sawReward)
        {
            OnRewardedCompleted();
            sawReward = false;
        }
    }

    private void OnDestroy()
    {

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
        MonetizationManager.Instance.monetization.ShowRewardedAd();

        lockButton.ReturnFromAdScreen();
        panel.SetActive(false);
    }

    public void NotSeeAd()
    {
        lockButton.ReturnFromAdScreen();
        panel.SetActive(false);
    }
}