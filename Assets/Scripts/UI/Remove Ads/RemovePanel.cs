using UnityEngine;
using UnityEngine.Purchasing;

public class RemovePanel : MonoBehaviour
{
    public RemoveAdsToUnlockButton lockButton;
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

    public void BuyProduct(Product product)
    {
        MonetizationManager.Instance.monetization.PurchaseCompleted(product);
        
        lockButton.ReturnFromShopScreen();
        panel.SetActive(false);
    }

    public void DidntBuyProduct()
    {
        lockButton.ReturnFromShopScreen();
        panel.SetActive(false);
    }

}
