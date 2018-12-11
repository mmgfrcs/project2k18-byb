using UnityEngine.UI;
using UnityEngine;

public class ShopEntry : MonoBehaviour {
    public int itemId;
    public string itemName;
    [TextArea]
    public string itemDescription;
    public float itemPrice;
    [Header("Requirements")]
    public int requiredLevel = 1;
    [Space]
    public Text itemNameText;
    public Text itemDescriptionText, itemPriceText;

    internal bool ItemBought { get; private set; }

    private void Start()
    {
        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
        itemPriceText.text = string.Format("${0:N0}", itemPrice);
    }

    public void Buy()
    {
        ItemBought = true;
        GameManager.BuyItem(this);
    }
}
