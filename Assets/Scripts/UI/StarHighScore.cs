using UnityEngine;
using System;
using UnityEngine.UI;

public class StarHighScore : MonoBehaviour
{
    [SerializeField] private GameObject starExample;
    private Transform starParent;

    private GameObject[] stars = new GameObject[0];

    [Header("Tutorial")]
    [SerializeField] private Sprite tutorialCompletedIcon;

    [Header("Normal mode")]
    [SerializeField] private int numberOfStarsInNormalMode;
    [SerializeField] private Sprite normalModeStar;
    [SerializeField] private Sprite normalModePostGameStar;

    [Header("Endless mode")]
    [SerializeField] private Sprite endlessModeStar;

    void Start()
    {
        starParent = starExample.transform.parent;
    }
    
    public void fill(GameModeID gameModeID)
    {
        //Get game mode
        GameMode gamemode = GameManager.GetGameMode(gameModeID);

        //Reset stars
        starExample.SetActive(true);

        for (int i = 0; i < stars.Length; i++)
        {
            Destroy(stars[i]);
        }

        //Procceds with customized star filling for each game mode
        switch (gameModeID)
        {
            case GameModeID.Tutorial:
            {
                if (gamemode.highScore > 0)
                {
                    GameObject star = Instantiate(starExample, starParent);
                    star.GetComponent<Image>().sprite = tutorialCompletedIcon;
                    stars = new GameObject[1];
                    stars[0] = star;
                }
            }
            break;
            case GameModeID.Normal:
            {
                int numOfStars = Math.Min(numberOfStarsInNormalMode, gamemode.highScore);
                int postGameLevel = gamemode.highScore - numOfStars;

                //Create stars
                stars = new GameObject[numOfStars+Mathf.Min(1,postGameLevel)];

                GameObject star;

                for (int i = 0; i < numOfStars; i++)
                {
                    star = Instantiate(starExample, starParent);
                    star.GetComponent<Image>().sprite = normalModeStar;
                    stars[i] = star;
                }
                
                if (postGameLevel > 0)
                {
                    star = Instantiate(starExample, starParent);
                    star.GetComponent<Image>().sprite = normalModePostGameStar;
                    star.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = postGameLevel.ToString();
                    stars[numOfStars] = star;
                }
            }
            break;
            case GameModeID.Endless:
            {
                if (gamemode.highScore > 0)
                {
                    GameObject star = Instantiate(starExample, starParent);
                    star.GetComponent<Image>().sprite = tutorialCompletedIcon;
                    star.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = gamemode.highScore.ToString();
                    stars = new GameObject[1];
                    stars[0] = star;
                }
            }
            break;
        }
        
        starExample.SetActive(false);
    }
}