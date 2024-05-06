using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Star System/Star Rewards/Upgrade", order = 1)]
public class UpgradeReward : StarReward
{
    public int numberOfUpgradeOptions;
    public int upgradeRarityLevel;
}