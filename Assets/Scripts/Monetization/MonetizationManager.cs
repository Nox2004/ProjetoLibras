using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;

public class MonetizationManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static Action OnRewardedCompleted;
    public static Action<string> OnPurchaseCompleted;

    public bool rewardedAvailable = false;

    [Header("----------- Android IDs")]
    public string _androidGameId;
    public string _androidBannerId;
    public string _androidInterstitialId;
    public string _androidRewardedId;

    [Header("----------- iOS IDs")]
    public string _iOSGameId;
    public string _iOSBannerId;
    public string _iOSInterstitialId;
    public string _iOSRewardedId;

    private string _gameId;
    private string _bannerId;
    private string _interstitialId;
    private string _rewardedId;

    void Awake()
    {
#if UNITY_IOS
        _gameId = _iOSGameId;
        _bannerId = _iOSAdUnitId;
        _interstitialId = _iOSInterstitialId;
        _rewardedId = _iOSRewardedId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
        _bannerId = _androidBannerId;
        _interstitialId = _androidInterstitialId;
        _rewardedId = _androidRewardedId;
#else
        Debug.LogError("PLATAFORMA NAO SUPORTADA");
        return;
#endif
        Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, Debug.isDebugBuild, this);
        }
    }

    //--------------------- INITIALIZATION LISTENER ---------------------
    public void OnInitializationComplete()
    {
#if ENABLE_LOG
        Debug.Log("Unity Ads initialization complete.");
#endif
        //Advertisement.Load(_interstitialId, this);
        Advertisement.Load(_rewardedId , this);

        //LoadBanner();
        //ShowInterstitial();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
#if ENABLE_LOG
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
#endif
    }

    //--------------------- BANNER CALLBACKS ---------------------
    // Implement a method to call when the Load Banner button is clicked:
    public void LoadBanner()
    {
        if (HasPurchased("removeads")) return;
        // Set up options to notify the SDK of load events:
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        // Load the Ad Unit with banner content:
        Advertisement.Banner.Load(_bannerId, options);
    }

    // Implement code to execute when the loadCallback event triggers:
    void OnBannerLoaded()
    {
        if (HasPurchased("removeads")) return;
#if ENABLE_LOG
        Debug.Log("Banner loaded");
#endif
        // Set up options to notify the SDK of show events:
        Advertisement.Banner.Show(_bannerId, null);
    }

    // Implement code to execute when the load errorCallback event triggers:
    void OnBannerError(string message)
    {
#if ENABLE_LOG
        Debug.Log($"Banner Error: {message}");
#endif
        // Optionally execute additional code, such as attempting to load another ad.
    }

    //---------------------------------------------------------------
    public void ShowInterstitialAd()
    {
        if (HasPurchased("removeads")) return;
        Advertisement.Show(_interstitialId, this);
    }

    //---------------------------------------------------------------
    public static void ShowInterstitial()
    {
        if (HasPurchased("removeads")) return;
        MonetizationManager manager = FindObjectOfType<MonetizationManager>(true);
        manager.ShowInterstitialAd();
    }

    //---------------------------------------------------------------
    public void ShowRewardedAd()
    {       
        if (rewardedAvailable)
        {
            Advertisement.Show(_rewardedId, this);
        }
    }

    //---------------------------------------------------------------
    public static void ShowRewarded()
    {
        MonetizationManager manager = FindObjectOfType<MonetizationManager>(true);
        manager.ShowRewardedAd();
    }

    //---------------------------------------------------------------
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId.Equals(_rewardedId)) rewardedAvailable = true;
#if ENABLE_LOG
        Debug.Log("[MonetizationManager] OnUnityAdsAdLoaded: " + placementId);
#endif
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (placementId.Equals(_rewardedId)) rewardedAvailable = false;
#if ENABLE_LOG
        Debug.Log("[MonetizationManager] OnUnityAdsFailedToLoad: " + placementId + " | " + message);
#endif
    }

    //---------------------------------------------------------------
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
    }

    public void OnUnityAdsShowStart(string placementId)
    {
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        //aqui vamos dar um premio para o usuario quando assistir REWARDED VIDEOS
        if (placementId == _rewardedId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            rewardedAvailable = false;
            RewardCompleted();
        }
        Advertisement.Load(placementId, this);
    }

    public void RewardCompleted()
    {
        if (OnRewardedCompleted != null) OnRewardedCompleted();
    }

    public void PurchaseCompleted(Product product)
    {
        PlayerPrefs.SetInt("PURCHASED_" + product.definition.id, 1);
        PlayerPrefs.Save();

        if (HasPurchased("removeads")) Advertisement.Banner.Hide();

        if (OnPurchaseCompleted != null) OnPurchaseCompleted(product.definition.id);
    }

    public static bool HasPurchased(string productID)
    {
        return PlayerPrefs.GetInt("PURCHASED_" + productID) == 1;
    }

}