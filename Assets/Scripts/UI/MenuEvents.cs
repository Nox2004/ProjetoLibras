using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEvents : MonoBehaviour
{
    private ChangeSceneManager sceneManager = Injector.GetSceneManager();
    [SerializeField] private MainMenu3DButton[] mainMenuButtons;

    [SerializeField] private MenuPanel modeSelectionPanel, signsPanel, settingsPanel;
    private MenuPanel currentPanel = null;

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
        sceneManager.LoadScene(modeSceneName);
    }
}
