using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarProgressionBar : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField] private RectTransform progressMarker;
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private GameObject starExample;
    private GameObject starParent;
    private GameObject[] stars;

    [SerializeField] private LevelManager levelManager;
    private StartProgressionSystem starProgressionSystem;

    
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        starProgressionSystem = levelManager.starProgressionSystem;

        UpdateSubstars();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(starProgressionSystem.currentStarScore + " / " + starProgressionSystem.currentStar.pointsRequired);
        //Debug.Log(starProgressionSystem.currentStarProgression);
        progressBar.localScale = new Vector3(starProgressionSystem.currentStarProgression, 1, 1);

        Vector3 tmp = progressMarker.localPosition;
        tmp.x = (-rectTransform.rect.width / 2) + rectTransform.rect.width * starProgressionSystem.currentStarProgression;
        progressMarker.localPosition = tmp; 
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
        }
        

        starExample.SetActive(false);
    }
}
