using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;

[CreateAssetMenu(fileName = "MonetizationIosIDs", menuName = "Monetization/MonetizationIosIDs", order = 0)]
public class MonetizationIosIDs : ScriptableObject 
{
    public string _iOSGameId;
    public string _iOSBannerId;
    public string _iOSInterstitialId;
    public string _iOSRewardedId;
}