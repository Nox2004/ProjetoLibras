using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public class FadePanel : Panel
{
    private float currentAlpha, targetAlpha;

    [SerializeField] private float maxAlpha, alphaSmoothRatio;
    [SerializeField] private float deltaAlphaTreshold;

    private bool changingAlpha = false;
    
    private struct childImage
    {
        public Image image;
        public float alpha;

        public childImage(Image image, float alpha)
        {
            this.image = image;
            this.alpha = alpha;
        }
    }
    List<childImage> childImages = new List<childImage>();

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();

        childImages = new List<childImage>();

        foreach (Transform child in transform)
        {
            Image childImage = child.gameObject.GetComponent<Image>();
            if (childImage != null)
            {
                childImages.Add(new childImage(childImage, childImage.color.a));
            }
        }        

        if (active) 
        {
            currentAlpha = maxAlpha;
            targetAlpha = maxAlpha;
        }
        else
        {
            currentAlpha = 0f;
            targetAlpha = 0f;
        }
    }

    override protected void Update()
    {
        base.Update();
        
        if (changingAlpha)
        {
            float deltaAlpha = (targetAlpha - currentAlpha) / (alphaSmoothRatio / Time.deltaTime);
            if (Mathf.Abs(deltaAlpha) <= deltaAlphaTreshold * Time.deltaTime)
            {
                deltaAlpha = (deltaAlphaTreshold * Time.deltaTime) * Mathf.Sign(deltaAlpha);
            }

            currentAlpha += deltaAlpha;
            currentAlpha = Mathf.Clamp(currentAlpha, 0f, maxAlpha);

            image.color = new Color(image.color.r, image.color.g, image.color.b, currentAlpha);

            foreach (childImage childImage in childImages)
            {

                childImage.image.color = new Color(childImage.image.color.r, 
                                                childImage.image.color.g, 
                                                childImage.image.color.b, 
                                                (currentAlpha/maxAlpha) * childImage.alpha);
            }

            if (currentAlpha == targetAlpha)
            {
                changingAlpha = false;
            }
        }
    

    }

    override public void SetActive(bool active)
    {
        base.SetActive(active);
        
        changingAlpha = true;
        targetAlpha = active ? maxAlpha : 0f;
    }
}
