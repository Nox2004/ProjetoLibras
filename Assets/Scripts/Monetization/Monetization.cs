using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Advertisements;

public class Monetization : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("General")]
    [SerializeField] private bool isTesting;
    [SerializeField] private bool ResetPurchase;

    [Header("----------- Android IDs -----------")]
    [SerializeField] private string _androidGameId;
    [SerializeField] private string _androidBannerId;
    [SerializeField] private string _androidInterstitialId;
    [SerializeField] private string _androidRewardedId;

    [Header("----------- iOS IDs -----------")]
    [SerializeField] private string _iOSGameId;
    [SerializeField] private string _iOSBannerId;
    [SerializeField] private string _iOSInterstitialId;
    [SerializeField] private string _iOSRewardedId;

    [Header("----------- Purchase -----------")]
    public static Action<string> OnPurchaseCompleted;

    private string _gameId;
    private string _bannerId;
    private string _interstitialId;
    private string _rewardedId;

    //--------------------- INITIALIZATION ---------------------
    private void Awake()
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
        //Initialize Ads
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, Debug.isDebugBuild, this);
            //Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (ResetPurchase)
        {
            PlayerPrefs.DeleteKey("PURCHASED_removeads");
        }
#endif
    }

    //--------------------- INITIALIZATION CALLBACKS ---------------------
    public void OnInitializationComplete()
    {
#if ENABLE_LOG
        Debug.Log("------------------------------ UNITY ADS INITIALIZED ------------------------------");
#endif
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
#if ENABLE_LOG
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
#endif
    }

    //--------------------- INTERSTITIAL CALLBACKS ---------------------
    public void ShowInterstitialAd()
    {
        //if (HasPurchased("removeads")) return;
        Advertisement.Show(_interstitialId, this);
        LoadAd(_interstitialId);
    }

    //--------------------- REWARDED CALLBACKS ---------------------
    public void ShowRewardedAd()
    {
        Advertisement.Show(_rewardedId, this);
        LoadAd(_rewardedId);
    }

    //--------------------- GENERAL CALLBACKS ---------------------
    public void LoadAd(string placementId)
    {
        //Debug.Log("Loading Ad: " + placementId);
        Advertisement.Load(placementId, this);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        //Reward after watching the ad
        if (placementId == _rewardedId && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
#if ENABLE_LOG

            Debug.Log("Reward do AD");
#endif
            AdPanel.sawReward = true;
        }

        //Interstitial
        if (placementId == _interstitialId && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
#if ENABLE_LOG
            Debug.Log("Fim do interstitial");
#endif
        }
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
#if ENABLE_LOG
        if (placementId == _rewardedId)
            Debug.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
        if (placementId == _interstitialId)
            Debug.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
#endif
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
#if ENABLE_LOG
        Debug.Log("[MonetizationManager] OnUnityAdsAdLoaded: " + placementId);
#endif
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
#if ENABLE_LOG
        Debug.Log("[MonetizationManager] OnUnityAdsFailedToLoad: " + placementId + " | " + message);
#endif
    }

    public void OnUnityAdsShowStart(string placementId)
    {
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }


    //--------------------- PURCHASE CALLBACKS ---------------------
    public void PurchaseCompleted(Product product)
    {
        PlayerPrefs.SetInt("PURCHASED_" + product.definition.id, 1);
        PlayerPrefs.Save();
        RemovePanel.purchased = true;

#if ENABLE_LOG
        if (HasPurchased("removeads"))
        {
            Debug.Log("ADS Removidos");
        }
        else
        {
            Debug.Log("ADS não removidos ;-;");
        }
#endif

        //if (HasPurchased("removeads")) HideBanner();

        if (OnPurchaseCompleted != null) OnPurchaseCompleted(product.definition.id);
    }

    public bool HasPurchased(string productID)
    {
        return (PlayerPrefs.GetInt("PURCHASED_" + productID) == 1);
    }
    
    //--------------------- OTHER CALLBACKS ---------------------
    public string[] GetIDs()
    {
        return new string[] { _interstitialId, _rewardedId };
    }
}