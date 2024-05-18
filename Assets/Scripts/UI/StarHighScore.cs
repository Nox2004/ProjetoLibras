using UnityEngine;
using System;
using UnityEngine.UI;

public class StarHighScore : MonoBehaviour
{
    [SerializeField] private GameObject starExample;
    private Transform starParent;

    private GameObject[] stars = new GameObject[0];

    [SerializeField] private GameObject postGameLevelObject;
    [SerializeField] private TMPro.TextMeshProUGUI postGameLevelText;

    void Start()
    {
        starParent = starExample.transform.parent;
        fill();
    }
    
    public void fill()
    {
        starExample.SetActive(true);
        postGameLevelObject.SetActive(false);

        for (int i = 0; i < stars.Length; i++)
        {
            Destroy(stars[i]);
        }

        stars = new GameObject[GameManager.maxStarsAchieved];
        for (int i = 0; i < GameManager.maxStarsAchieved; i++)
        {
            GameObject star = Instantiate(starExample, starParent);
            stars[i] = star;
        }

        starExample.SetActive(false);

        if (GameManager.maxPostGameScore > 0)
        {
            postGameLevelObject.SetActive(true);
            postGameLevelText.text = GameManager.maxPostGameScore.ToString();

            //set post game level object on hierarchy
            postGameLevelObject.transform.SetAsLastSibling();
        }
    }
}