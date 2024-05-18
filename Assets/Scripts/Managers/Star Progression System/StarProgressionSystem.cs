using System;
using UnityEngine;

[Serializable]
public class StartProgressionSystem
{
    [SerializeField] private Star[] stars;
    private bool reward = false; //Is set to true when player reaches a substar

    //post game stuff
    [SerializeField] private Star postGameStar;
    public bool isInPostGame { get { return currentStarIndex >= stars.Length; } }
    public int postGameLevel { get { return (currentStarIndex - stars.Length)+1; } } 

    //Total number of stars (excluding post game progress)
    public int numberOfStarsAchieved { get { return Mathf.Min(currentStarIndex+1,stars.Length); } }

    //Current star player is trying to reach
    public int currentStarIndex; //Index of the star player is currently trying to reach
    public int currentSubStarIndex; //Index of the star player is currently trying to reach

    public int currentStarScore = 0; //Score used to reach the current star


    //Total score of the player (considering all the past stars)
    public int currentTotalScore { get { 
        int total = 0;

        if (isInPostGame) 
        {
            total = 0;
            for (int i = 0; i < stars.Length; i++)
            {
                total += stars[i].pointsRequired;
            }

            for (int i = stars.Length; i < currentStarIndex; i++)
            {
                total += postGameStar.pointsRequired;
            }
            return total + currentStarScore;
        }

        for (int i = 0; i < currentStarIndex; i++)
        {
            total += stars[i].pointsRequired;
        }
        return total+currentStarScore;
    } }
    
    public Star currentStar { get { 
        if (isInPostGame) 
        {
            return postGameStar;
        }
        return stars[currentStarIndex]; 
    } }

    public SubStar currentSubstar { get { return currentStar.subStars[currentSubStarIndex]; } }
    public int currentNumOfSubstars { get { return currentStar.numOfSubstars; } }
    
    //Progression up to reach the current star in a range from 0 to 1
    public float currentStarProgression { get { return Mathf.Clamp01((float) currentStarScore / currentStar.pointsRequired); } }
    
    public StarReward currentReward { get { return currentSubstar.reward; } }

    public EnemyPool[] postGameEnemyPools;

    //Progression up to reach the last star in a range from 0 to 1
    public float totalStarProgression { get {
        float maxScore = 0;
        for (int i = 0; i < stars.Length; i++)
        {
            maxScore += stars[i].pointsRequired;
        }
        return Mathf.Clamp01((float)currentTotalScore / maxScore);
    } }

    public bool ReadyForReward() { return reward; }

    public void AddScore(int score)
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

    public void ResolveReward(bool rightAnswer)
    {
        //gets to the next substar
        if (isInPostGame) 
        {
            if (rightAnswer) //only advences to the next star if the player answered right
            {
                currentSubStarIndex++;
            }
            else
            {
                int _substar_length = currentStar.pointsRequired / currentStar.numOfSubstars;

                currentStarScore = currentSubStarIndex * _substar_length;
            }
        }
        else
        {
            currentSubStarIndex++;
        }

        //reached next star
        if (currentSubStarIndex >= currentStar.numOfSubstars) 
        {
            //Debug.Log("Reached new star");
            currentStarIndex++;
            currentSubStarIndex = 0;
            currentStarScore = 0;
            
            //if the player reached the last star, sets the post game star
            if (isInPostGame)
            {
                foreach (SubStar sub in currentStar.subStars)
                {
                    sub.enemyPool = postGameEnemyPools[UnityEngine.Random.Range(0, postGameEnemyPools.Length)];
                }
            }
        }

        reward = false;
    }

    private bool achievedNewStar = false;
    public bool AchievedNewStar()
    {
        if (achievedNewStar)
        {
            achievedNewStar = false;
            return true;
        }
        return false;
    }

    // private bool achievedNewSubStar = false;
    // public bool AchievedNewSubStar()
    // {
    //     if (achievedNewSubStar)
    //     {
    //         achievedNewSubStar = false;
    //         return true;
    //     }
    //     return false;
    // }

    // private bool achievedNewReward = false;
    // public bool AchievedNewReward()
    // {
    //     if (achievedNewReward)
    //     {
    //         achievedNewReward = false;
    //         return true;
    //     }
    //     return false;
    // }
}