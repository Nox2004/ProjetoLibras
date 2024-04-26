using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSetting : MonoBehaviour
{
    private LanguageSettings[] languageSettings;
    private int selectedLanguageIndex = 0;
    [SerializeField] private SignCatalogue signCatalogue;

    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();

        languageSettings = GameManager.GetLanguages();

        //set the selected language
        for (int i = 0; i < languageSettings.Length; i++)
        {
            if (languageSettings[i].language == GameManager.GetLanguage())
            {
                selectedLanguageIndex = i;
                image.sprite = languageSettings[i].flagSprite;
                break;
            }
        }
    }

    public void SelectNextLanguage()
    {
        selectedLanguageIndex++;
        if (selectedLanguageIndex >= languageSettings.Length) selectedLanguageIndex = 0;

        image.sprite = languageSettings[selectedLanguageIndex].flagSprite;
        GameManager.SetLanguage(languageSettings[selectedLanguageIndex].language);

        signCatalogue.UpdateCatalogue();
    }
}
