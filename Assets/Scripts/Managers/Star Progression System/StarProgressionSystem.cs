using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StarProgressionSystem", menuName = "Star System/Progression System/Basic Progression", order = 1)]
public class StarProgressionSystem : ScriptableObject
{
    [SerializeField] protected Star[] stars;
    protected bool reward = false; //Is set to true when player reaches a substar

    //Total number of stars (excluding post game progress)
    public int numberOfStarsAchieved { get { return Mathf.Min(currentStarIndex,stars.Length); } }

    //Current star player is trying to reach
    public int currentStarIndex; //Index of the star player is currently trying to reach
    public int currentSubStarIndex; //Index of the star player is currently trying to reach

    public int currentStarScore = 0; //Score used to reach the current star

    //Total score of the player (considering all the past stars)
    public int currentTotalScore { get { 
        int total = 0;

        for (int i = 0; i < currentStarIndex; i++)
        {
            total += stars[i].pointsRequired;
        }
        return total+currentStarScore;
    } }
    
    public Star currentStar { get { return stars[currentStarIndex]; } }

    public SubStar currentSubstar { get { return currentStar.subStars[currentSubStarIndex]; } }
    public int currentNumOfSubstars { get { return currentStar.numOfSubstars; } }
    
    //Progression up to reach the current star in a range from 0 to 1
    public float currentStarProgression { get { return Mathf.Clamp01((float) currentStarScore / currentStar.pointsRequired); } }
    
    public StarReward currentReward { get { return currentSubstar.reward; } }

    //Progression up to reach the last star in a range from 0 to 1
    public float totalStarProgression { get {
        float maxScore = 0;
        for (int i = 0; i < stars.Length; i++)
        {
            maxScore += stars[i].pointsRequired;
        }
        return Mathf.Clamp01((float)currentTotalScore / maxScore);
    } }

    public virtual bool ReadyForReward() { return reward; }

    public virtual void AddScore(int score)
    {
        if (reward) return;
       
        int _substar = currentStar.pointsRequired / currentStar.numOfSubstars;
        int _max = (currentSubStarIndex+1) * _substar;

        currentStarScore += score;

        //reached a new substar
        if (currentStarScore >= _max)
        {
            //caps score and sets reward to true
            currentStarScore = _max;
            reward = true;
        }
    }

    public virtual void ResolveReward(bool rightAnswer)
    {
        //gets to the next substar
        currentSubStarIndex++;

        //reached next star
        if (currentSubStarIndex >= currentStar.numOfSubstars) 
        {
            currentStarIndex++;
            currentSubStarIndex = 0;
            currentStarScore = 0;
        }

        reward = false;
    }
}