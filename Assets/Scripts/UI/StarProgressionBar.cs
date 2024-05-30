using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarProgressionBar : MonoBehaviour
{
    [Header("References")]
    //Star system
    [SerializeField] private LevelManager levelManager;

    [SerializeField] private RectTransform progressMarker; //Marker that shows the current progression

    //Progress bar fill
    [SerializeField] private GameObject progressBar; 
    private Image progressBarImage;
    private RectTransform progressBarRect;

    //Progress bar outline
    [SerializeField] private GameObject progressBarOutline;
    private Image progressBarOutlineImage;

    //My rect
    private RectTransform rectTransform;

    [Header("Stars")]
    [SerializeField] private GameObject starExample;
    private GameObject starParent;
    private GameObject[] stars;

    [Header("Bar properties")]
    [SerializeField] private Color outlineColor;
    [SerializeField] private Color barColor;
    [SerializeField] private Color barColorFilling;
    [SerializeField] private float fillingSmoothRatio;
    //[SerializeField] [Range(0,1)] private float changingColorTreshold;
    private float targetFillAmmount;
    private float fillAmmount;
    private float lastFillAmmount;


    //Last frame values
    private Star lastStar;
    private bool lastReward = false;
    private int lastSubstarIndex = 0;
    
    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        progressBarRect = progressBar.GetComponent<RectTransform>();

        progressBarImage = progressBar.GetComponent<Image>();
        progressBarOutlineImage = progressBarOutline.GetComponent<Image>();
        
        progressBarImage.color = barColor;
        progressBarOutlineImage.color = outlineColor;
        
        fillAmmount = targetFillAmmount;
        lastFillAmmount = fillAmmount;
    }

    public void UpdateBar(float completion)
    {
        targetFillAmmount = completion;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        fillAmmount += (targetFillAmmount-fillAmmount) / (fillingSmoothRatio / Time.deltaTime);
        progressBarRect.localScale = new Vector3(fillAmmount, 1, 1);

        float lerpValue = Mathf.Abs(fillAmmount-targetFillAmmount) / Mathf.Abs(lastFillAmmount-targetFillAmmount);
        progressBarImage.color = Color.Lerp(barColor, barColorFilling, lerpValue);

        Vector3 tmp = progressMarker.localPosition;
        tmp.x = (-rectTransform.rect.width / 2) + rectTransform.rect.width * fillAmmount;
        progressMarker.localPosition = tmp; 

        //Normal mode particularities
        // if (levelManager is NormalModeLevelManager)
        // {
        //     NormalModeLevelManager lm = levelManager as NormalModeLevelManager;

        //     if (lm.isInPostGame)
        //     {
        //         //set star to be filled
        //         stars[lm.currentNumOfSubstars-1].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = lm.postGameLevel.ToString();
        //     }
        // }

        // //Endless mode particularities
        // if (levelManager is EndlessModeLevelManager)
        // {
        //     EndlessModeLevelManager lm = levelManager as EndlessModeLevelManager;

        //     stars[lm.currentNumOfSubstars-1].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (lm.score+1).ToString();
        // }
    }

    public void UpdateSubstars(Star star, string lastRewardText = "")
    {
        starExample.SetActive(true);

        if (starParent == null)
        {
            starParent = starExample.transform.parent.gameObject;
        }

        //destroy all stars
        foreach (Transform child in starParent.transform)
        {
            if (child == starExample.transform) continue;
            Destroy(child.gameObject);
        }

        stars = new GameObject[star.numOfSubstars];

        //create stars using star system sub stars
        for (int i = 0; i < star.numOfSubstars; i++)
        {
            float width = rectTransform.rect.width;

            GameObject starObj = Instantiate(starExample, starParent.transform);
            
            //Set position
            Vector3 tmp = starObj.transform.localPosition;
            tmp.x = (-width / 2) + (width / star.numOfSubstars) * (i+1f);
            starObj.transform.localPosition = tmp;

            //Set sprite
            Sprite sprite = star.subStars[i].reward.rewardIcon;
            starObj.GetComponent<Image>().sprite = sprite;
            
            //Set size to match sprites
            starObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.texture.width / (sprite.pixelsPerUnit / 100), sprite.texture.height / (sprite.pixelsPerUnit / 100));

            if (i == star.numOfSubstars-1)
            {
                starObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = lastRewardText;
            }

            stars[i] = starObj;
        }

        starExample.SetActive(false);
    }
}
