using System;
using UnityEngine;

public class TMProTranslatableText : MonoBehaviour
{
    [SerializeField] private TranslatableText text;
    private TMPro.TextMeshProUGUI textComponent;

    void Start()
    {
        textComponent = GetComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = text.translatedText;
    }
}