using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StarProgressionSystem", menuName = "Star System/Progression System/Normal Mode", order = 1)]
public class NormalModeStarProgressionSystem : StarProgressionSystem
{
    //post game stuff
    [SerializeField] private Star postGameStar;
    public bool isInPostGame { get { return currentStarIndex >= stars.Length; } }
    public int postGameLevel { get { return (currentStarIndex - stars.Length)+1; } } 

    public EnemyPool[] postGameEnemyPools;

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

    public override void ResolveReward(bool rightAnswer)
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

    // public StarProgressionSystem()
    // {
    //     // if (isInPostGame)
    //     // {
    //     //     foreach (SubStar sub in currentStar.subStars)
    //     //     {
    //     //         sub.enemyPool = postGameEnemyPools[UnityEngine.Random.Range(0, postGameEnemyPools.Length)];
    //     //     }
    //     // }
    // }
}