using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SignCatalogue : MonoBehaviour
{
    [SerializeField] GameObject signBase;

    private VerticalLayoutGroup verticalLayoutGroup;
    private float touchStartY;
    private float initialTop = 0, top = 0;
    private float minLimit = 0;

    private RectTransform rectTransform;

    void Start()
    {
        verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        top = verticalLayoutGroup.padding.top;
        initialTop = top;

        foreach (SignCode code in SignSetManager.signCodes)
        {
            GameObject sign = Instantiate(signBase, transform);
            //sign.GetComponent<SignCatalogueItem>().SetSign(code);
            Image[] images = sign.GetComponentsInChildren<Image>();
            
            Texture sourceTex = SignSetManager.GetSoureSign(code).signTexture;
            Sprite sourceSprite = Sprite.Create((Texture2D) sourceTex, new Rect(0, 0, sourceTex.width, sourceTex.height), new Vector2(0.5f, 0.5f));
            images[0].sprite = sourceSprite;

            Texture targetTex = SignSetManager.GetTargetSign(code).signTexture;
            Sprite targetSprite = Sprite.Create((Texture2D) targetTex, new Rect(0, 0, targetTex.width, targetTex.height), new Vector2(0.5f, 0.5f));
            images[1].sprite = targetSprite;
        }

        Destroy(signBase);

        foreach (Transform child in transform)
        {
            minLimit += child.GetComponent<RectTransform>().rect.height;
            minLimit += verticalLayoutGroup.spacing;
        }
        minLimit -= rectTransform.rect.height;
        minLimit -= initialTop;
    }
    
    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touch.position))
            {
                if (touch.phase == TouchPhase.Began) touchStartY = touch.position.y;
                
                float deltaY = touch.position.y - touchStartY;
                top -= deltaY/5;
                touchStartY = touch.position.y;
            }

            top = Mathf.Clamp(top, initialTop - minLimit, initialTop);

            verticalLayoutGroup.padding.top = (int)top;
            
            verticalLayoutGroup.CalculateLayoutInputVertical(); //apply change to vertical layout group
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}
