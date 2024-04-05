using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SettingsManager
{
    public static GameSettings mainSettings;
    
    public static void UpdateLanguage(Language language)
    { 
        mainSettings.selectedLanguage = language;
    }

    public static void UpdateInvertedSignals(bool inverted)
    {
        mainSettings.invertedSignals = inverted;
    }

    public static void SetSignSets()
    {
        SignSetManager.signCodes = mainSettings.signsUsed;

        LanguageSettings _ls = mainSettings.GetLanguageSettings();

        if (!mainSettings.invertedSignals) 
        {
            SignSetManager.SetSignSets(_ls.letters, _ls.signs);
        }
        else
        {
            SignSetManager.SetSignSets(_ls.signs, _ls.letters);
        } 
    }

    static SettingsManager()
    {
        Debug.Log("SettingsManager initialized");
        mainSettings = Resources.Load<GameSettings>("GameSettings");
    }
}
