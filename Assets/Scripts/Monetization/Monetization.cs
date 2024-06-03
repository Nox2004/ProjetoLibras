using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;

public class Monetization : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener, IStoreListener
{
    //Mudar os UNITY_EDITOR para ENABLE_LOG

    [Header("General")]
    [SerializeField] private bool isTesting;

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

    private string _gameId;
    private string _bannerId;
    private string _interstitialId;
    private string _rewardedId;

    [Header("----------- Purchase -----------")]
    public static Action<string> OnPurchaseCompleted;
    public IStoreController storeController;
    public SubscriptionItem sItem;

    //--------------------- INITIALIZATION ---------------------
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
        //Initialize Ads
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
#if UNITY_IOS
            Advertisement.Initialize(_gameId, Debug.isDebugBuild, this);
#else
            Advertisement.Initialize(_gameId, isTesting, this);
#endif
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        }
    }

    private void Start()
    {
        //Initialize Purchase
        SetupBuilder();
    }

    //--------------------- INITIALIZATION LISTENER ---------------------
    public void OnInitializationComplete()
    {
#if ENABLE_LOG
        Debug.Log("Unity Ads initialization complete.");
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
        if (HasPurchased("removeads")) return;
        Advertisement.Show(_interstitialId, this);
        LoadAd(_interstitialId);
    }

    //--------------------- REWARDED CALLBACKS ---------------------
    public void ShowRewardedAd()
    {
        Advertisement.Show(_rewardedId, this);
        LoadAd(_rewardedId);
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
        if (placementId == _rewardedId)
            Debug.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
        if (placementId == _interstitialId)
            Debug.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
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

    //---------------------------------------------------------------

    public string[] GetIDs() {
        return new string[] {_interstitialId, _rewardedId };
    }

    //--------------------- PURCHASE INITIALIZATION ---------------------
    void SetupBuilder()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct("removeads", ProductType.Subscription);

#pragma warning disable CS0618 // O tipo ou membro é obsoleto
        UnityPurchasing.Initialize(this, builder);
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
#if ENABLE_LOG
        Debug.Log("Unity Purchase initialization complete.");
#endif
        storeController = controller;
    }

    //--------------------- PURCHASE CALLBACKS ---------------------
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;
#if ENABLE_LOG
        Debug.Log("Purchase complete " + product.definition.id);
#endif
        if (product.definition.id == sItem.ID)
        {
            PurchaseCompleted(product);
        }
        return PurchaseProcessingResult.Complete;
    }

    public void PurchaseCompleted(Product product)
    {
        PlayerPrefs.SetInt("PURCHASED_" + product.definition.id, 1);
        PlayerPrefs.Save();
        RemovePanel.purchased = true;

#if UNITY_EDITOR
        if (HasPurchased("removeads"))
        {
            Debug.Log("Deu bom, o bgl é igual a 1");
        }
        else
        {
            Debug.Log("deu nada, o bgl é igual a 0");
        }
#endif

        if (HasPurchased("removeads")) Advertisement.Banner.Hide();

        if (OnPurchaseCompleted != null) OnPurchaseCompleted(product.definition.id);
    }

    public bool HasPurchased(string productID)
    {
        return PlayerPrefs.GetInt("PURCHASED_" + productID) == 1;
    }

    //---------------------------------------------------------------

    public void OnInitializeFailed(InitializationFailureReason error)
    {
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
    }

}
[Serializable]
public class SubscriptionItem
{
    public string Name;
    public string ID;
    public string desc;
    public string price;
    public string timeDuration; //in Days
}