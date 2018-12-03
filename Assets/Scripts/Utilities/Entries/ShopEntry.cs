using UnityEngine.UI;
using UnityEngine;

public class ShopEntry : MonoBehaviour {
    public int itemId;
    public string itemName;
    [TextArea]
    public string itemDescription;
    public float itemPrice;
    [Space]
    public Text itemNameText;
    public Text itemDescriptionText, itemPriceText;

    public void Buy()
    {
        GameManager.BuyItem(this);
    }
}
