using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Star", menuName = "Star System/Star")]
public class Star : ScriptableObject
{
    public int pointsRequired;
    public SubStar[] subStars;
    public int numOfSubstars { get { return subStars.Length; } }
}

[Serializable]
public class SubStar
{
    public EnemyPool enemyPool;
    public StarReward reward;
}