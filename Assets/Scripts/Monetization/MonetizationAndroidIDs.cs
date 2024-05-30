using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;

[CreateAssetMenu(fileName = "MonetizationAndroidIDs", menuName = "Monetization/MonetizationAndroidIDs", order = 0)]
public class MonetizationAndroidIDs : ScriptableObject 
{
    public string _androidGameId;
    public string _androidBannerId;
    public string _androidInterstitialId;
    public string _androidRewardedId;
}