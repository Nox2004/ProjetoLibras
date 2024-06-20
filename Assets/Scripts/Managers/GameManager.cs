using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public static class GameManager
{
    private static GameSettings mainSettings;
    private static GameModeManager gameModeManager;

    private static string gameModeDataPath;
    
    #region //Getters and Setters

    public static GameMode[] GetGameModes() => gameModeManager.gameModes;

    public static GameMode GetGameMode(GameModeID id) => gameModeManager.GetGameMode(id);

    public static void UnlockGameMode(GameModeID id)
    {
        Debug.Log("Unlocking game mode: " + id);
        GameMode mode = GetGameMode(id);
        mode.unlocked = true;
        gameModeManager.SaveGameModeData();
    }

    public static void SetHighScore(GameModeID id, int highScore)
    {
        GameMode mode = GetGameMode(id);
        mode.highScore = highScore;
        gameModeManager.SaveGameModeData();
    }

    public static LanguageSettings[] GetLanguages() => mainSettings.languageSettings;

    public static bool GetMusicOn() => false;//mainSettings.musicOn;
    public static void SetMusicOn(bool musicOn)
    {
        mainSettings.musicOn = musicOn;
        MusicManager.Instance.UpdateVolume(musicOn ? 1 : 0);
        SaveSettings();
    }

    public static bool GetSoundsOn() => false;//mainSettings.soundsOn;
    public static void SetSoundsOn(bool soundsOn)
    {
        mainSettings.soundsOn = soundsOn;
        SaveSettings();
    }

    public static Language GetLanguage() => mainSettings.selectedLanguage;
    public static void SetLanguage(Language language)
    {
        mainSettings.selectedLanguage = language;
        UpdateSignSets();
        SaveSettings();
    }

    public static bool GetInvertedSignals() => mainSettings.invertedSignals;
    public static void SetInvertedSignals(bool inverted)
    {
        mainSettings.invertedSignals = inverted;
        UpdateSignSets();
        SaveSettings();
    }

    public static bool GetEffectsOn() => mainSettings.effectsOn;
    public static void SetEffectsOn(bool effectsOn)
    {
        mainSettings.effectsOn = effectsOn;
        SaveSettings();
    }

    #endregion

    public static void UpdateSignSets()
    {
        SignSetManager.signCodes = mainSettings.signsUsed;

        LanguageSettings _ls = mainSettings.GetLanguageSettings();

        if (!mainSettings.invertedSignals) 
        {
            SignSetManager.SetSignSets(_ls.signs, _ls.letters);
        }
        else
        {
            SignSetManager.SetSignSets(_ls.letters, _ls.signs);
        } 
    }

    #region //Save and Load Methods

    public static void SaveSettings()
    {
        //Save stuff to player prefs
        PlayerPrefs.SetInt("MusicOn", mainSettings.musicOn ? 1 : 0);
        PlayerPrefs.SetInt("SoundsOn", mainSettings.soundsOn ? 1 : 0);
        PlayerPrefs.SetInt("EffectsOn", mainSettings.effectsOn ? 1 : 0);
        PlayerPrefs.SetInt("SelectedLanguage", (int)mainSettings.selectedLanguage);
        PlayerPrefs.SetInt("InvertedSignals", mainSettings.invertedSignals ? 1 : 0);
    }

    public static void LoadSettings()
    {
        //Get stuff from player prefs
        mainSettings.musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        mainSettings.soundsOn = PlayerPrefs.GetInt("SoundsOn", 1) == 1;
        mainSettings.effectsOn = PlayerPrefs.GetInt("EffectsOn", 1) == 1;
        mainSettings.selectedLanguage = (Language)PlayerPrefs.GetInt("SelectedLanguage", 0);
        mainSettings.invertedSignals = PlayerPrefs.GetInt("InvertedSignals", 0) == 1;
    }

    #endregion

    static GameManager()
    {
        //Load Game Mode Data
        gameModeManager = Resources.Load<GameModeManager>("GameModes");
        gameModeManager.SetSaveFilePath(Application.persistentDataPath + "/gameModeSaveData.json");
        gameModeManager.LoadGameModeData();
        
        //Load Game Settings
        mainSettings = Resources.Load<GameSettings>("GameSettings");

        LoadSettings();

        MusicManager.Instance.UpdateVolume(mainSettings.musicOn ? 1 : 0);
        
        //Update Sign Sets
        UpdateSignSets();
    }
}
