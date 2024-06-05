public class MonetizationManager : Singleton<MonetizationManager>
{
    public Monetization monetization;
    private string _interstitialId;
    private string _rewardedId;

    private void Awake()
    {
        base.Awake();
        //Prepare for Ads
        _interstitialId = monetization.GetIDs()[0];
        _rewardedId = monetization.GetIDs()[1];

        //Load Ads
        monetization.LoadAd(_interstitialId);
        monetization.LoadAd(_rewardedId);

        //LoadBanner
        //monetization.LoadBanner();
    }
}