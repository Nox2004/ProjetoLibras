using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public enum GameModeID
{
    Tutorial,
    Normal,
    Endless
}

[System.Serializable]
public class GameMode
{
    //Save data
    public GameModeID id;
    public bool unlocked;
    public int highScore;


    //Misceallaneous
    public Sprite icon;
    public string sceneName;
    public TranslatableText name;
    public TranslatableText hasToUnlockText;
}

[System.Serializable]
public struct GameModeSaveInstance
{   
    public int id;
    public bool unlocked;
    public int highScore;

    public GameModeSaveInstance(int id, bool unlocked, int highScore)
    {
        this.id = id;
        this.unlocked = unlocked;
        this.highScore = highScore;
    }
}

[System.Serializable]
public class GameModeSaveData
{
    public List<GameModeSaveInstance> data;

    public GameModeSaveData()
    {
        data = new List<GameModeSaveInstance>();
    }
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameModeSettings", order = 1)]
public class GameModeManager : ScriptableObject
{
    private string saveFilePath;
    [SerializeField] private GameMode[] _gameModes;
    public GameMode[] gameModes
    {
        get
        {
            return _gameModes;
        }
        set 
        {
            _gameModes = value; 
        }
    }

    public GameMode GetGameMode(GameModeID id)
    {
        for (int i = 0; i < gameModes.Length; i++)
        {
            if (gameModes[i].id == id)
            {
                return gameModes[i];
            }
        }
        Debug.LogError("GameMode not found");
        return gameModes[0];
    }

    public void SetSaveFilePath(string path)
    {
        saveFilePath = path;
    }
    
    public void SaveGameModeData()
    {
        GameModeSaveData gameModeSaveData = new GameModeSaveData();

        for (int i = 0; i < gameModes.Length; i++)
        {
            gameModeSaveData.data.Add(new GameModeSaveInstance((int) gameModes[i].id, gameModes[i].unlocked, gameModes[i].highScore));
        }
        string saveString = JsonUtility.ToJson(gameModeSaveData);
        
        File.WriteAllText(saveFilePath, saveString);
    }
  
    public void LoadGameModeData()
    {
        if (File.Exists(saveFilePath))
        {
            string loadData = File.ReadAllText(saveFilePath);
            GameModeSaveData gameModeSaveData = JsonUtility.FromJson<GameModeSaveData>(loadData);
  
            for (int i = 0; i < gameModeSaveData.data.Count; i++)
            {
                for (int j = 0; j < gameModes.Length; j++)
                {
                    if ((GameModeID) gameModeSaveData.data[i].id == gameModes[j].id)
                    {
                        gameModes[j].unlocked = gameModeSaveData.data[i].unlocked;
                        gameModes[j].highScore = gameModeSaveData.data[i].highScore;
                    }
                }
            }
        }
        else
        {

        }
            //There is no save files to load
  
    }
  
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
  
            Debug.Log("Save file deleted!");
        }
        else
            Debug.Log("There is nothing to delete!");
    }
}