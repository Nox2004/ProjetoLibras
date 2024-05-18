using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdPanel : MonoBehaviour
{
    public SeeAdToUnlockButton lockButton;
    private Panel panel;

    void Start()
    {
        panel = GetComponent<Panel>();
    }

    public void SetPanelActive()
    {
        panel.SetActive(true);
    }
    
    public void SeeAd()
    {
        lockButton.ReturnFromAdScreen();
        panel.SetActive(false);

        //TODO: Implement ad watching
        if (true) //if watched to the end
        {
            lockButton.SawAdd();
        }
    }

    public void NotSeeAd()
    {
        lockButton.ReturnFromAdScreen();
        panel.SetActive(false);   
    }
}