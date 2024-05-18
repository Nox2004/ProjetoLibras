using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;

public class UpgradeSelection : MonoBehaviour
{
    [HideInInspector] public SignQuizEventManager quizManager;
    [SerializeField] private GameObject buttonExample;
    private Transform buttonParent;
    private Panel panel;

    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    private PlayerUpgrade selectedUpgrade;

    private List<UpgradeButton> upgradeButtons;

    private AudioManager audioManager;
    [SerializeField] private AudioClip selectingNoUpgrade;
    [SerializeField] private AudioClip selectingAUpgrade;

    void Start()
    {
        audioManager = Injector.GetAudioManager(gameObject);
        upgradeButtons = new List<UpgradeButton>();

        buttonParent = buttonExample.transform.parent;
        panel = GetComponent<Panel>();

        buttonExample.SetActive(false);
    }

    void Update()
    {
        
    }

    public void SelectUpgrade(PlayerUpgrade upgrade)
    {
        selectedUpgrade = upgrade;
        descriptionText.text = upgrade.description.translatedText;

        //Marks the button as selected
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            if (upgradeButtons[i].upgrade == upgrade) upgradeButtons[i].selected = true;
            else upgradeButtons[i].selected = false;
        }
    }

    public void ApplyUpgrade()
    {
        if (selectedUpgrade == null) 
        {
            audioManager.PlaySound(selectingNoUpgrade);
            return;
        }

        audioManager.PlaySound(selectingAUpgrade);
        quizManager.SelectUpgrade(selectedUpgrade.id);
    }

    public void SetButtons(PlayerUpgrade[] currentUpgradeSelection)
    {
        //resets description
        descriptionText.text = "";
        selectedUpgrade = null;

        //remove all existing buttons
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            panel.RemoveButton(upgradeButtons[i]);
            Destroy(upgradeButtons[i].gameObject);
        }

        upgradeButtons.Clear();
        int index = 0;

        //add buttons to select upgrade

        buttonExample.SetActive(true);
        
        foreach (PlayerUpgrade upgrade in currentUpgradeSelection)
        {
            GameObject button = Instantiate(buttonExample, buttonParent);
            button.GetComponent<Image>().color = Color.black;

            upgradeButtons.Add(button.GetComponent<UpgradeButton>());
            upgradeButtons[index].upgrade = upgrade;
            panel.AddButton(upgradeButtons[index]);

            index++;
        }

        buttonExample.SetActive(false);
    }
}