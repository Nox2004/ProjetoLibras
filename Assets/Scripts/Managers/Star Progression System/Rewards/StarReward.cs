using System;
using UnityEngine;

public enum RewardType
{
    SignQuizEvent,
    FreeUpgreade,
    BossFight,
    Points
}

public class StarReward : ScriptableObject
{
    public Sprite rewardIcon;
}