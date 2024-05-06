using System;
using UnityEngine;

public enum RewardType
{
    UpgradeEvent,
    FreeUpgreade,
    BossFight,
    Points
}

public class StarReward : ScriptableObject
{
    public Sprite rewardIcon;
}