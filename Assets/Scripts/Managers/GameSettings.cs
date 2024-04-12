using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Language
{
    PortugueseBR,
    English
}

[System.Serializable]
public struct LanguageSettings
{
    public Language language;
    public Sprite flagSprite;
    public SignSet signs; 
    public SignSet letters;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    //Not changed by the player
    public List<SignCode> signsUsed;
    public LanguageSettings[] languageSettings;

    //Gameplay
    public Language selectedLanguage;
    public bool invertedSignals;

    //private bool MusicOn, SoundsOn, SelectedLanguage, InvertedSignals, EffectsOn, OutlineWidth;

    //Audio
    public bool musicOn;
    public bool soundsOn;

    //Graphics
    public bool effectsOn;
    public float outlineWidth;

    public LanguageSettings GetLanguageSettings()
    {
        foreach (LanguageSettings languageSetting in languageSettings)
        {
            if (languageSetting.language == selectedLanguage)
            {
                return languageSetting;
            }
        }
        Debug.LogError("!!!!Language not found");
        return languageSettings[0];
    }
}