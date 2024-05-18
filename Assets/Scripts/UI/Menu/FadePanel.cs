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

    private struct childText
    {
        public TMPro.TextMeshProUGUI text;
        public float alpha;

        public childText(TMPro.TextMeshProUGUI text, float alpha)
        {
            this.text = text;
            this.alpha = alpha;
        }
    }
    List<childImage> childImages = new List<childImage>();
    List<childText> childTexts = new List<childText>();

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();

        List<Image> _childImages = new List<Image>();
        Utilities.GetComponentsInAllChildren(transform,ref _childImages);
        
        childImages = new List<childImage>();

        foreach (Image image in _childImages)
        {
            childImages.Add(new childImage(image, image.color.a));
        }

        List<TMPro.TextMeshProUGUI> _childTexts = new List<TMPro.TextMeshProUGUI>();
        Utilities.GetComponentsInAllChildren(transform,ref _childTexts);

        childTexts = new List<childText>();

        foreach (TMPro.TextMeshProUGUI text in _childTexts)
        {
            childTexts.Add(new childText(text, text.color.a));
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

                Color tmp = childImage.image.color;
                tmp.a = (currentAlpha/maxAlpha) * childImage.alpha;
                childImage.image.color = tmp;
            }

            foreach (childText childText in childTexts)
            {
                Color tmp = childText.text.color;
                tmp.a = (currentAlpha/maxAlpha) * childText.alpha;
                childText.text.color = tmp;
            }

            if (currentAlpha == targetAlpha)
            {
                changingAlpha = false;
            }
        }

        if (needsToUpdateButtons)
        {
            foreach (childImage childImage in childImages)
            {

                childImage.image.color = new Color(childImage.image.color.r, 
                                                childImage.image.color.g, 
                                                childImage.image.color.b, 
                                                (currentAlpha/maxAlpha) * childImage.alpha);
            }
        }
    }

    override public void SetActive(bool active)
    {
        base.SetActive(active);
        
        changingAlpha = true;
        targetAlpha = active ? maxAlpha : 0f;
    }

    public override void AddButton(Button2D button)
    {
        base.AddButton(button);

        Image childImage = button.GetComponent<Image>();
        if (childImage != null)
        {
            childImages.Add(new childImage(childImage, childImage.color.a));
        }
    }

    public override void RemoveButton(Button2D button)
    {
        base.RemoveButton(button);

        Image childImage = button.GetComponent<Image>();
        if (childImage != null)
        {
            childImages.RemoveAll(x => x.image == childImage);
        }
    }
}
