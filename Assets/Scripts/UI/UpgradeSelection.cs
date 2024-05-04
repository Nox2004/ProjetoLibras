using UnityEngine;
using UnityEngine.UI;
using System;

public class UpgradeSelection : MonoBehaviour
{
    [SerializeField] private GameObject buttonExample;
    private Panel panel;

    void Start()
    {
        panel = GetComponent<Panel>();
    }

    public void SetButtons(PlayerUpgrade[] currentUpgradeSelection)
    {
        //remove all buttons
        foreach (Transform child in transform)
        {
            if (child == buttonExample.transform) continue;
            panel.RemoveButton(child.GetComponent<Button2D>());
            Destroy(child.gameObject);
        }

        buttonExample.SetActive(true);

        //add buttons to select upgrade
        foreach (PlayerUpgrade upgrade in currentUpgradeSelection)
        {
            GameObject button = Instantiate(buttonExample, transform);
            button.GetComponent<Image>().sprite = upgrade.icon;
            button.GetComponent<Image>().color = Color.white;
            button.GetComponent<UpgradeButton>().status = upgrade.id;

            panel.AddButton(button.GetComponent<Button2D>());
        }

        buttonExample.SetActive(false);
    }
}