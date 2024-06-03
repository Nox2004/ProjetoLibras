using UnityEngine;

public class RemovePanel : MonoBehaviour
{
    public RemoveAds lockButton;
    private Panel panel;
    public static bool purchased = false;

    void Start()
    {
        panel = GetComponent<Panel>();
        if (MonetizationManager.Instance.monetization.HasPurchased("removeads"))
        {
            AdRemove();
        }
    }

    private void Update()
    {
        if (purchased)
        {
            AdRemove();
            purchased = false;
        }
    }

    public void SetPanelActive()
    {
        panel.SetActive(true);
    }

    public void AdRemove()
    {
        lockButton.BoughtAdRemove();
    }

    public void BuyProduct()
    {
        MonetizationManager manager = MonetizationManager.Instance;
        manager.monetization.storeController.InitiatePurchase(manager.monetization.sItem.ID);

        lockButton.ReturnFromShopScreen();
        panel.SetActive(false);
    }

    public void DidntBuyProduct()
    {
        lockButton.ReturnFromShopScreen();
        panel.SetActive(false);
    }

}
