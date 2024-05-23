using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEvents : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private MainMenu3DButton[] mainMenuButtons;

    [Header("Sign Catalogue Component")]
    [SerializeField] private SignCatalogue signCatalogue;

    [Header("Menu Panels")]
    [SerializeField] private Panel modeSelectionPanel;
    [SerializeField] private Panel signsPanel;
    [SerializeField] private Panel settingsPanel;
    private Panel currentPanel = null;

    [SerializeField] private StarHighScore highScoreElement;

    [Header("Transitions")]
    [SerializeField] private GameObject transitionPrefab;

    [Header("Settings Elements")]
    [SerializeField] private Toggle2D musicOnButton;
    [SerializeField] private Toggle2D soundsOnButton;
    [SerializeField] private Toggle2D effectsOnButton;
    [SerializeField] private Toggle2D invertedSignalsButton;
    
    private void Start()
    {
        musicOnButton.SetValue(GameManager.GetMusicOn());
        soundsOnButton.SetValue(GameManager.GetSoundsOn());
        effectsOnButton.SetValue(GameManager.GetEffectsOn());
        invertedSignalsButton.SetValue(GameManager.GetInvertedSignals());
    }

    public void PlayButton()
    {
        modeSelectionPanel.SetActive(true);
        currentPanel = modeSelectionPanel;

        foreach (var button in mainMenuButtons)
        {
            button.Hide();
        }
    }

    public void ShowSignsButton()
    {
        signsPanel.SetActive(true);
        currentPanel = signsPanel;

        foreach (var button in mainMenuButtons)
        {
            button.Hide();
        }
    }

    public void SettingsButton()
    {
        settingsPanel.SetActive(true);
        currentPanel = settingsPanel;

        foreach (var button in mainMenuButtons)
        {
            button.Hide();
        }
    }

    public void GoBackToMainMenuButton()
    {
        if (currentPanel == null) return;

        currentPanel.SetActive(false);
        currentPanel = null;

        foreach (var button in mainMenuButtons)
        {
            button.Show();
        }
    }

    public void SelectMode(string modeSceneName)
    {
        GameObject transition_obj = Instantiate(transitionPrefab);
        transition_obj.GetComponent<Transition>().targetSceneName = modeSceneName;
    }

    public void SetMusicOn(bool value)
    {
        //MusicManager.Instance.UpdateVolume(value ? 1 : 0);
        GameManager.SetMusicOn(value);
    }

    public void SetSoundsOn(bool value)
    {
        GameManager.SetSoundsOn(value);
    }

    public void SetEffectsOn(bool value)
    {
        GameManager.SetEffectsOn(value);
    }

    public void SetInvertedSignals(bool value)
    {
        GameManager.SetInvertedSignals(value);
        signCatalogue.UpdateCatalogue();
    }
}
