using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarProgressionBar : MonoBehaviour
{
    [Header("References")]
    //Star system
    [SerializeField] private LevelManager levelManager;
    private StartProgressionSystem starProgressionSystem;

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
    private float fillAmmount;
    private float lastFillAmmount;

    private float lastFrameProgression;

    //Last frame values
    private int lastStarIndex = 0;
    private bool lastReward = false;
    private int lastSubstarIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        progressBarRect = progressBar.GetComponent<RectTransform>();

        progressBarImage = progressBar.GetComponent<Image>();
        progressBarOutlineImage = progressBarOutline.GetComponent<Image>();
        
        progressBarImage.color = barColor;
        progressBarOutlineImage.color = outlineColor;

        starProgressionSystem = levelManager.starProgressionSystem;
        fillAmmount = starProgressionSystem.currentStarProgression;
        lastFillAmmount = fillAmmount;

        UpdateSubstars();

        lastStarIndex = starProgressionSystem.currentStarIndex;
        lastReward = starProgressionSystem.ReadyForReward();
        lastSubstarIndex = starProgressionSystem.currentSubStarIndex;
    }

    // Update is called once per frame
    void Update()
    {
        fillAmmount += (starProgressionSystem.currentStarProgression-fillAmmount) / (fillingSmoothRatio / Time.deltaTime);
        progressBarRect.localScale = new Vector3(fillAmmount, 1, 1);
        
        if (lastFrameProgression != starProgressionSystem.currentStarProgression)
        {
            lastFillAmmount = fillAmmount;
        }
        lastFrameProgression = starProgressionSystem.currentStarProgression;

        float lerpValue = Mathf.Abs(fillAmmount-starProgressionSystem.currentStarProgression) / Mathf.Abs(lastFillAmmount-starProgressionSystem.currentStarProgression);
        progressBarImage.color = Color.Lerp(barColor, barColorFilling, lerpValue);


        Vector3 tmp = progressMarker.localPosition;
        tmp.x = (-rectTransform.rect.width / 2) + rectTransform.rect.width * fillAmmount;
        progressMarker.localPosition = tmp; 

        if (lastStarIndex != starProgressionSystem.currentStarIndex)
        {
            UpdateSubstars();
            lastSubstarIndex = starProgressionSystem.currentSubStarIndex;
            
            if (starProgressionSystem.isInPostGame)
            {
                //sets up post game level text

            }
            //start new star animation
        }
        if (lastReward != starProgressionSystem.ReadyForReward())
        {
            //start reward animation
        }
        if (lastSubstarIndex != starProgressionSystem.currentSubStarIndex)
        {
            //start fill substar animation
        }

        lastStarIndex = starProgressionSystem.currentStarIndex;
        lastReward = starProgressionSystem.ReadyForReward();
        lastSubstarIndex = starProgressionSystem.currentSubStarIndex;
    }

    void UpdateSubstars()
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

        stars = new GameObject[starProgressionSystem.currentNumOfSubstars];

        //create stars using star system sub stars
        for (int i = 0; i < starProgressionSystem.currentNumOfSubstars; i++)
        {
            float width = rectTransform.rect.width;

            GameObject star = Instantiate(starExample, starParent.transform);
            
            //Set position
            Vector3 tmp = star.transform.localPosition;
            tmp.x = (-width / 2) + (width / starProgressionSystem.currentNumOfSubstars) * (i+1f);
            star.transform.localPosition = tmp;

            //Set sprite
            Sprite sprite = starProgressionSystem.currentStar.subStars[i].reward.rewardIcon;
            star.GetComponent<Image>().sprite = sprite;
            
            //Set size to match sprites
            star.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.texture.width / (sprite.pixelsPerUnit / 100), sprite.texture.height / (sprite.pixelsPerUnit / 100));

            if (starProgressionSystem.isInPostGame && i == starProgressionSystem.currentNumOfSubstars-1)
            {
                //set star to be filled
                star.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = starProgressionSystem.postGameLevel.ToString();
            }

            stars[i] = star;
        }
        

        starExample.SetActive(false);
    }
}
